//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.Testing.Assertions;

/// <summary>
/// Represents an assertion failure with a descriptive message and optional inner exception.
/// This is the sole exception type thrown by all assertion methods in this library.
/// </summary>
[Serializable]
public class AssertionFailedException : Exception {
    /// <summary>Creates a new assertion failure with no message.</summary>
    public AssertionFailedException() { }

    /// <summary>Creates a new assertion failure with the given message.</summary>
    /// <param name="message">The message that describes the assertion failure.</param>
    public AssertionFailedException(string? message) : base(message) { }

    /// <summary>
    /// Creates a new assertion failure with a message and a reference to the inner exception
    /// that caused this failure (for example, an exception of the wrong type that was thrown
    /// when a specific exception type was expected).
    /// </summary>
    /// <param name="message">The message that describes the assertion failure.</param>
    /// <param name="innerException">The exception that is the cause of the current failure.</param>
    public AssertionFailedException(string? message, Exception? innerException) : base(message, innerException) { }

    /// <summary>Initializes a new instance from serialized data.</summary>
    /// <param name="info">The <see cref="SerializationInfo"/> holding the serialized object data.</param>
    /// <param name="context">The <see cref="StreamingContext"/> describing the source and destination of the serialized stream.</param>
    /// <exception cref="ArgumentNullException"><paramref name="info"/> is <see langword="null"/>.</exception>
#pragma warning disable SYSLIB0051
    protected AssertionFailedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#pragma warning restore SYSLIB0051
}
