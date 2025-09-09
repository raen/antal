using Antal.Core.Application.Contracts;

namespace Antal.Core.Application.Abstractions;

/// <summary>
/// Coordinates fetching jokes from an upstream provider and persisting unique entries.
/// </summary>
public interface IJokeFetcher
{
    /// <summary>
    /// Fetches a set of jokes, removes duplicates by their text, and persists only new entries.
    /// </summary>
    /// <param name="count">The number of jokes to attempt to fetch. Must be between 1 and 1000, inclusive.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// A <see cref="JokeFetchResult"/> that contains metrics about the operation, including the total number fetched, the number of unique jokes, and how many were saved to the database.
    /// </returns>
    /// <remarks>
    /// Implementations typically fetch in parallel and control concurrency via <see cref="JokeFetcherOptions.MaxParallelism"/>.
    /// Duplicates are identified by the joke <c>Text</c> and only new records are persisted.
    /// </remarks>
    /// <exception cref="System.ArgumentOutOfRangeException">Thrown when <paramref name="count"/> is less than 1 or greater than 1000.</exception>
    /// <exception cref="System.OperationCanceledException">May be thrown if the operation is canceled.</exception>
    Task<JokeFetchResult> GetJokesAsync(int count, CancellationToken cancellationToken = default);
}
