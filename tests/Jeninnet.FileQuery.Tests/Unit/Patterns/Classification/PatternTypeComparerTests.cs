namespace Jeninnet.FileQuery.Tests.Unit.Patterns.Classification;

/// <summary>
/// Tests for PatternTypeComparerTests.
/// </summary>
[TestClass]
public sealed class PatternTypeComparerTests {
    /// <summary>
    /// Verifies that Should ReturnTrue When SameKindCompared.
    /// </summary>
    [TestMethod]
    public void Should_ReturnTrue_When_SameKindCompared() {
        var comparer = new PatternTypeComparer();
        Assert.IsTrue(comparer.Equals(PatternKind.Glob, PatternKind.Glob));
    }

    /// <summary>
    /// Verifies that Should ReturnFalse When DifferentKindCompared.
    /// </summary>
    [TestMethod]
    public void Should_ReturnFalse_When_DifferentKindCompared() {
        var comparer = new PatternTypeComparer();
        Assert.IsFalse(comparer.Equals(PatternKind.Glob, PatternKind.Regex));
    }

    /// <summary>
    /// Verifies that Should ReturnHashCode When ValidKindUsed.
    /// </summary>
    [TestMethod]
    public void Should_ReturnHashCode_When_ValidKindUsed() {
        var comparer = new PatternTypeComparer();
        Assert.AreEqual(PatternKind.GitIgnore.GetHashCode(), comparer.GetHashCode(PatternKind.GitIgnore));
    }
}

