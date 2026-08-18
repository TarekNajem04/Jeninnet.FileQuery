//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Exceptions;

/// <summary>
/// Represents an error that occurs during pattern parsing,
/// compilation, or invariant validation.
///
/// <para>
/// This exception type is thrown when a pattern fails to meet
/// syntactic, structural, or semantic requirements enforced by
/// the pattern compiler or invariant system.
/// </para>
///
/// <para>
/// Consumers of the pattern engine should catch this exception
/// when compiling user-provided patterns, as it represents a
/// domain-level failure rather than a programming error.
/// </para>
/// </summary>
public class PatternException : Exception {
    /// <summary>
    /// Initializes a new instance of the <see cref="PatternException"/> class.
    /// </summary>
    public PatternException() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="PatternException"/> class
    /// with a descriptive error message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public PatternException(string message)
            : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="PatternException"/> class
    /// with a descriptive error message and an inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public PatternException(string message, Exception innerException)
        : base(message, innerException) { }
}
