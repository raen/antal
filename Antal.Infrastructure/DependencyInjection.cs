using Antal.Core.Application.Abstractions;
using Antal.Infrastructure.Api;
using Antal.Infrastructure.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace Antal.Infrastructure;

/// <summary>
/// Provides dependency injection extensions for the Antal infrastructure layer.
/// </summary>
/// <remarks>
/// Registers the SQLite-backed repository and configures an <see cref="IJokeApi"/> HTTP client
/// with retry and circuit-breaker policies.
/// </remarks>
public static class DependencyInjection
{
    /// <summary>
    /// Registers infrastructure services: database connection factory, repository, database initializer,
    /// and the HTTP client for <see cref="IJokeApi"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration used to resolve required settings.</param>
    /// <returns>The same service collection for chaining.</returns>
    /// <remarks>
    /// The following registrations are applied:
    /// <list type="bullet">
    /// <item>
    /// <description><see cref="IDbConnectionFactory"/> as a singleton using <see cref="SqliteConnectionFactory"/>.</description>
    /// </item>
    /// <item>
    /// <description><see cref="IJokeRepository"/> and <see cref="IDatabaseInitializer"/> mapped to <see cref="JokeRepository"/>.</description>
    /// </item>
    /// <item>
    /// <description><see cref="IJokeApi"/> implemented by <see cref="JokeRestApi"/> via <see cref="System.Net.Http.HttpClient"/>,
    /// configured with base address and headers, and wrapped with retry and circuit-breaker policies.</description>
    /// </item>
    /// </list>
    /// Required configuration keys:
    /// <list type="bullet">
    /// <item>
    /// <description><c>ConnectionStrings:SqliteConnectionString</c> or <c>SqliteConnectionString</c></description>
    /// </item>
    /// <item>
    /// <description><c>JokeApi:BaseAddress</c> (and optional <c>JokeApi:ApiKey</c>)</description>
    /// </item>
    /// </list>
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when required configuration values are missing.</exception>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SqliteConnectionString")
                             ?? configuration["SqliteConnectionString"]
                             ?? throw new InvalidOperationException("SQLite connection string not found. Configure 'ConnectionStrings:SqliteConnectionString' or 'Values:SqliteConnectionString'.");

        services.AddSingleton<IDbConnectionFactory>(_ => new SqliteConnectionFactory(connectionString));
        services.AddScoped<JokeRepository>();
        services.AddScoped<IJokeRepository, JokeRepository>(s => s.GetRequiredService<JokeRepository>());
        services.AddScoped<IDatabaseInitializer, JokeRepository>(s => s.GetRequiredService<JokeRepository>());

        services.AddHttpClient<IJokeApi, JokeRestApi>(client =>
        {
            var baseAddress = configuration["JokeApi:BaseAddress"] 
                              ?? throw new InvalidOperationException("JokeApi:BaseAddress is not configured.");
            var apiKey = configuration["JokeApi:ApiKey"];

            client.BaseAddress = new Uri(baseAddress);
            client.DefaultRequestHeaders.Add("x-rapidapi-key", apiKey);
            client.DefaultRequestHeaders.Add("x-rapidapi-host", "matchilling-chuck-norris-jokes-v1.p.rapidapi.com");
            client.DefaultRequestHeaders.Add("accept", "application/json");
        })
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetCircuitBreakerPolicy());

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
    }
}