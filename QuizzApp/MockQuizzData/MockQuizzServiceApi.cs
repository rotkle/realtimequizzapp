namespace QuizzApp.MockQuizzData
{
    public class MockQuizzServiceApi
    {
        private readonly HttpClient _client;
        public MockQuizzServiceApi(HttpClient client)
        {
            _client = client;
        }

        public async Task<QuizzDataQuestion[]> GetQuestionsAsync(int numberOfQuestions)
        {
            var results = new List<QuizzDataQuestion>();

            while (results.Count < numberOfQuestions)
            {
                // Get 10 quizz questions that match a certain criteria
                var quizzQuestions = await _client.GetFromJsonAsync<QuizzDataQuestion[]>("?limit=10");

                foreach (var q in quizzQuestions!)
                {
                    if (q.Type == "Multiple Choice" && !q.IsNiche)
                    {
                        results.Add(q);

                        if (results.Count == numberOfQuestions)
                        {
                            break;
                        }
                    }
                }
            }

            return results.ToArray();
        }
    }
}
