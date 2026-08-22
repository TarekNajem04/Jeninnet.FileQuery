//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Correctness;

/// <summary>
/// Contains tests for validating the precedence rules of patterns in the matcher engine.
/// </summary>
[TestClass]
public sealed class MatchPrecedenceTests {
    /// <summary>Tests LaterPatternOverridesEarlier.</summary>
    [TestMethod]
    public void LaterPatternOverridesEarlier() {
        var matcher = TestMatcher.Create();
        var context = TestPath.File("a.txt");

        var pattern_1 = TestPattern.Glob("*.txt", include: true);
        var pattern_2 = TestPattern.Glob("a.txt", include: false);

        var result_1 = matcher.Match(pattern_1, context);
        var result_2 = matcher.Match(pattern_2, context);

        Assert.AreEqual(MatchOutcome.Include, result_1);
        Assert.AreEqual(MatchOutcome.Include, result_2);
        Assert.AreEqual(result_1, result_2);
    }

    /// <summary>Tests NegatedPatternOverridesPreviousInclude.</summary>
    [TestMethod]
    public void NegatedPatternOverridesPreviousInclude() {
        var matcher = TestMatcher.Create();
        var context = TestPath.File("error.log");

        var patternInclude = TestPattern.GitIgnore("!*.log");
        var patternExclude = TestPattern.GitIgnore("*.log");

        var resultInclude = matcher.Match(patternInclude, context);
        var resultExclude = matcher.Match(patternExclude, context);

        Assert.AreEqual(MatchOutcome.Include, resultInclude);
        Assert.AreEqual(MatchOutcome.Exclude, resultExclude);
    }
}
