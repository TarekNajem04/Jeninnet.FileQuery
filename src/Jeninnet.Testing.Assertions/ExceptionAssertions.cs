namespace Jeninnet.Testing.Assertions;

/// <summary>
/// Provides access to a caught exception after a <c>Throw</c> or <c>ThrowAsync</c> assertion succeeds,
/// enabling further inspection of the exception instance.
/// </summary>
/// <typeparam name="TException">The type of the caught exception.</typeparam>
/// <param name="exception">The caught exception instance to expose.</param>
public class ExceptionAssertions<TException>(TException exception) where TException : Exception {
    /// <summary>Gets the caught exception instance.</summary>
    /// <exception cref="AssertionFailedException">Thrown if the exception instance is <see langword="null"/>.</exception>
    public TException Exception { get; } = exception ?? throw new AssertionFailedException("Exception cannot be null.");
}
