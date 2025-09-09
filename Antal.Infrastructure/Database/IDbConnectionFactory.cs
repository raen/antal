using System.Data.Common;

namespace Antal.Infrastructure.Database;

/// <summary>
/// Provides a factory for creating and opening database connections.
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>
    /// Creates and opens a new database connection.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while opening the connection.</param>
    /// <returns>An open <see cref="DbConnection"/> instance.</returns>
    Task<DbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default);
}
