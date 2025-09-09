using Antal.Core.Application.Contracts;

namespace Antal.Core.Application.Abstractions;

/// <summary>
/// Provides access to an external jokes API for retrieving random jokes.
/// </summary>
public interface IJokeApi
{
    /// <summary>
    /// Retrieves a random joke from the upstream provider.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A <see cref="JokeDto"/> instance if the provider returns a valid joke; otherwise, <c>null</c>.
    /// </returns>
    /// <remarks>
    /// Implementations may return <c>null</c> when the upstream API responds with an empty payload or a non-mappable result.
    /// </remarks>
    /// <exception cref="System.Net.Http.HttpRequestException">Thrown when the HTTP request fails (if applicable in a given implementation).</exception>
    Task<JokeDto?> GetRandomJokeAsync(CancellationToken cancellationToken = default);
}