namespace Jeninnet.FileQuery.Tests.Patterns.Analysis;

/// <summary>
/// Contains unit tests for the <see cref="PatternAnalyzer"/> class.
/// </summary>
[TestClass]
public class PatternAnalyzerTests {
    /// <summary>Tests Analyze_ShouldDetectRecursiveWildcard.</summary>
    [TestMethod]
    public void Analyze_ShouldDetectRecursiveWildcard() {
        var analyzer = new PatternAnalyzer();

        var result = analyzer.Analyze("**/*.cs");

        Assert.IsTrue(result.HasRecursiveWildcard);
        Assert.AreEqual(2, result.SegmentCount);
    }

    /// <summary>Tests Analyze_ShouldDetectTrailingSlash.</summary>
    [TestMethod]
    public void Analyze_ShouldDetectTrailingSlash() {
        var analyzer = new PatternAnalyzer();
        var result = analyzer.Analyze("foo/");
        Assert.IsTrue(result.HasGitIgnoreSyntax);
    }
}

