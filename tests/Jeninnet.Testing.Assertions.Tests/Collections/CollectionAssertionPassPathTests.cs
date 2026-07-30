namespace Jeninnet.Testing.Assertions.Tests.Collections;

/// <summary>Verifies passing paths for remaining collection assertion methods: HaveCount, Contain, and pass-through assertions.</summary>
[TestClass]
public sealed class CollectionAssertionPassPathTests {
    private static readonly int[] _threeItems = [1, 2, 3];
    private static readonly int[] _singleItem = [1];

    /// <summary>HaveCount passes when the collection has exactly the expected number of items.</summary>
    [TestMethod]
    public void HaveCount_WithMatchingCount_Passes() => _threeItems.Should().HaveCount(3);

    /// <summary>HaveCount with a null collection throws AssertionFailedException.</summary>
    [TestMethod]
    public void HaveCount_NullCollection_Throws() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(() => NullCollection().Should().HaveCount(0));
        Assert.IsFalse(string.IsNullOrWhiteSpace(ex.Message));
    }

    /// <summary>HaveCount with a mismatched count throws AssertionFailedException.</summary>
    [TestMethod]
    public void HaveCount_Mismatch_Throws() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(() => _threeItems.Should().HaveCount(1));
        Assert.IsFalse(string.IsNullOrWhiteSpace(ex.Message));
    }

    /// <summary>A custom message appears when HaveCount fails.</summary>
    [TestMethod]
    public void HaveCount_WithCustomMessage_IncludesMessage() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(() => _threeItems.Should().HaveCount(99, "custom count msg"));
        Assert.AreEqual("custom count msg", ex.Message);
    }

    /// <summary>Contain with a predicate passes when a matching item is found.</summary>
    [TestMethod]
    public void Contain_Predicate_WithMatch_Passes() => _threeItems.Should().Contain(x => x == 2);

    /// <summary>Contain with a predicate on a null collection throws AssertionFailedException.</summary>
    [TestMethod]
    public void Contain_Predicate_NullCollection_Throws() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(() => NullCollection().Should().Contain(x => x == 1));
        Assert.IsFalse(string.IsNullOrWhiteSpace(ex.Message));
    }

    /// <summary>Contain with a predicate and no match throws AssertionFailedException.</summary>
    [TestMethod]
    public void Contain_Predicate_NoMatch_Throws() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(() => _threeItems.Should().Contain(x => x == 99));
        Assert.IsFalse(string.IsNullOrWhiteSpace(ex.Message));
    }

    /// <summary>A custom message appears when Contain predicate fails.</summary>
    [TestMethod]
    public void Contain_Predicate_WithCustomMessage_IncludesMessage() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(() => _threeItems.Should().Contain(x => x == 99, "custom contain pred msg"));
        Assert.AreEqual("custom contain pred msg", ex.Message);
    }

    /// <summary>BeEquivalentTo passes when collections have the same items in any order.</summary>
    [TestMethod]
    public void BeEquivalentTo_WithSameItems_Passes() => _threeItems.Should().BeEquivalentTo([3, 1, 2]);

    /// <summary>Contain with an item passes when the item exists in the collection.</summary>
    [TestMethod]
    public void Contain_Item_WithMatch_Passes() => _threeItems.Should().Contain(2);

    /// <summary>Contain with an item on a null collection throws.</summary>
    [TestMethod]
    public void Contain_Item_NullCollection_Throws() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(() => NullCollection().Should().Contain(1));
        Assert.IsFalse(string.IsNullOrWhiteSpace(ex.Message));
    }

    /// <summary>NotContain with a predicate passes when no item matches.</summary>
    [TestMethod]
    public void NotContain_Predicate_NoMatch_Passes() => _threeItems.Should().NotContain(x => x == 99);

    /// <summary>ContainSingle passes when exactly one item exists in the collection.</summary>
    [TestMethod]
    public void ContainSingle_WithSingleItem_Passes() {
        var result = _singleItem.Should().ContainSingle();
        Assert.AreEqual(1, result.Which);
    }

    /// <summary>ContainSingle with a predicate passes when exactly one item matches.</summary>
    [TestMethod]
    public void ContainSingle_Predicate_WithMatch_Passes() {
        var result = _threeItems.Should().ContainSingle(x => x == 2);
        Assert.AreEqual(2, result.Which);
    }

    /// <summary>NotContain predicate on a null collection throws.</summary>
    [TestMethod]
    public void NotContain_Predicate_NullCollection_Throws() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(() => NullCollection().Should().NotContain(x => x == 1));
        Assert.IsFalse(string.IsNullOrWhiteSpace(ex.Message));
    }

    /// <summary>NotContain predicate throws when a matching item is found.</summary>
    [TestMethod]
    public void NotContain_Predicate_WithMatch_Throws() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(() => _threeItems.Should().NotContain(x => x == 2));
        Assert.IsFalse(string.IsNullOrWhiteSpace(ex.Message));
    }

    /// <summary>Contain with an item not in the collection throws.</summary>
    [TestMethod]
    public void Contain_Item_NotFound_Throws() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(() => _singleItem.Should().Contain(99));
        Assert.IsFalse(string.IsNullOrWhiteSpace(ex.Message));
    }

    /// <summary>A custom message appears when Contain item fails.</summary>
    [TestMethod]
    public void Contain_Item_WithCustomMessage_IncludesMessage() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(() => _singleItem.Should().Contain(99, "custom contain item msg"));
        Assert.AreEqual("custom contain item msg", ex.Message);
    }

    private static IEnumerable<int>? NullCollection() => null;
}
