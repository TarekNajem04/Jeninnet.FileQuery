//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Unit.Patterns.Analysis;

/// <summary>
/// Tests for PatternAnalyzerTests.
/// </summary>
[TestClass]
public class PatternAnalyzerTests {
    /// <summary>
    /// Verifies that Should DetectRecursiveWildcard When Analyzed.
    /// </summary>
    [TestMethod]
    public void Should_DetectRecursiveWildcard_When_Analyzed() {
        var analyzer = new PatternAnalyzer();

        var result = analyzer.Analyze("**/*.cs");

        Assert.IsTrue(result.HasRecursiveWildcard);
        Assert.AreEqual(2, result.SegmentCount);
    }

    /// <summary>
    /// Verifies that Should DetectTrailingSlash When Analyzed.
    /// </summary>
    [TestMethod]
    public void Should_DetectTrailingSlash_When_Analyzed() {
        var analyzer = new PatternAnalyzer();
        var result = analyzer.Analyze("foo/");
        Assert.IsTrue(result.HasGitIgnoreSyntax);
    }
}
