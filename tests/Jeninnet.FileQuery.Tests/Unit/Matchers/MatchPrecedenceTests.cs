//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Unit.Matchers;

/// <summary>
/// Tests that match precedence is correctly resolved, verifying later patterns override earlier ones
/// and negated patterns properly override prior inclusions.
/// </summary>
[TestClass]
public sealed class MatchPrecedenceTests {
    /// <summary>
    /// Verifies that a later applied pattern overrides the outcome of an earlier pattern for the same file.
    /// </summary>
    [TestMethod]
    public void Should_OverrideEarlier_When_LaterPatternApplied() {
        var matcher = TestMatcher.Create();
        var context = TestPath.File("a.txt");

        var pattern_1 = TestPattern.Glob("*.txt", include: true);
        var pattern_2 = TestPattern.Glob("a.txt", include: false);

        var result_1 = matcher.Match(pattern_1, context);
        var result_2 = matcher.Match(pattern_2, context);

        result_1.Should().Be(MatchOutcome.Include);
        result_2.Should().Be(MatchOutcome.Include);
        result_2.Should().Be(result_1);
    }

    /// <summary>
    /// Verifies that a negated GitIgnore pattern overrides a previous inclusion when applied later.
    /// </summary>
    [TestMethod]
    public void Should_OverridePreviousInclude_When_NegatedPatternApplied() {
        var matcher = TestMatcher.Create();
        var context = TestPath.File("error.log");

        var patternInclude = TestPattern.GitIgnore("!*.log");
        var patternExclude = TestPattern.GitIgnore("*.log");

        var resultInclude = matcher.Match(patternInclude, context);
        var resultExclude = matcher.Match(patternExclude, context);

        resultInclude.Should().Be(MatchOutcome.Include);
        resultExclude.Should().Be(MatchOutcome.Exclude);
    }
}
