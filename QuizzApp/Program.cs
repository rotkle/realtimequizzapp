using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace QuizzApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.RootComponents.Add<Quizz>("#app");

            await builder.Build().RunAsync();
        }
    }
}