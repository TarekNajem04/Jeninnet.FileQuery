namespace Jeninnet.FileQuery.Tests.Patterns;

[TestClass]
public class PatternClassifierAdditionalTests {
    [TestMethod]
    public void Classify_HandleVariousPrefixes() {
        // Test cases based on code review of PatternClassifier
        Assert.AreEqual(PatternKind.GitIgnore, PatternClassifier.Classify("!foo"));
        Assert.AreEqual(PatternKind.GitIgnore, PatternClassifier.Classify("/foo"));
        Assert.AreEqual(PatternKind.GitIgnore, PatternClassifier.Classify("foo/"));
        Assert.AreEqual(PatternKind.GitIgnore, PatternClassifier.Classify("**/foo"));
    }
}
