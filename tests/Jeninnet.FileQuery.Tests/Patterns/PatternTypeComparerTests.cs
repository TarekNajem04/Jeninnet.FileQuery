namespace Jeninnet.FileQuery.Tests.Patterns;

/// <summary>
/// Contains unit tests for the <see cref="PatternTypeComparer"/> class, ensuring correct comparison behavior for pattern kinds.
/// </summary>
[TestClass]
public sealed class PatternTypeComparerTests {
    /// <summary>
    /// Verifies that <see cref="PatternTypeComparer.Equals"/> returns <c>true</c> when comparing two identical pattern kinds.
    /// </summary>
    [TestMethod]
    public void Equals_SameKind_ReturnsTrue() {
        var comparer = new PatternTypeComparer();
        Assert.IsTrue(comparer.Equals(PatternKind.Glob, PatternKind.Glob));
    }

    /// <summary>
    /// Verifies that <see cref="PatternTypeComparer.Equals"/> returns <c>false</c> when comparing two different pattern kinds.
    /// </summary>
    [TestMethod]
    public void Equals_DifferentKind_ReturnsFalse() {
        var comparer = new PatternTypeComparer();
        Assert.IsFalse(comparer.Equals(PatternKind.Glob, PatternKind.Regex));
    }

    /// <summary>
    /// Verifies that <see cref="PatternTypeComparer.GetHashCode"/> returns the expected hash code for a valid pattern kind.
    /// </summary>
    [TestMethod]
    public void GetHashCode_ValidKind_ReturnsHashCode() {
        var comparer = new PatternTypeComparer();
        Assert.AreEqual(PatternKind.GitIgnore.GetHashCode(), comparer.GetHashCode(PatternKind.GitIgnore));
    }
}

