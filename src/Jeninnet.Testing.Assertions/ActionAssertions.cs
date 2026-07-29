namespace Jeninnet.Testing.Assertions;

/// <summary>Provides assertion methods for verifying that synchronous delegates throw expected exceptions.</summary>
/// <param name="action">The synchronous delegate to invoke and verify.</param>
public class ActionAssertions(Action action) {
    private readonly Action _action = action;

    /// <summary>Asserts that the action throws an exception of type <typeparamref name="TException"/>.</summary>
    /// <typeparam name="TException">The expected exception type.</typeparam>
    /// <param name="message">
    /// An optional custom failure message that is included in the
    /// <see cref="AssertionFailedException"/> if the assertion fails.
    /// </param>
    /// <returns>An <see cref="ExceptionAssertions{TException}"/> that provides access to the caught exception.</returns>
    /// <exception cref="AssertionFailedException">No exception was thrown, or a different exception type was thrown.</exception>
    public ExceptionAssertions<TException> Throw<TException>(string? message = null) where TException : Exception {
        try {
            _action();
        }
        catch(TException ex) {
            return new ExceptionAssertions<TException>(ex);
        }
        catch(Exception ex) {
            throw new AssertionFailedException(
                message ?? $"Expected exception of type {typeof(TException).Name}, but got {ex.GetType().Name}.", ex);
        }

        throw new AssertionFailedException(
            message ?? $"Expected exception of type {typeof(TException).Name}, but no exception was thrown.");
    }
}
