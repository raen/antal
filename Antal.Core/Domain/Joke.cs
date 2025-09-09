namespace Antal.Core.Domain;

/// <summary>
/// Represents a domain joke entity with invariant validation.
/// </summary>
public class Joke
{
    private const int MaxLength = 200;

    /// <summary>
    /// Gets the unique identifier of the joke.
    /// </summary>
    public string Id { get; private set; }
    /// <summary>
    /// Gets the textual content of the joke.
    /// </summary>
    public string Text { get; private set; }
    
    private Joke(string id, string text)
        => (Id, Text) = (id, text);
    
    /// <summary>
    /// Creates a <see cref="Joke"/> from primitive values enforcing domain invariants.
    /// </summary>
    /// <param name="id">The unique joke identifier.</param>
    /// <param name="text">The joke text.</param>
    /// <returns>A new <see cref="Joke"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> or <paramref name="text"/> is <c>null</c> or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="text"/> exceeds the maximum length of 200 characters.</exception>
    public static Joke Create(string id, string text)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Joke ID cannot be null or empty.", nameof(Id));

        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Joke text cannot be null or empty.", nameof(Text));
        
        if (text.Length > MaxLength)
            throw new ArgumentOutOfRangeException(nameof(Text), $"Joke cannot be longer than {MaxLength} characters.");

        return new Joke(id, text);
    }
}