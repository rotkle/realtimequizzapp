using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using QuizzApp.Configurations;
using QuizzApp.Hub;
using QuizzApp.MockQuizzData;
using QuizzApp.Share;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace QuizzApp.Services
{
    public class Quizz
    {
        private static readonly TimeSpan _gameTransitionDelay = TimeSpan.FromSeconds(5);

        private readonly QuizzOptions _options;
        private readonly TimeSpan _serverTimeout;

        // Injected dependencies
        private readonly IHubContext<QuizzHub, IQuizzPlayer> _hubContext;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;

        // Player state keyed by connection id
        private readonly ConcurrentDictionary<string, PlayerState> _players = new();

        // Notification when the quizz is completed
        private readonly CancellationTokenSource _completedCts = new();

        // Number of open player slots in this quizz
        private readonly Channel<int> _playerSlots;

        public Quizz(IHubContext<QuizzHub, IQuizzPlayer> hubContext,
                    IHttpClientFactory httpClientFactory,
                    ILogger<Quizz> logger,
                    IOptionsMonitor<QuizzOptions> options)
        {
            _hubContext = hubContext;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _options = options.CurrentValue;
            _playerSlots = Channel.CreateBounded<int>(_options.MaxPlayersPerQuizz);

            Name = RandomNameGenerator.GenerateRandomName();
            Group = hubContext.Clients.Group(Name);

            // Give the client some buffer
            _serverTimeout = TimeSpan.FromSeconds(_options.TimePerQuestion + 5);

            // Fill the slots for this quizz
            for (int i = 0; i < _options.MaxPlayersPerQuizz; i++)
            {
                _playerSlots.Writer.TryWrite(0);
            }
        }

        public string Name { get; }

        private IQuizzPlayer Group { get; }

        public CancellationToken Completed => _completedCts.Token;

        public async Task<bool> AddPlayerAsync(string connectionId)
        {
            // Try to grab a player slot
            if (_playerSlots.Reader.TryRead(out _))
            {
                // We succeeded so set up this player
                _players.TryAdd(connectionId, new PlayerState
                {
                    Proxy = _hubContext.Clients.Client(connectionId)
                });

                await _hubContext.Groups.AddToGroupAsync(connectionId, Name);

                await _hubContext.Clients.GroupExcept(Name, connectionId).WriteMessage($"A new player joined quizz {Name}");

                var waitingForPlayers = true;

                // If we don't have any more slots, it means we're full, lets start the quizz.
                if (!_playerSlots.Reader.TryPeek(out _))
                {
                    // Complete the channel so players can no longer join the quizz
                    _playerSlots.Writer.TryComplete();

                    // Check to see any slots were given back from players that might have dropped from the quizz while waiting on the quizz to start.
                    // We check this after TryComplete since it means no new players can join.
                    if (!_playerSlots.Reader.TryPeek(out _))
                    {
                        waitingForPlayers = false;

                        // We're clear, start the quizz
                        _ = Task.Run(PlayQuizz);
                    }

                    // More players can join, let's wait
                }

                if (waitingForPlayers)
                {
                    await Group.WriteMessage($"Waiting for {_playerSlots.Reader.Count} player(s) to join.");
                }

                return true;
            }

            return false;
        }

        public async Task RemovePlayerAsync(string connectionId)
        {
            // This should never be false, since we only remove players from quizzes they are associated with
            if (_players.TryRemove(connectionId, out _))
            {
                // If the quizz hasn't started (the channel was completed for e.g.), we can give this slot back to the quizz.
                _playerSlots.Writer.TryWrite(0);

                await Group.WriteMessage($"A player has left the quizz");
            }
        }

        // This method runs the entire quizz loop
        private async Task PlayQuizz()
        {
            // Ask the player a question until we get a valid answer
            static async Task<(PlayerState, QuizzAnswer?)> AskPlayerQuestion(PlayerState playerState, QuizzQuestion question, CancellationToken cancellationToken)
            {
                try
                {
                    var player = playerState.Proxy;

                    while (true)
                    {
                        // Ask the player this question and wait for the response
                        var answer = await player.AskQuestion(question, cancellationToken);

                        // If it's a valid choice, the return the answer
                        if (answer.Choice >= 0 && answer.Choice < question.Choices.Length)
                        {
                            await player.WriteMessage("Answer received. Waiting for other players to answer.");

                            return (playerState, answer);
                        }
                    }
                }
                catch
                {
                    // We don't want to throw exceptions when answers don't come back successfully.
                    return (playerState, null);
                }
            }

            var questionsPerQuizz = _options.QuestionsPerQuizz;
            var maxPlayersPerQuizz = _options.MaxPlayersPerQuizz;
            var timePerQuestion = _options.TimePerQuestion;

            // Did everyone rage quit the quizz? Then no point asking anymore questions
            // nobody can join mid-quizz.
            var emptyQuizz = false;

            // The per question cancellation token source
            var questionTimoutTokenSource = new CancellationTokenSource();

            try
            {
                // Get the mock questions for this quizz
                var client = _httpClientFactory.CreateClient();
                var quizzDataApi = new MockQuizzServiceApi(client);

                var quizzQuestions = await quizzDataApi.GetQuestionsAsync(questionsPerQuizz);

                var playerAnswers = new List<Task<(PlayerState, QuizzAnswer?)>>(maxPlayersPerQuizz);

                var configuration = new QuizzConfiguration
                {
                    NumberOfQuestions = quizzQuestions.Length,
                    QuestionTimeout = timePerQuestion
                };

                await Group.QuizzStarted(configuration);

                await Task.Delay(_gameTransitionDelay);

                var questionId = 0;
                foreach (var question in quizzQuestions)
                {
                    // Prepare the question to send to the client
                    var (quizzQuestion, indexOfCorrectAnswer) = CreateQuizzQuestion(question);

                    // Each question has a timeout (give the client some buffer before the server stops waiting for a reply)
                    questionTimoutTokenSource.CancelAfter(_serverTimeout);

                    // Clear the answers from the previous round
                    playerAnswers.Clear();

                    _logger.LogInformation("Asking question {QuestionId}", questionId);

                    // Ask the players the question concurrently
                    foreach (var (_, player) in _players)
                    {
                        playerAnswers.Add(AskPlayerQuestion(player, quizzQuestion, questionTimoutTokenSource.Token));
                    }

                    // Detect if all players exit the quizz. This is an optimization so we can clean up early.
                    emptyQuizz = playerAnswers.Count == 0;

                    if (emptyQuizz)
                    {
                        // Early exit if there are no players
                        break;
                    }

                    // Wait for all of the responses to come back (or timeouts).
                    await Task.WhenAll(playerAnswers);

                    if (!questionTimoutTokenSource.TryReset())
                    {
                        // We were unable to reset so make a new token
                        questionTimoutTokenSource = new();
                    }

                    _logger.LogInformation("Received all answers for question {QuestionId}", questionId);

                    // Observe the valid responses to questions
                    foreach (var (player, answer) in playerAnswers.Select(t => t.Result))
                    {
                        // Increment the correct answers for this player
                        if (answer?.Choice == indexOfCorrectAnswer)
                        {
                            player.Correct++;
                            await player.Proxy.WriteMessage($"{question.CorrectAnswer} is correct!");
                        }
                        else if (answer is not null)
                        {
                            await player.Proxy.WriteMessage($"That answer is incorrect! The correct answer is {question.CorrectAnswer}.");
                        }
                        else
                        {
                            await player.Proxy.WriteMessage($"The correct answer is {question.CorrectAnswer}.");
                        }
                    }

                    questionId++;

                    if (questionId < questionsPerQuizz)
                    {
                        // Tell each player that we're moving to the next question
                        await Group.WriteMessage($"Moving to the next question in {_gameTransitionDelay.TotalSeconds} seconds...");

                        await Task.Delay(_gameTransitionDelay);
                    }
                }

                if (!emptyQuizz)
                {
                    await Group.WriteMessage("Calculating scores...");

                    await Task.Delay(_gameTransitionDelay);

                    // Report the scores
                    foreach (var (_, player) in _players)
                    {
                        await player.Proxy.QuizzCompleted(new QuizzCompletedEvent
                        {
                            Name = Name,
                            Correct = player.Correct
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The processing for quizz {Name} failed unexpectedly", Name);

                await Group.WriteMessage($"The processing for quizz {Name} failed unexpectedly: {ex}");
            }
            finally
            {
                _logger.LogInformation("The quizz {Name} has run to completion.", Name);

                questionTimoutTokenSource.Dispose();

                // Signal that we're done
                _completedCts.Cancel();
            }
        }

        static (QuizzQuestion, int) CreateQuizzQuestion(QuizzDataQuestion question)
        {
            static void Shuffle<T>(T[] array)
            {
                // In-place Fisher-Yates shuffle
                for (int i = 0; i < array.Length - 1; ++i)
                {
                    int j = Random.Shared.Next(i, array.Length);
                    (array[j], array[i]) = (array[i], array[j]);
                }
            }

            // Copy the choices into an array and shuffle
            var choices = new string[question.IncorrectAnswers.Length + 1];
            choices[^1] = question.CorrectAnswer;
            for (int i = 0; i < question.IncorrectAnswers.Length; i++)
            {
                choices[i] = question.IncorrectAnswers[i];
            }

            // Shuffle the choices so it's randomly placed
            Shuffle(choices);

            var indexOfCorrectAnswer = choices.AsSpan().IndexOf(question.CorrectAnswer);
            var gameQuestion = new QuizzQuestion
            {
                Question = question.Question,
                Choices = choices
            };

            return (gameQuestion, indexOfCorrectAnswer);
        }

        private sealed class PlayerState
        {
            public IQuizzPlayer Proxy { get; init; }
            public int Correct { get; set; }
        }
    }
}
