using System.Text.Json;
using Antal.Core.Application.Abstractions;
using Antal.Core.Application.Contracts;

namespace Antal.Infrastructure.Api;

/// <summary>
/// HTTP-based implementation of <see cref="IJokeApi"/> that uses a configured <see cref="HttpClient"/>.
/// </summary>
/// <remarks>
/// The <see cref="HttpClient"/> should be configured with a valid <see cref="HttpClient.BaseAddress"/> and required headers via dependency injection.
/// </remarks>
public class JokeRestApi(HttpClient httpClient) : IJokeApi
{
    /// <summary>
    /// Retrieves a random joke from the upstream API.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// A <see cref="JokeDto"/> if the response content can be successfully deserialized; otherwise, <c>null</c>.
    /// </returns>
    /// <exception cref="HttpRequestException">Thrown when the HTTP response indicates failure.</exception>
    /// <exception cref="JsonException">Thrown when the response content cannot be deserialized to <see cref="JokeDto"/>.</exception>
    /// <exception cref="OperationCanceledException">Thrown if the operation is canceled.</exception>
    public async Task<JokeDto?> GetRandomJokeAsync(CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync("/jokes/random", cancellationToken);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<JokeDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}