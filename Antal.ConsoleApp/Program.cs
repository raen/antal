using Antal.Core.Application;
using Antal.Core.Application.Abstractions;
using Antal.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// 1. Build configuration
var configuration = new ConfigurationBuilder()
    .AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["ConnectionStrings:SqliteConnectionString"] = "Data Source=jokes.db",
        ["JokeApi:BaseAddress"] = "https://api.chucknorris.io",
        ["JokeApi:ApiKey"] = "" // API key is not needed for this endpoint
    })
    .Build();

// 2. Setup Dependency Injection
var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole());
services.AddInfrastructure(configuration);
services.AddScoped<JokeFetcher>(); // Register JokeFetcher

var serviceProvider = services.BuildServiceProvider();

// Initialize the database
var dbInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
await dbInitializer.InitializeAsync();

// 3. Fetch jokes
var jokeFetcher = serviceProvider.GetRequiredService<JokeFetcher>();
var result = await jokeFetcher.GetJokesAsync(10);

Console.WriteLine("--- Fetch Stats ---");
Console.WriteLine($"Total jokes fetched from API: {result.TotalFetched}");
Console.WriteLine($"Unique jokes found: {result.UniqueFetched}");
Console.WriteLine($"New jokes saved to DB: {result.SavedToDatabase}");

// 4. Display jokes from the database
var repository = serviceProvider.GetRequiredService<IJokeRepository>();
var allJokes = await repository.GetAllAsync();

Console.WriteLine("\n--- Jokes in DB ---");
foreach (var joke in allJokes)
{
    Console.WriteLine($"- {joke.Text}");
}