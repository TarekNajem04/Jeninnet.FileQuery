namespace Jeninnet.Testing.Assertions.Tests.Collections;

/// <summary>Smoke tests for collection assertions: emptiness, containment, count, and equivalence.</summary>
[TestClass]
public sealed class CollectionAssertionSmokeTests {
    private static readonly int[] _empty = [];
    private static readonly int[] _single = [1];
    private static readonly int[] _three = [1, 2, 3];
    private static readonly int[] _two = [1, 2];

    /// <summary>BeEmpty passes on an empty collection.</summary>
    [TestMethod]
    public void BeEmpty_OnEmptyCollection_Passes() {
        _empty.Should().BeEmpty();
        Assert.IsEmpty(_empty);
    }

    /// <summary>BeEmpty throws on a non-empty collection.</summary>
    [TestMethod]
    public void BeEmpty_OnNonEmptyCollection_Throws() => Assert.ThrowsExactly<AssertionFailedException>(() => _single.Should().BeEmpty());

    /// <summary>Contain passes when the item is present.</summary>
    [TestMethod]
    public void Contain_Item_Passes() {
        _three.Should().Contain(2);
        Assert.IsTrue(_three.Contains(2));
    }

    /// <summary>Contain throws when the item is missing.</summary>
    [TestMethod]
    public void Contain_MissingItem_Throws() => Assert.ThrowsExactly<AssertionFailedException>(() => _two.Should().Contain(99));

    /// <summary>HaveCount passes when counts match.</summary>
    [TestMethod]
    public void HaveCount_WithMatchingCount_Passes() {
        _three.Should().HaveCount(3);
        Assert.HasCount(3, _three);
    }

    /// <summary>HaveCount throws when counts differ.</summary>
    [TestMethod]
    public void HaveCount_WithNonMatchingCount_Throws() => Assert.ThrowsExactly<AssertionFailedException>(() => _two.Should().HaveCount(5));

    /// <summary>BeEquivalentTo passes for permutations of the same elements.</summary>
    [TestMethod]
    public void BeEquivalentTo_WithSameElements_Passes() {
        _three.Should().BeEquivalentTo([3, 1, 2]);
        Assert.HasCount(3, _three);
    }

    /// <summary>BeEquivalentTo throws when element sets differ.</summary>
    [TestMethod]
    public void BeEquivalentTo_WithDifferentElements_Throws() => Assert.ThrowsExactly<AssertionFailedException>(() => _two.Should().BeEquivalentTo([1, 3]));
}
