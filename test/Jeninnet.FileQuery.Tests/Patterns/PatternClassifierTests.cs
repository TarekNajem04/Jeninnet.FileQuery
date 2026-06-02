namespace Jeninnet.FileQuery.Tests.Patterns;

[TestClass]
public sealed class PatternClassifierTests {
    [TestMethod]
    public void Classify_ShouldHandleAllBranches() {
        // Unknown (Malformed)
        Assert.AreEqual(PatternKind.Unknown, PatternClassifier.Classify("[a-z"));

        // Empty
        Assert.AreEqual(PatternKind.GitIgnore, PatternClassifier.Classify(""));

        // Regex
        Assert.AreEqual(PatternKind.Regex, PatternClassifier.Classify("r:.*"));

        // Glob (Stray bracket)
        Assert.AreEqual(PatternKind.Glob, PatternClassifier.Classify("a]"));

        // Glob (Windows Path)
        Assert.AreEqual(PatternKind.Glob, PatternClassifier.Classify(@"C:\foo"));

        // GitIgnore (Escaped)
        Assert.AreEqual(PatternKind.GitIgnore, PatternClassifier.Classify(@"\!foo"));

        // GitIgnore (GitIgnore syntax)
        Assert.AreEqual(PatternKind.GitIgnore, PatternClassifier.Classify("!foo"));

        // GitIgnore (Wildcard)
        Assert.AreEqual(PatternKind.GitIgnore, PatternClassifier.Classify("*"));

        // Literal
        Assert.AreEqual(PatternKind.GitIgnore, PatternClassifier.Classify("foo"));
    }
}
