using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace QuizzApp.Services
{
    public class QuizzLobby
    {
        private readonly IServiceProvider _serviceProvider;

        // FIFO queue of quizzes waiting to be played.
        private readonly ConcurrentQueue<Quizz> _waitingQuizzes = new();

        // The set of active or completed quizzes.
        private readonly ConcurrentDictionary<string, Quizz> _activeQuizzes = new();

        // The key into the per connection dictionary used to look up the current quizz;
        private static readonly object _quizzKey = new();

        public QuizzLobby(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<Quizz> AddPlayerToQuizzAsync(HubCallerContext hubCallerContext)
        {
            // There's already a quizz associated with this player, just return it
            if (hubCallerContext.Items[_quizzKey] is Quizz q)
            {
                return q;
            }

            while (true)
            {
                // Try to get a waiting quizz from the queue (the longest waiting quizz is served first FIFO)
                if (_waitingQuizzes.TryPeek(out var quizz))
                {
                    // Try to add the player to this quizz. It'll return false if the quizz is full.
                    if (!await quizz.AddPlayerAsync(hubCallerContext.ConnectionId))
                    {
                        // We're unable to use this waiting quizz, so make it an active quizz.
                        if (_activeQuizzes.TryAdd(quizz.Name, quizz))
                        {
                            // Remove the quizz when it completes
                            quizz.Completed.UnsafeRegister(_ =>
                            {
                                _activeQuizzes.TryRemove(quizz.Name, out var _);
                            },
                            null);

                            // Remove it from the list of waiting quizzes after we've made it active
                            _waitingQuizzes.TryDequeue(out _);
                        }

                        continue;
                    }
                    else
                    {
                        // Store the association of this player to this quizz
                        hubCallerContext.Items[_quizzKey] = quizz;

                        // When the player disconnects, remove them from the quizz
                        hubCallerContext.ConnectionAborted.Register(() =>
                        {
                            // We can't wait here (since this is synchronous), so fire and forget
                            _ = quizz.RemovePlayerAsync(hubCallerContext.ConnectionId);
                        });

                        // When the quizz ends, remove the quizz from the player (they can join another quizz)
                        quizz.Completed.Register(() => hubCallerContext.Items.Remove(_quizzKey));
                    }

                    return quizz;
                }

                // This works because quizzes are transient so a new one gets created
                // when it is requested
                var newQuizz = _serviceProvider.GetRequiredService<Quizz>();

                _waitingQuizzes.Enqueue(newQuizz);
            }
        }
    }
}
