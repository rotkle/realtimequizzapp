using QuizzApp.Configurations;
using QuizzApp.Hub;
using QuizzApp.Services;

namespace QuizzApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSignalR();

            builder.Services.AddSingleton<QuizzLobby>();

            // Bind the quizzes options from configuration
            builder.Services.AddOptions<QuizzOptions>()
                            .BindConfiguration("Quizz");

            // This needs to be transient as the GameFactory manages the lifetime
            // of Quizz
            builder.Services.AddTransient<Quizz>();

            // We only have a single client so we'll use the empty name
            builder.Services.AddHttpClient(string.Empty, client =>
            {
                client.BaseAddress = new Uri("https://the-trivia-api.com/api/questions");
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
            }

            app.UseFileServer();
            app.UseBlazorFrameworkFiles();

            app.UseRouting();

            app.MapHub<QuizzHub>("/quizz");

            app.Run();
        }
    }
}