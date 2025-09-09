using Antal.Core.Domain;

namespace Antal.Core.Application.Abstractions;

/// <summary>
/// Provides persistence operations for jokes.
/// </summary>
public interface IJokeRepository
{
    /// <summary>
    /// Inserts the provided jokes into the underlying store.
    /// </summary>
    /// <param name="jokes">The jokes to insert. Implementations may assume the collection is already de-duplicated by <see cref="Joke.Text"/>.</param>
    /// <param name="cancellationToken">A token to observe while performing the operation.</param>
    /// <remarks>
    /// Implementations should avoid inserting duplicates by joke text. If duplicates are provided, they should be ignored.
    /// </remarks>
    /// <returns>The number of jokes that were actually inserted.</returns>
    Task<int> InsertJokesAsync(IReadOnlyCollection<Joke> jokes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all jokes from the underlying store.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while performing the operation.</param>
    /// <returns>A collection of all jokes.</returns>
    Task<IEnumerable<Joke>> GetAllAsync(CancellationToken cancellationToken = default);
}