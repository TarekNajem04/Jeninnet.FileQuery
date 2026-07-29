namespace Jeninnet.Testing.Assertions.Tests.Collections;

[TestClass]
public sealed class CollectionAssertionSmokeTests {
    private static readonly int[] _empty = [];
    private static readonly int[] _single = [1];
    private static readonly int[] _three = [1, 2, 3];
    private static readonly int[] _two = [1, 2];

    [TestMethod]
    public void BeEmpty_OnEmptyCollection_Passes() {
        _empty.Should().BeEmpty();
    }

    [TestMethod]
    public void BeEmpty_OnNonEmptyCollection_Throws() {
        Assert.ThrowsExactly<AssertionFailedException>(() => _single.Should().BeEmpty());
    }

    [TestMethod]
    public void Contain_Item_Passes() {
        _three.Should().Contain(2);
    }

    [TestMethod]
    public void Contain_MissingItem_Throws() {
        Assert.ThrowsExactly<AssertionFailedException>(() => _two.Should().Contain(99));
    }

    [TestMethod]
    public void HaveCount_WithMatchingCount_Passes() {
        _three.Should().HaveCount(3);
    }

    [TestMethod]
    public void HaveCount_WithNonMatchingCount_Throws() {
        Assert.ThrowsExactly<AssertionFailedException>(() => _two.Should().HaveCount(5));
    }

    [TestMethod]
    public void BeEquivalentTo_WithSameElements_Passes() {
        _three.Should().BeEquivalentTo([3, 1, 2]);
    }

    [TestMethod]
    public void BeEquivalentTo_WithDifferentElements_Throws() {
        Assert.ThrowsExactly<AssertionFailedException>(() => _two.Should().BeEquivalentTo([1, 3]));
    }
}
