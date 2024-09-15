namespace QuizzApp.MockQuizzData
{
    public class QuizzDataQuestion
    {
        public string? Category { get; set; }
        public string Id { get; set; }
        public string CorrectAnswer { get; set; }
        public string[] IncorrectAnswers { get; set; }
        public string Question { get; set; }
        public string[]? Tags { get; set; }
        public string? Type { get; set; }
        public string? Difficulty { get; set; }
        public object[]? Regions { get; set; }
        public bool IsNiche { get; set; }
    }
}
