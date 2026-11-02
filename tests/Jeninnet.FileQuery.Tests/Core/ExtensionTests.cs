//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Core;

/// <summary>
/// Contains tests for extension methods in the Core project.
/// </summary>
[TestClass]
public sealed class ExtensionTests {
    /// <summary>Tests MatchOutcomeExtensions_ShouldReturnCorrectBooleans.</summary>
    [TestMethod]
    public void MatchOutcomeExtensions_ShouldReturnCorrectBooleans() {
        MatchOutcome.Include.IsSuccess().Should().BeTrue();
        MatchOutcome.Include.IsIncluded().Should().BeTrue();
        MatchOutcome.Include.IsExcluded().Should().BeFalse();
        MatchOutcome.Include.IsUnmatched().Should().BeFalse();

        MatchOutcome.Exclude.IsSuccess().Should().BeFalse();
        MatchOutcome.Exclude.IsIncluded().Should().BeFalse();
        MatchOutcome.Exclude.IsExcluded().Should().BeTrue();
        MatchOutcome.Exclude.IsUnmatched().Should().BeFalse();

        MatchOutcome.NoMatch.IsSuccess().Should().BeFalse();
        MatchOutcome.NoMatch.IsIncluded().Should().BeFalse();
        MatchOutcome.NoMatch.IsExcluded().Should().BeFalse();
        MatchOutcome.NoMatch.IsUnmatched().Should().BeTrue();
    }

    /// <summary>Tests MatchResultExtensions_ShouldReturnCorrectBooleans.</summary>
    [TestMethod]
    public void MatchResultExtensions_ShouldReturnCorrectBooleans() {
        var includeResult = MatchResult.Included().Match();
        includeResult.IsSuccess().Should().BeTrue();
        includeResult.IsExcluded().Should().BeFalse();
        includeResult.IsUnmatched().Should().BeFalse();
        includeResult.ShouldYield().Should().BeTrue();
        includeResult.ToDebugString().Should().Be("Matched: True, Included: True");

        var excludeResult = MatchResult.Fail().Match();
        excludeResult.IsSuccess().Should().BeFalse();
        excludeResult.IsExcluded().Should().BeTrue();
        excludeResult.IsUnmatched().Should().BeFalse();
        excludeResult.ShouldYield().Should().BeFalse();
        excludeResult.ToDebugString().Should().Be("Matched: True, Included: False");

        var noMatchResult = MatchResult.Fail();
        noMatchResult.IsSuccess().Should().BeFalse();
        noMatchResult.IsExcluded().Should().BeFalse();
        noMatchResult.IsUnmatched().Should().BeTrue();
        noMatchResult.ShouldYield().Should().BeTrue();
        noMatchResult.ToDebugString().Should().Be("Matched: False, Included: False");
    }
}
