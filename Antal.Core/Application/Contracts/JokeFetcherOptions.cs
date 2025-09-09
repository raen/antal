namespace Antal.Core.Application.Contracts;

/// <summary>
/// Provides configuration for the joke fetching process.
/// </summary>
public class JokeFetcherOptions
{
    /// <summary>
    /// The default number of jokes to attempt to fetch when not otherwise specified.
    /// </summary>
    public int Count { get; set; } = 10;
    
    /// <summary>
    /// The maximum degree of parallelism used when fetching jokes.
    /// </summary>
    /// <remarks>
    /// Values less than 1 will be coerced to 1 by the implementation.
    /// </remarks>
    public int MaxParallelism { get; set; } = 10;
}
