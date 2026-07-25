namespace Jeninnet.FileQuery.Tests.Patterns.Analysis;

[TestClass]
public class PatternAnalyzerTests {
    [TestMethod]
    public void Analyze_ShouldDetectRecursiveWildcard() {
        var analyzer = new PatternAnalyzer();

        var result = analyzer.Analyze("**/*.cs");

        Assert.IsTrue(result.HasRecursiveWildcard);
        Assert.AreEqual(2, result.SegmentCount);
    }

    [TestMethod]
    public void Analyze_ShouldDetectTrailingSlash() {
        var analyzer = new PatternAnalyzer();
        var result = analyzer.Analyze("foo/");
        Assert.IsTrue(result.HasGitIgnoreSyntax);
    }
}
