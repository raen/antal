using Antal.Core.Application.Abstractions;
using Antal.Core.Domain;
using Dapper;

namespace Antal.Infrastructure.Database;

/// <summary>
/// SQLite/Dapper-based implementation of <see cref="IJokeRepository"/> and <see cref="IDatabaseInitializer"/>.
/// </summary>
/// <remarks>
/// Uses an <see cref="IDbConnectionFactory"/> to create connections per operation. The underlying table
/// is created as needed by <see cref="InitializeAsync"/> with a unique constraint on joke text.
/// </remarks>
public class JokeRepository(IDbConnectionFactory dbConnectionFactory) : IJokeRepository, IDatabaseInitializer
{
    /// <summary>
    /// Ensures the SQLite schema required by this repository exists.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while performing the operation.</param>
    /// <remarks>
    /// Creates a table named <c>jokes</c> with columns: <c>id</c> (TEXT PRIMARY KEY),
    /// <c>JokeText</c> (TEXT NOT NULL UNIQUE), and <c>created_at</c> (TEXT NOT NULL, ISO-8601).
    /// The operation is idempotent.
    /// </remarks>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        using var createTableCommand = connection.CreateCommand();
        createTableCommand.CommandText = @"
            CREATE TABLE IF NOT EXISTS jokes (
                id TEXT PRIMARY KEY,
                JokeText TEXT NOT NULL UNIQUE,
                created_at TEXT NOT NULL
            );
        ";
        await createTableCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <inheritdoc />
    /// <remarks>
    /// Inserts are performed in a single transaction. The table enforces a UNIQUE constraint on <c>JokeText</c>
    /// and a PRIMARY KEY on <c>id</c>. Duplicates are ignored at the database level via <c>INSERT OR IGNORE</c>.
    /// </remarks>
    public async Task<int> InsertJokesAsync(IReadOnlyCollection<Joke> jokes, CancellationToken cancellationToken = default)
    {
        await using var connection = await dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        var affected = 0;
        foreach (var p in jokes.Select(j => new { j.Id, j.Text, CreatedAt = DateTime.UtcNow.ToString("o") }))
        {
            var command = new CommandDefinition(
                "INSERT OR IGNORE INTO jokes (id, JokeText, created_at) VALUES (@Id, @Text, @CreatedAt);",
                p,
                transaction,
                cancellationToken: cancellationToken);
            affected += await connection.ExecuteAsync(command);
        }

        await transaction.CommitAsync(cancellationToken);
        return affected;
    }

    public async Task<IEnumerable<Joke>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        var command = new CommandDefinition("SELECT Id, JokeText as Text FROM jokes", cancellationToken: cancellationToken);
        return await connection.QueryAsync<Joke>(command);
    }
}