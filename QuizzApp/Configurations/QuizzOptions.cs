namespace QuizzApp.Configurations
{
    public class QuizzOptions
    {
        public int MaxPlayersPerQuizz { get; init; } = 4;
        public int TimePerQuestion { get; init; } = 20;
        public int QuestionsPerQuizz { get; init; } = 5;
    }
}
