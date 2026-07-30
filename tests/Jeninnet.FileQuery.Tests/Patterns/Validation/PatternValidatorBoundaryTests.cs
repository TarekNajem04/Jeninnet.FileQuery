namespace Jeninnet.FileQuery.Tests.Patterns.Validation;

/// <summary>
/// Contains boundary unit tests for the <see cref="PatternValidator"/>, ensuring robust behavior for edge cases like null inputs, unicode, and extreme lengths.
/// </summary>
[TestClass]
public sealed class PatternValidatorBoundaryTests {
    /// <summary>
    /// Verifies that <see cref="PatternValidator.IsMalformed"/> correctly handles a null input by returning <c>false</c>.
    /// </summary>
    [TestMethod]
    public void IsMalformed_NullInput_ReturnsFalse() =>
        Assert.IsFalse(PatternValidator.IsMalformed(default));

    /// <summary>
    /// Verifies that <see cref="PatternValidator.IsMalformed"/> handles non-ASCII (Unicode) characters gracefully without flagging them as malformed.
    /// </summary>
    [TestMethod]
    public void IsMalformed_UnicodeCharacters_IsNotMalformed() =>
        Assert.IsFalse(PatternValidator.IsMalformed("file-🌸.txt".AsSpan()));

    /// <summary>
    /// Verifies that <see cref="PatternValidator.IsMalformed"/> handles an extremely long pattern string gracefully, ensuring no stack or performance issues.
    /// </summary>
    [TestMethod]
    public void IsMalformed_ExtremelyLongPattern_HandlesGracefully() {
        var longPattern = new string('a', 10000) + ".txt";
        Assert.IsFalse(PatternValidator.IsMalformed(longPattern.AsSpan()));
    }

    /// <summary>
    /// Verifies that <see cref="PatternValidator.IsMalformed"/> correctly identifies a complex mixed pattern as not malformed.
    /// </summary>
    [TestMethod]
    public void IsMalformed_ComplexMixedPattern_IsNotMalformed() =>
        Assert.IsFalse(PatternValidator.IsMalformed("!**/[a-z[:digit:]]/{sub,dir}/file-🌸.txt".AsSpan()));

    /// <summary>
    /// Verifies that <see cref="PatternValidator.HasStrayClosingBracket"/> correctly handles a null input by returning <c>false</c>.
    /// </summary>
    [TestMethod]
    public void HasStrayClosingBracket_NullInput_ReturnsFalse() => Assert.IsFalse(PatternValidator.HasStrayClosingBracket(default));
}

