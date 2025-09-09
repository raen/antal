namespace Antal.Core.Application.Contracts;

/// <summary>
/// Represents metrics collected during a joke fetching operation.
/// </summary>
public record JokeFetchResult
{
    /// <summary>
    /// The total number of jokes returned by the upstream provider (including duplicates and invalid entries that were filtered out).
    /// </summary>
    public int TotalFetched { get; init; }

    /// <summary>
    /// The number of unique jokes after de-duplication by text.
    /// </summary>
    public int UniqueFetched { get; init; }
    
    /// <summary>
    /// The number of jokes that were newly persisted to the database.
    /// </summary>
    public int SavedToDatabase { get; init; }
}