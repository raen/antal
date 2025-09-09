using Antal.Core.Application.Abstractions;
using Antal.Core.Application.Contracts;
using Antal.Core.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace Antal.Core.Application;

/// <summary>
/// Default implementation of <see cref="IJokeFetcher"/> that retrieves jokes from an upstream API,
/// de-duplicates them by text, and persists only new entries.
/// </summary>
/// <remarks>
/// The implementation fetches jokes in parallel using <see cref="Parallel.ForEachAsync"/> with concurrency
/// limited by <see cref="JokeFetcherOptions.MaxParallelism"/>.
/// </remarks>
public class JokeFetcher(IJokeApi jokeApi, IJokeRepository jokeRepository, ILogger<JokeFetcher> logger, IOptions<JokeFetcherOptions> fetcherOptions) 
    : IJokeFetcher
{
    /// <inheritdoc />
    public async Task<JokeFetchResult> GetJokesAsync(int count, CancellationToken cancellationToken = default)
    {
        ValidateJokeCount(count);
        
        logger.LogInformation("Starting to fetch {Count} jokes", count);

        var jokeDtos = await FetchJokesFromApiAsync(count, cancellationToken);
        var (totalFetched, uniqueJokes) = ProcessFetchedJokes(jokeDtos);
        var savedCount = await SaveNewJokes(uniqueJokes, cancellationToken);

        logger.LogInformation("Successfully finished fetching jokes");
        
        return new JokeFetchResult
        {
            TotalFetched = totalFetched,
            UniqueFetched = uniqueJokes.Count,
            SavedToDatabase = savedCount
        };
    }

    private static void ValidateJokeCount(int count)
    {
        if (count <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "The number of jokes to fetch must be positive.");
        }

        if (count > 1000)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "The maximum number of jokes in a single request is 1000.");
        }
    }

    private async Task<ConcurrentBag<JokeDto?>> FetchJokesFromApiAsync(int count, CancellationToken cancellationToken)
    {
        var jokeDtos = new ConcurrentBag<JokeDto?>();
        await Parallel.ForEachAsync(Enumerable.Range(0, count),
            new ParallelOptions { MaxDegreeOfParallelism = Math.Max(1, fetcherOptions.Value.MaxParallelism), CancellationToken = cancellationToken },
            async (_, token) =>
            {
                var jokeDto = await jokeApi.GetRandomJokeAsync(token);
                jokeDtos.Add(jokeDto);
            });
        return jokeDtos;
    }

    private (int totalFetched, List<Joke> uniqueJokes) ProcessFetchedJokes(ConcurrentBag<JokeDto?> jokeDtos)
    {
        var jokes = jokeDtos
            .Select(CreateJokeSafely)
            .Where(joke => joke != null)
            .Cast<Joke>()
            .ToList();

        var uniqueJokes = RemoveDuplicates(jokes);
        
        logger.LogInformation("Fetched {FetchedCount} jokes, {UniqueCount} unique", jokes.Count, uniqueJokes.Count);

        return (jokes.Count, uniqueJokes);
    }
    
    private static List<Joke> RemoveDuplicates(List<Joke> jokes)
    {
        return jokes
            .GroupBy(joke => joke.Text)
            .Select(group => group.First())
            .ToList();
    }
    
    private Joke? CreateJokeSafely(JokeDto? jokeDto)
    {
        if (jokeDto == null)
        {
            logger.LogWarning("API returned null for a joke");
            return null;
        }

        try
        {
            return Joke.Create(jokeDto.Id, jokeDto.Value);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to create a joke from DTO: {DtoId}", jokeDto.Id);
            return null;
        }
    }
    
    private async Task<int> SaveNewJokes(List<Joke> jokes, CancellationToken cancellationToken)
    {
        if (jokes.Count == 0)
        { 
            logger.LogInformation("No new jokes to save");
            return 0;
        }

        // Database enforces uniqueness, and we use INSERT OR IGNORE; rely on DB for final deduplication.
        var saved = await jokeRepository.InsertJokesAsync(jokes, cancellationToken);
        logger.LogInformation("Saved {JokeCount} new jokes", saved);

        return saved;
    }
}