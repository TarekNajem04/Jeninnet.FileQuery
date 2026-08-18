namespace Jeninnet.FileQuery.Tests.Patterns;

/// <summary>
/// Contains unit tests for the PatternClassifier functionality.
/// </summary>
[TestClass]
public sealed class PatternClassifierTests {
    /// <summary>
    /// Verifies that the Classify method correctly identifies various pattern types across different input scenarios.
    /// </summary>
    [TestMethod]
    public void Classify_ShouldHandleAllBranches() {
        // Unknown (Malformed)
        Assert.AreEqual(PatternKind.Unknown, PatternClassifier.Classify("[a-z"));

        // Empty
        Assert.AreEqual(PatternKind.GitIgnore, PatternClassifier.Classify(""));

        // Regex
        Assert.AreEqual(PatternKind.Regex, PatternClassifier.Classify("r:.*"));
        Assert.AreEqual(PatternKind.GitIgnore, PatternClassifier.Classify("regex:^.*$"));
        Assert.AreEqual(PatternKind.GitIgnore, PatternClassifier.Classify("regex:"));

        // Glob (Stray bracket)
        Assert.AreEqual(PatternKind.Glob, PatternClassifier.Classify("a]"));

        // Glob (Windows Path)
        Assert.AreEqual(PatternKind.Glob, PatternClassifier.Classify(@"C:\foo"));
        Assert.AreEqual(PatternKind.Unknown, PatternClassifier.Classify(@"C:\"));

        // Glob (Windows Path - negative)
        Assert.AreNotEqual(PatternKind.Glob, PatternClassifier.Classify(@"!C:\foo"));

        // Literal with escaped characters (no backslash)
        Assert.AreEqual(PatternKind.GitIgnore, PatternClassifier.Classify(@"\!foo"));

        // GitIgnore (GitIgnore syntax)
        Assert.AreEqual(PatternKind.GitIgnore, PatternClassifier.Classify("!foo"));

        // GitIgnore (Wildcard)
        Assert.AreEqual(PatternKind.GitIgnore, PatternClassifier.Classify("*"));

        // Literal
        Assert.AreEqual(PatternKind.GitIgnore, PatternClassifier.Classify("foo"));
    }
}
