namespace Jeninnet.FileQuery.Tests.Core;

/// <summary>
/// Contains tests for extension methods in the Core project.
/// </summary>
[TestClass]
public sealed class ExtensionTests {
    /// <summary>Tests MatchOutcomeExtensions_ShouldReturnCorrectBooleans.</summary>
    [TestMethod]
    public void MatchOutcomeExtensions_ShouldReturnCorrectBooleans() {
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

    /// <summary>Tests MatchResultExtensions_ShouldReturnCorrectBooleans.</summary>
    [TestMethod]
    public void MatchResultExtensions_ShouldReturnCorrectBooleans() {
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
