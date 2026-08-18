namespace Jeninnet.FileQuery.Tests.Unit.Engine;

/// <summary>
/// Tests for extension methods on MatchOutcome and MatchResult types.
/// </summary>
[TestClass]
public sealed class ExtensionTests {
    /// <summary>
    /// Verifies that MatchOutcome extension methods return the correct boolean values for each outcome.
    /// </summary>
    [TestMethod]
    public void Should_ReturnCorrectBooleans_When_MatchOutcomeUsed() {
        Assert.IsTrue(MatchOutcome.Include.IsSuccess());
        Assert.IsTrue(MatchOutcome.Include.IsIncluded());
        Assert.IsFalse(MatchOutcome.Include.IsExcluded());
        Assert.IsFalse(MatchOutcome.Include.IsUnmatched());

        Assert.IsFalse(MatchOutcome.Exclude.IsSuccess());
        Assert.IsFalse(MatchOutcome.Exclude.IsIncluded());
        Assert.IsTrue(MatchOutcome.Exclude.IsExcluded());
        Assert.IsFalse(MatchOutcome.Exclude.IsUnmatched());

        Assert.IsFalse(MatchOutcome.NoMatch.IsSuccess());
        Assert.IsFalse(MatchOutcome.NoMatch.IsIncluded());
        Assert.IsFalse(MatchOutcome.NoMatch.IsExcluded());
        Assert.IsTrue(MatchOutcome.NoMatch.IsUnmatched());
    }

    /// <summary>
    /// Verifies that MatchResult extension methods return the correct boolean values and debug strings.
    /// </summary>
    [TestMethod]
    public void Should_ReturnCorrectBooleans_When_MatchResultUsed() {
        var includeResult = MatchResult.Included().Match();
        Assert.IsTrue(includeResult.IsSuccess());
        Assert.IsFalse(includeResult.IsExcluded());
        Assert.IsFalse(includeResult.IsUnmatched());
        Assert.IsTrue(includeResult.ShouldYield());
        Assert.AreEqual("Matched: True, Included: True", includeResult.ToDebugString());

        var excludeResult = MatchResult.Fail().Match();
        Assert.IsFalse(excludeResult.IsSuccess());
        Assert.IsTrue(excludeResult.IsExcluded());
        Assert.IsFalse(excludeResult.IsUnmatched());
        Assert.IsFalse(excludeResult.ShouldYield());
        Assert.AreEqual("Matched: True, Included: False", excludeResult.ToDebugString());

        var noMatchResult = MatchResult.Fail();
        Assert.IsFalse(noMatchResult.IsSuccess());
        Assert.IsFalse(noMatchResult.IsExcluded());
        Assert.IsTrue(noMatchResult.IsUnmatched());
        Assert.IsTrue(noMatchResult.ShouldYield());
        Assert.AreEqual("Matched: False, Included: False", noMatchResult.ToDebugString());
    }
}
