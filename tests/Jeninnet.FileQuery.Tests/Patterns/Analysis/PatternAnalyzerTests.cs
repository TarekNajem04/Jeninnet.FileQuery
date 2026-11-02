//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
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

        result.HasRecursiveWildcard.Should().BeTrue();
        result.SegmentCount.Should().Be(2);
    }

    /// <summary>Tests Analyze_ShouldDetectTrailingSlash.</summary>
    [TestMethod]
    public void Analyze_ShouldDetectTrailingSlash() {
        var analyzer = new PatternAnalyzer();
        var result = analyzer.Analyze("foo/");
        result.HasGitIgnoreSyntax.Should().BeTrue();
    }

    /// <summary>An empty pattern produces an empty analysis result.</summary>
    [TestMethod]
    public void Analyze_EmptyPattern_ReturnsEmptyResult() {
        var analyzer = new PatternAnalyzer();

        var result = analyzer.Analyze("");

        result.IsEmpty.Should().BeTrue();
    }
}
