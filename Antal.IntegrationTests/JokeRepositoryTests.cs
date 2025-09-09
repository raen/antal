using Antal.Core.Domain;
using Antal.Infrastructure.Database;
using Microsoft.Data.Sqlite;
using Xunit;

namespace Antal.IntegrationTests;

public class JokeRepositoryTests : IAsyncLifetime
{
    private readonly string _dbName;
    private readonly string _connectionString;
    private readonly JokeRepository _repository;

    public JokeRepositoryTests()
    {
        _dbName = $"test_{Path.GetRandomFileName()}.db";
        _connectionString = $"Data Source={_dbName}";
        IDbConnectionFactory connectionFactory = new SqliteConnectionFactory(_connectionString);
        _repository = new JokeRepository(connectionFactory);
    }

    public async Task InitializeAsync()
    {
        await _repository.InitializeAsync();
    }

    [Fact]
    public async Task InsertJokesAsync_ShouldStoreJokesInDatabase()
    {
        // Arrange
        var jokes = new List<Joke>
        {
            Joke.Create("test-id-1", "A classic joke"),
            Joke.Create("test-id-2", "Another classic joke")
        };

        // Act
        var inserted = await _repository.InsertJokesAsync(jokes);

        // Assert
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM jokes;";
        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        Assert.Equal(2, count);
        Assert.Equal(2, inserted);
    }

    [Fact]
    public async Task InsertJokesAsync_WithDuplicateIdInBatch_ShouldIgnoreDuplicates()
    {
        // Arrange
        var jokes = new List<Joke>
        {
            Joke.Create("duplicate-id", "First joke"),
            Joke.Create("duplicate-id", "Second joke")
        };

        // Act
        var inserted = await _repository.InsertJokesAsync(jokes);

        // Assert
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM jokes;";
        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        Assert.Equal(1, count);
        Assert.Equal(1, inserted);
    }

    [Fact]
    public async Task InsertJokesAsync_WithDuplicateTextInBatch_ShouldIgnoreDuplicates()
    {
        // Arrange
        var jokes = new List<Joke>
        {
            Joke.Create("test-id-1", "A duplicated joke"),
            Joke.Create("test-id-2", "A duplicated joke")
        };

        // Act
        var inserted = await _repository.InsertJokesAsync(jokes);

        // Assert
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM jokes;";
        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        Assert.Equal(1, count);
        Assert.Equal(1, inserted);
    }

    [Fact]
    public async Task InsertJokesAsync_WithExistingJoke_ShouldIgnoreDuplicate()
    {
        // Arrange
        var joke = Joke.Create("test-id-1", "A classic joke");
        await _repository.InsertJokesAsync(new[] { joke });

        // Act
        var inserted = await _repository.InsertJokesAsync(new[] { joke });

        // Assert
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM jokes;";
        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        Assert.Equal(1, count);
        Assert.Equal(0, inserted);
    }

    public Task DisposeAsync()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        SqliteConnection.ClearAllPools();

        if (File.Exists(_dbName))
        { 
            File.Delete(_dbName);
        }

        return Task.CompletedTask;
    }
}