using Antal.Core.Domain;
using FluentAssertions;
using Xunit;

namespace Antal.UnitTests.Domain;

public class JokeTests
{
    [Fact]
    public void Create_WithValidInput_ShouldReturnJoke()
    {
        // Act
        var joke = Joke.Create("1", "A valid joke.");

        // Assert
        joke.Should().NotBeNull();
        joke.Id.Should().Be("1");
        joke.Text.Should().Be("A valid joke.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Create_WithNullOrEmptyId_ShouldThrowArgumentException(string? id)
    {
        // Act
        Action act = () => Joke.Create(id!, "A valid joke.");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName(nameof(Joke.Id));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Create_WithNullOrEmptyText_ShouldThrowArgumentException(string? text)
    {
        // Act
        Action act = () => Joke.Create("1", text!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName(nameof(Joke.Text));
    }

    [Fact]
    public void Create_WithTextLongerThanMaxLength_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var longJoke = new string('a', 201);

        // Act
        Action act = () => Joke.Create("1", longJoke);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName(nameof(Joke.Text));
    }
}
