namespace Jeninnet.Testing.Assertions;

/// <summary>Provides assertion methods for verifying <see cref="string"/> values.</summary>
/// <param name="value">The string value to assert on.</param>
public class StringAssertions(string? value) {
    private readonly string? _value = value;

    /// <summary>Asserts that the string exactly equals <paramref name="expected"/> using ordinal comparison.</summary>
    /// <param name="expected">The expected string value.</param>
    /// <param name="message">
    /// An optional custom failure message that is included in the
    /// <see cref="AssertionFailedException"/> if the assertion fails.
    /// </param>
    /// <exception cref="AssertionFailedException">The string does not match <paramref name="expected"/>.</exception>
    public void Be(string? expected, string? message = null) {
        if (!string.Equals(_value, expected, StringComparison.Ordinal)) {
            throw new AssertionFailedException(message ?? $"Expected string to be '{expected}', but was '{_value}'.");
        }
    }

    /// <summary>Asserts that the string value is <see langword="null"/>.</summary>
    /// <param name="message">
    /// An optional custom failure message that is included in the
    /// <see cref="AssertionFailedException"/> if the assertion fails.
    /// </param>
    /// <exception cref="AssertionFailedException">The string is not <see langword="null"/>.</exception>
    public void BeNull(string? message = null) {
        if (_value is not null) {
            throw new AssertionFailedException(message ?? "Expected null, but was not null.");
        }
    }

    /// <summary>Asserts that the string value is not <see langword="null"/>.</summary>
    /// <param name="message">
    /// An optional custom failure message that is included in the
    /// <see cref="AssertionFailedException"/> if the assertion fails.
    /// </param>
    /// <exception cref="AssertionFailedException">The string is <see langword="null"/>.</exception>
    public void NotBeNull(string? message = null) {
        if (_value is null) {
            throw new AssertionFailedException(message ?? "Expected non-null, but was null.");
        }
    }

    /// <summary>Asserts that the string contains the specified <paramref name="expected"/> substring using ordinal comparison.</summary>
    /// <param name="expected">The substring to search for.</param>
    /// <param name="message">
    /// An optional custom failure message that is included in the
    /// <see cref="AssertionFailedException"/> if the assertion fails.
    /// </param>
    /// <exception cref="AssertionFailedException">The underlying string is <see langword="null"/> or does not contain <paramref name="expected"/>.</exception>
    public void Contain(string expected, string? message = null) {
        if (_value is null) {
            throw new AssertionFailedException("Cannot assert on a null string.");
        }

        if (!_value.Contains(expected, StringComparison.Ordinal)) {
            throw new AssertionFailedException(message ?? $"Expected string to contain '{expected}', but was '{_value}'.");
        }
    }

    /// <summary>Asserts that the string ends with the specified <paramref name="expected"/> substring using ordinal comparison.</summary>
    /// <param name="expected">The suffix that the string is expected to end with.</param>
    /// <param name="message">
    /// An optional custom failure message that is included in the
    /// <see cref="AssertionFailedException"/> if the assertion fails.
    /// </param>
    /// <exception cref="AssertionFailedException">The underlying string is <see langword="null"/> or does not end with <paramref name="expected"/>.</exception>
    public void EndsWith(string expected, string? message = null) {
        if (_value is null) {
            throw new AssertionFailedException("Cannot assert on a null string.");
        }

        if (!_value.EndsWith(expected, StringComparison.Ordinal)) {
            throw new AssertionFailedException(message ?? $"Expected string to end with '{expected}', but was '{_value}'.");
        }
    }
}
