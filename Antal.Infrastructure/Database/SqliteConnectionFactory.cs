using Microsoft.Data.Sqlite;
using System.Data.Common;

namespace Antal.Infrastructure.Database;

/// <summary>
/// SQLite implementation of <see cref="IDbConnectionFactory"/>.
/// </summary>
/// <param name="connectionString">The SQLite connection string (e.g., <c>Data Source=antal.db</c>).</param>
/// <remarks>
/// Each call creates a new <see cref="SqliteConnection"/> and opens it before returning.
/// </remarks>
public class SqliteConnectionFactory(string connectionString) : IDbConnectionFactory
{
    /// <inheritdoc />
    public async Task<DbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
