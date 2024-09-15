using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizzApp.Share
{
    public interface IQuizzPlayer
    {
        Task<QuizzAnswer> AskQuestion(QuizzQuestion question, CancellationToken cancellationToken);
        Task WriteMessage(string message);
        Task QuizzStarted(QuizzConfiguration gameConfiguration);
        Task QuizzCompleted(QuizzCompletedEvent @event);
    }
}
