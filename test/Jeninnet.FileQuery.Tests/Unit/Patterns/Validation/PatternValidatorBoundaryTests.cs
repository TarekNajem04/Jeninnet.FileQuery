namespace Jeninnet.FileQuery.Tests.Unit.Patterns.Validation;

/// <summary>
/// Boundary tests for <see cref="PatternValidator"/> verifying resilience against
/// extreme inputs such as null, Unicode characters, and very long patterns.
/// </summary>
[TestClass]
public sealed class PatternValidatorBoundaryTests {
    /// <summary>
    /// Verifies that null input does not throw and returns false.
    /// </summary>
    [TestMethod]
    public void Should_ReturnFalse_When_NullInput() =>
        // Boundary: Is it safe against null?
        Assert.IsFalse(PatternValidator.IsMalformed(default));

    /// <summary>
    /// Verifies that Unicode characters in the pattern are handled gracefully without errors.
    /// </summary>
    [TestMethod]
    public void Should_NotBeMalformed_When_UnicodeCharacters() =>
        // Boundary: Ensure it handles non-ASCII characters gracefully.
        Assert.IsFalse(PatternValidator.IsMalformed("file-🌸.txt".AsSpan()));

    /// <summary>
    /// Verifies that an extremely long pattern does not cause stack overflow or performance issues.
    /// </summary>
    [TestMethod]
    public void Should_HandleGracefully_When_ExtremelyLongPattern() {
        // Boundary: Test with a very long pattern to check for stack/performance issues.
        var longPattern = new string('a', 10000) + ".txt";
        Assert.IsFalse(PatternValidator.IsMalformed(longPattern.AsSpan()));
    }

    /// <summary>
    /// Verifies that a complex pattern combining negation, globs, ranges, POSIX classes, and Unicode is not malformed.
    /// </summary>
    [TestMethod]
    public void Should_NotBeMalformed_When_ComplexMixedPattern() =>
        // Boundary: Complex combination.
        Assert.IsFalse(PatternValidator.IsMalformed("!**/[a-z[:digit:]]/{sub,dir}/file-🌸.txt".AsSpan()));

    /// <summary>
    /// Verifies that null input does not throw and returns false for stray bracket detection.
    /// </summary>
    [TestMethod]
    public void Should_ReturnFalseForStrayBracket_When_NullInput() => Assert.IsFalse(PatternValidator.HasStrayClosingBracket(default));
}

