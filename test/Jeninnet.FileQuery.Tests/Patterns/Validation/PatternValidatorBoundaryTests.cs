namespace Jeninnet.FileQuery.Tests.Patterns.Validation;

[TestClass]
public sealed class PatternValidatorBoundaryTests
{
    [TestMethod]
    public void IsMalformed_NullInput_ReturnsFalse() =>
        // Boundary: Is it safe against null?
        Assert.IsFalse(PatternValidator.IsMalformed(default));

    [TestMethod]
    public void IsMalformed_UnicodeCharacters_IsNotMalformed() =>
        // Boundary: Ensure it handles non-ASCII characters gracefully.
        Assert.IsFalse(PatternValidator.IsMalformed("file-🌸.txt".AsSpan()));

    [TestMethod]
    public void IsMalformed_ExtremelyLongPattern_HandlesGracefully()
    {
        // Boundary: Test with a very long pattern to check for stack/performance issues.
        var longPattern = new string('a', 10000) + ".txt";
        Assert.IsFalse(PatternValidator.IsMalformed(longPattern.AsSpan()));
    }

    [TestMethod]
    public void IsMalformed_ComplexMixedPattern_IsNotMalformed() =>
        // Boundary: Complex combination.
        Assert.IsFalse(PatternValidator.IsMalformed("!**/[a-z[:digit:]]/{sub,dir}/file-🌸.txt".AsSpan()));

    [TestMethod]
    public void HasStrayClosingBracket_NullInput_ReturnsFalse() =>
        Assert.IsFalse(PatternValidator.HasStrayClosingBracket(default));
}
