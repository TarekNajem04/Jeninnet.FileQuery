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

        result.HasRecursiveWildcard.Should().BeTrue();
        result.SegmentCount.Should().Be(2);
    }

    /// <summary>
    /// Verifies that Should DetectTrailingSlash When Analyzed.
    /// </summary>
    [TestMethod]
    public void Should_DetectTrailingSlash_When_Analyzed() {
        var analyzer = new PatternAnalyzer();
        var result = analyzer.Analyze("foo/");
        result.HasGitIgnoreSyntax.Should().BeTrue();
    }
}
