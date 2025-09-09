using System.Text.Json.Serialization;

namespace Antal.Core.Application.Contracts;

/// <summary>
/// Represents a joke returned by the external jokes API.
/// </summary>
public class JokeDto
{
    /// <summary>
    /// The upstream identifier of the joke.
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    /// The textual content of the joke.
    /// </summary>
    [JsonPropertyName("value")]
    public required string Value { get; init; }
}
