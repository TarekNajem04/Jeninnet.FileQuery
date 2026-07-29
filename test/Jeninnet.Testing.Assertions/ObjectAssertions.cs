namespace Jeninnet.Testing.Assertions;

/// <summary>Provides assertion methods for verifying objects of type <typeparamref name="T"/>.</summary>
/// <typeparam name="T">The static type of the object under assertion.</typeparam>
/// <param name="value">The object value to assert on.</param>
public class ObjectAssertions<T>(T? value) {
    private readonly T? _value = value;

    /// <summary>Asserts that the value equals <paramref name="expected"/> using the default equality comparer.</summary>
    /// <param name="expected">The expected value.</param>
    /// <param name="message">
    /// An optional custom failure message that is included in the
    /// <see cref="AssertionFailedException"/> if the assertion fails.
    /// </param>
    /// <exception cref="AssertionFailedException">The values are not equal.</exception>
    public void Be(T? expected, string? message = null) {
        if (!EqualityComparer<T?>.Default.Equals(_value, expected)) {
            throw new AssertionFailedException(message ?? $"Expected '{expected}', but got '{_value}'.");
        }
    }

    /// <summary>Asserts that the run-time type of the value matches <typeparamref name="TExpected"/>.</summary>
    /// <typeparam name="TExpected">The expected run-time type.</typeparam>
    /// <param name="message">
    /// An optional custom failure message that is included in the
    /// <see cref="AssertionFailedException"/> if the assertion fails.
    /// </param>
    /// <exception cref="AssertionFailedException">The run-time type does not match <typeparamref name="TExpected"/>.</exception>
    public void Be<TExpected>(string? message = null) {
        if (typeof(TExpected) != (_value?.GetType() ?? typeof(T))) {
            throw new AssertionFailedException(
                message ?? $"Expected type {typeof(TExpected).Name}, but got {_value?.GetType().Name ?? "null"}.");
        }
    }

    /// <summary>Asserts that the value is <see langword="null"/>.</summary>
    /// <param name="message">
    /// An optional custom failure message that is included in the
    /// <see cref="AssertionFailedException"/> if the assertion fails.
    /// </param>
    /// <exception cref="AssertionFailedException">The value is not <see langword="null"/>.</exception>
    public void BeNull(string? message = null) {
        if (_value is not null) {
            throw new AssertionFailedException(message ?? "Expected null, but was not null.");
        }
    }

    /// <summary>Asserts that the value is not <see langword="null"/>.</summary>
    /// <param name="message">
    /// An optional custom failure message that is included in the
    /// <see cref="AssertionFailedException"/> if the assertion fails.
    /// </param>
    /// <exception cref="AssertionFailedException">The value is <see langword="null"/>.</exception>
    public void NotBeNull(string? message = null) {
        if (_value is null) {
            throw new AssertionFailedException(message ?? "Expected non-null, but was null.");
        }
    }

    /// <summary>Asserts that the value is assignable to type <typeparamref name="TExpected"/>.</summary>
    /// <typeparam name="TExpected">The type that the value must be assignable to.</typeparam>
    /// <param name="message">
    /// An optional custom failure message that is included in the
    /// <see cref="AssertionFailedException"/> if the assertion fails.
    /// </param>
    /// <exception cref="AssertionFailedException">The value is <see langword="null"/> or is not of the expected type.</exception>
    public void BeOfType<TExpected>(string? message = null) {
        if (_value is null) {
            throw new AssertionFailedException(message ?? "Expected non-null value of compatible type.");
        }

        if (_value is not TExpected) {
            throw new AssertionFailedException(
                message ?? $"Expected object of type {typeof(TExpected).Name}, but got {_value.GetType().Name}.");
        }
    }
}
