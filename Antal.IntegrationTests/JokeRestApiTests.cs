using System;
using Antal.Core.Application.Contracts;
using Antal.Infrastructure.Api;
using Xunit;

namespace Antal.IntegrationTests;

public class JokeRestApiTests
{
    [RequiresRapidApiKeyFact]
    public async Task GetRandomJokeAsync_ShouldReturnJoke()
    {
        // Arrange
        var apiKey = Environment.GetEnvironmentVariable("RAPIDAPI_KEY");
        var baseAddress = "https://matchilling-chuck-norris-jokes-v1.p.rapidapi.com";

        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseAddress)
        };
        httpClient.DefaultRequestHeaders.Add("x-rapidapi-key", apiKey);
        httpClient.DefaultRequestHeaders.Add("x-rapidapi-host", "matchilling-chuck-norris-jokes-v1.p.rapidapi.com");
        httpClient.DefaultRequestHeaders.Add("accept", "application/json");

        var api = new JokeRestApi(httpClient);

        // Act
        var joke = await api.GetRandomJokeAsync();

        // Assert
        Assert.NotNull(joke);
        Assert.False(string.IsNullOrWhiteSpace(joke.Value));
        Assert.False(string.IsNullOrWhiteSpace(joke.Id));
    }
}
