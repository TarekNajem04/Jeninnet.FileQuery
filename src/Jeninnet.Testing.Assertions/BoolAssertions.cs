namespace Jeninnet.Testing.Assertions;

/// <summary>Provides assertion methods for verifying <see cref="bool"/> values.</summary>
/// <param name="value">The boolean value to assert on.</param>
public class BoolAssertions(bool value) {
    private readonly bool _value = value;

    /// <summary>Asserts that the boolean value is <see langword="true"/>.</summary>
    /// <param name="message">
    /// An optional custom failure message that is included in the
    /// <see cref="AssertionFailedException"/> if the assertion fails.
    /// </param>
    /// <exception cref="AssertionFailedException">The value is <see langword="false"/>.</exception>
    public void BeTrue(string? message = null) {
        if (!_value) {
            throw new AssertionFailedException(message ?? "Expected true, but got false.");
        }
    }

    /// <summary>Asserts that the boolean value is <see langword="false"/>.</summary>
    /// <param name="message">
    /// An optional custom failure message that is included in the
    /// <see cref="AssertionFailedException"/> if the assertion fails.
    /// </param>
    /// <exception cref="AssertionFailedException">The value is <see langword="true"/>.</exception>
    public void BeFalse(string? message = null) {
        if (_value) {
            throw new AssertionFailedException(message ?? "Expected false, but got true.");
        }
    }
}
