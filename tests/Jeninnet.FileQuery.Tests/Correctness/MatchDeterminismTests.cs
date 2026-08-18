namespace Jeninnet.FileQuery.Tests.Correctness;

/// <summary>
/// Contains tests for ensuring consistent matching behavior of the matcher engine.
/// </summary>
[TestClass]
public sealed class MatchDeterminismTests {
    /// <summary>Tests SameInput_YieldsSameOutcome.</summary>
    [TestMethod]
    public void SameInput_YieldsSameOutcome() {
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

    /// <summary>Tests NoMatchingPattern_YieldsNoMatch.</summary>
    [TestMethod]
    public void NoMatchingPattern_YieldsNoMatch() {
        var pattern = TestPattern.Glob("*.md");
        var matcher = TestMatcher.Create();
        var context = TestPath.File("readme.txt");

        var result = matcher.Match(pattern, context);

        Assert.AreEqual(MatchOutcome.NoMatch, result);
    }
}
