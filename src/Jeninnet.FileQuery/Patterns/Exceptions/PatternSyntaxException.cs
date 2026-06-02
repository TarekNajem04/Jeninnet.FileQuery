#pragma warning disable RCS1194
namespace Jeninnet.FileQuery.Patterns.Exceptions;

/// <summary>
/// Represents a syntax error in a pattern string.
/// </summary>
public sealed class PatternSyntaxException : PatternException {
    /// <summary>The original pattern text that caused the error.</summary>
    public string Pattern { get; }

    /// <summary>
    /// Initializes a new instance with the offending pattern and a message.
    /// </summary>
    public PatternSyntaxException(string pattern, string message)
        : base(message) => Pattern = pattern;

    /// <summary>
    /// Initializes a new instance with the offending pattern, a message, and an inner exception.
    /// </summary>
    public PatternSyntaxException(string pattern, string message, Exception innerException)
        : base(message, innerException) => Pattern = pattern;
}
