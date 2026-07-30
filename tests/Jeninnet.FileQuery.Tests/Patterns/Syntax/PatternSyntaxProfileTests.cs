namespace Jeninnet.FileQuery.Tests.Patterns.Syntax;

/// <summary>
/// Contains unit tests for the <see cref="PatternSyntaxProfile"/> class, focusing on profile resolution.
/// </summary>
[TestClass]
public sealed class PatternSyntaxProfileTests {
    /// <summary>
    /// Verifies that <see cref="PatternSyntaxProfile.GetProfileForPatternType"/> returns the expected profile for each <see cref="PatternKind"/>.
    /// </summary>
    [TestMethod]
    public void GetProfileForPatternType_ShouldReturnCorrectProfile() {
        Assert.AreEqual(PatternSyntaxProfile.GitIgnore, PatternSyntaxProfile.GetProfileForPatternType(PatternKind.GitIgnore));
        Assert.AreEqual(PatternSyntaxProfile.Glob, PatternSyntaxProfile.GetProfileForPatternType(PatternKind.Glob));
        Assert.AreEqual(PatternSyntaxProfile.Regex, PatternSyntaxProfile.GetProfileForPatternType(PatternKind.Regex));
        Assert.AreEqual(PatternSyntaxProfile.Default, PatternSyntaxProfile.GetProfileForPatternType(PatternKind.Unknown));
    }
}

