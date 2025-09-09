namespace Antal.Core.Application.Abstractions;

/// <summary>
/// Provides a mechanism to initialize required database structures and seed data.
/// </summary>
public interface IDatabaseInitializer
{
    /// <summary>
    /// Initializes the database for the application.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while performing initialization.</param>
    /// <remarks>
    /// Implementations should be idempotent and safe to call multiple times.
    /// </remarks>
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
