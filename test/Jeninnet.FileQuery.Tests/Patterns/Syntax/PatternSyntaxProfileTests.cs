namespace Jeninnet.FileQuery.Tests.Patterns.Syntax;

[TestClass]
public sealed class PatternSyntaxProfileTests
{
    [TestMethod]
    public void GetProfileForPatternType_ShouldReturnCorrectProfile()
    {
        Assert.AreEqual(PatternSyntaxProfile.GitIgnore, PatternSyntaxProfile.GetProfileForPatternType(PatternKind.GitIgnore));
        Assert.AreEqual(PatternSyntaxProfile.Glob, PatternSyntaxProfile.GetProfileForPatternType(PatternKind.Glob));
        Assert.AreEqual(PatternSyntaxProfile.Regex, PatternSyntaxProfile.GetProfileForPatternType(PatternKind.Regex));
        Assert.AreEqual(PatternSyntaxProfile.Default, PatternSyntaxProfile.GetProfileForPatternType(PatternKind.Unknown));
    }
}
