namespace Jeninnet.FileQuery.Tests.Unit.Patterns.Classification;

/// <summary>
/// Tests for PatternClassifierAdditionalTests.
/// </summary>
[TestClass]
public class PatternClassifierAdditionalTests {
    /// <summary>
    /// Verifies that Should HandleVariousPrefixes When Classified.
    /// </summary>
    [TestMethod]
    public void Should_HandleVariousPrefixes_When_Classified() {
        // Test cases based on code review of PatternClassifier
        Assert.AreEqual(PatternKind.GitIgnore, PatternClassifier.Classify("!foo"));
        Assert.AreEqual(PatternKind.GitIgnore, PatternClassifier.Classify("/foo"));
        Assert.AreEqual(PatternKind.GitIgnore, PatternClassifier.Classify("foo/"));
        Assert.AreEqual(PatternKind.GitIgnore, PatternClassifier.Classify("**/foo"));
    }
}
