namespace Jeninnet.FileQuery.Tests.Unit.Matchers;

/// <summary>
/// Tests that match operations are deterministic, producing the same outcome for identical inputs
/// and correctly returning NoMatch when no pattern matches.
/// </summary>
[TestClass]
public sealed class MatchDeterminismTests {
    /// <summary>
    /// Verifies that matching the same pattern against the same context always yields the same outcome.
    /// </summary>
    [TestMethod]
    public void Should_YieldSameOutcome_When_SameInputUsed() {
        // Arrange
        var pattern = TestPattern.Glob("*.txt");
        var matcher = TestMatcher.Create();
        var context = TestPath.File("readme.txt");

        // Act
        var first = matcher.Match(pattern, context);
        var second = matcher.Match(pattern, context);

        // Assert
        Assert.AreEqual(first, second);
    }

    /// <summary>
    /// Verifies that a non-matching pattern returns NoMatch when the file name does not match.
    /// </summary>
    [TestMethod]
    public void Should_YieldNoMatch_When_NoPatternMatches() {
        var pattern = TestPattern.Glob("*.md");
        var matcher = TestMatcher.Create();
        var context = TestPath.File("readme.txt");

        var result = matcher.Match(pattern, context);

        Assert.AreEqual(MatchOutcome.NoMatch, result);
    }
}

