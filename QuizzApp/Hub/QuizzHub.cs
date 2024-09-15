using Microsoft.AspNetCore.SignalR;
using QuizzApp.Services;
using QuizzApp.Share;

namespace QuizzApp.Hub
{
    public class QuizzHub : Hub<IQuizzPlayer>
    {
        private readonly QuizzLobby _quizzLobby;

        public QuizzHub(QuizzLobby quizzLobby) => _quizzLobby = quizzLobby;

        public async Task<string> JoinQuizz()
        {
            Quizz quizz = await _quizzLobby.AddPlayerToQuizzAsync(Context);

            return quizz.Name;
        }
    }
}
