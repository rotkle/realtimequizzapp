using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizzApp.Share
{
    public class QuizzAnswer
    {
        public int? Choice { get; set; }
    }

    public class QuizzQuestion
    {
        public string Question { get; set; }
        public string[] Choices { get; set; }
    }

    public class QuizzConfiguration
    {
        public int NumberOfQuestions { get; init; }

        public int QuestionTimeout { get; init; }
    }

    public class QuizzCompletedEvent
    {
        public string Name { get; init; }
        public int Correct { get; init; }
    }
}
