namespace Jeninnet.Testing.Assertions;

/// <summary>Provides assertion methods for verifying that asynchronous delegates throw expected exceptions.</summary>
/// <param name="action">The asynchronous delegate to invoke and verify.</param>
public class AsyncActionAssertions(Func<Task> action) {
    private readonly Func<Task> _action = action;

    /// <summary>Asserts that the asynchronous action throws an exception of type <typeparamref name="TException"/>.</summary>
    /// <typeparam name="TException">The expected exception type.</typeparam>
    /// <param name="message">
    /// An optional custom failure message that is included in the
    /// <see cref="AssertionFailedException"/> if the assertion fails.
    /// </param>
    /// <returns>A task whose result is an <see cref="ExceptionAssertions{TException}"/> that provides access to the caught exception.</returns>
    /// <exception cref="AssertionFailedException">No exception was thrown, or a different exception type was thrown.</exception>
    public async Task<ExceptionAssertions<TException>> ThrowAsync<TException>(string? message = null) where TException : Exception {
        try {
            await _action();
        }
        catch (TException ex) {
            return new ExceptionAssertions<TException>(ex);
        }
        catch (Exception ex) {
            throw new AssertionFailedException(
                message ?? $"Expected exception of type {typeof(TException).Name}, but got {ex.GetType().Name}.", ex);
        }

        throw new AssertionFailedException(
            message ?? $"Expected exception of type {typeof(TException).Name}, but no exception was thrown.");
    }
}
