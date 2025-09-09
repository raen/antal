using System.Collections.Concurrent;
using Antal.Core.Application;
using Antal.Core.Application.Abstractions;
using Antal.Core.Application.Contracts;
using Antal.Core.Domain;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace Antal.UnitTests.Application;

public class JokeFetcherTests
{
    [Fact]
    public async Task GetJokesAsync_FiltersTooLongAndDuplicates_SavesOnlyNewAndReturnsMetrics()
    {
        // Arrange
        var longText = new string('a', 201);
        var inputs = new List<JokeDto?>
        {
            new() { Id = "1", Value = "short1" },
            new() { Id = "2", Value = "short1" }, // duplicate text
            new() { Id = "3", Value = longText },  // too long
            null,
            new() { Id = "4", Value = "short2" }
        };

        var api = new FakeJokeApi(inputs);
        var repo = new FakeJokeRepository();
        repo.ExistingTexts.Add("short1"); // already in DB
        var logger = new TestLogger<JokeFetcher>();
        var options = Options.Create(new JokeFetcherOptions() { MaxParallelism = 1 });

        var sut = new JokeFetcher(api, repo, logger, options);

        // Act
        var result = await sut.GetJokesAsync(inputs.Count);

        // Assert
        result.TotalFetched.Should().Be(3);      // null and too long filtered out before domain creation
        result.UniqueFetched.Should().Be(2);     // "short1" and "short2"
        result.SavedToDatabase.Should().Be(1);   // only "short2" inserted ("short1" existed)

        repo.Inserted.Select(j => j.Text).Should().BeEquivalentTo("short2");
    }

    [Fact]
    public async Task GetJokesAsync_LogsWarnings_ForNullAndInvalidDtos()
    {
        // Arrange
        var longText = new string('a', 201);
        var inputs = new List<JokeDto?>
        {
            null,                                       // should log a warning about null
            new JokeDto { Id = "x", Value = longText } // should log a warning about invalid DTO
        };

        var api = new FakeJokeApi(inputs);
        var repo = new FakeJokeRepository();
        var logger = new TestLogger<JokeFetcher>();
        var options = Options.Create(new JokeFetcherOptions() { MaxParallelism = 1 });

        var sut = new JokeFetcher(api, repo, logger, options);

        // Act
        var result = await sut.GetJokesAsync(inputs.Count);

        // Assert: no valid jokes created
        result.TotalFetched.Should().Be(0);
        result.UniqueFetched.Should().Be(0);
        result.SavedToDatabase.Should().Be(0);

        logger.Logs.Any(l => l.Level == LogLevel.Warning && l.Message.Contains("null", StringComparison.OrdinalIgnoreCase))
            .Should().BeTrue("should log warning for null DTO");
        logger.Logs.Any(l => l.Level == LogLevel.Warning && l.Message.Contains("Failed to create a joke", StringComparison.CurrentCulture))
            .Should().BeTrue("should log warning for invalid DTO");
    }

    private sealed class FakeJokeApi(IEnumerable<JokeDto?> dtos) : IJokeApi
    {
        private readonly ConcurrentQueue<JokeDto?> _queue = new(dtos);

        public Task<JokeDto?> GetRandomJokeAsync(CancellationToken cancellationToken = default)
        {
            _queue.TryDequeue(out var dto);
            return Task.FromResult(dto);
        }
    }

    private sealed class FakeJokeRepository : IJokeRepository
    {
        public HashSet<string> ExistingTexts { get; } = [];
        public List<Joke> Inserted { get; } = [];

        public Task<int> InsertJokesAsync(IReadOnlyCollection<Joke> jokes, CancellationToken cancellationToken = default)
        {
            var before = Inserted.Count;
            foreach (var j in jokes)
            {
                // Simulate DB unique constraint by text and that existing texts are already present
                if (ExistingTexts.Contains(j.Text))
                    continue;

                if (Inserted.Any(x => x.Text == j.Text))
                    continue;

                Inserted.Add(j);
                ExistingTexts.Add(j.Text);
            }

            var insertedCount = Inserted.Count - before;
            return Task.FromResult(insertedCount);
        }

        public Task<IEnumerable<Joke>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IEnumerable<Joke>>([]);
        }
    }

    private sealed class TestLogger<T> : ILogger<T>
    {
        public List<(LogLevel Level, string Message, Exception? Exception)> Logs { get; } = [];

        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var message = formatter(state, exception);
            lock (Logs)
            {
                Logs.Add((logLevel, message, exception));
            }
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }
}
