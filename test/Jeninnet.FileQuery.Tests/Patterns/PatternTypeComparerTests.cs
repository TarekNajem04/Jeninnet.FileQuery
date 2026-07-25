using Jeninnet.FileQuery.Patterns;

namespace Jeninnet.FileQuery.Tests.Patterns;

[TestClass]
public sealed class PatternTypeComparerTests {
    [TestMethod]
    public void Equals_SameKind_ReturnsTrue() {
        var comparer = new PatternTypeComparer();
        Assert.IsTrue(comparer.Equals(PatternKind.Glob, PatternKind.Glob));
    }

    [TestMethod]
    public void Equals_DifferentKind_ReturnsFalse() {
        var comparer = new PatternTypeComparer();
        Assert.IsFalse(comparer.Equals(PatternKind.Glob, PatternKind.Regex));
    }

    [TestMethod]
    public void GetHashCode_ValidKind_ReturnsHashCode() {
        var comparer = new PatternTypeComparer();
        Assert.AreEqual(PatternKind.GitIgnore.GetHashCode(), comparer.GetHashCode(PatternKind.GitIgnore));
    }
}
