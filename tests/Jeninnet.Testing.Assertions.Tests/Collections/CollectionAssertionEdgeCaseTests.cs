//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.Testing.Assertions.Tests.Collections;

/// <summary>Verifies error paths, edge cases, and custom messages in <see cref="CollectionAssertions{T}"/>.</summary>
[TestClass]
public sealed class CollectionAssertionEdgeCaseTests {
    private static readonly int[] _emptyArray = [];
    private static readonly int[] _singleItem = [1];
    private static readonly int[] _twoItems = [1, 2];
    private static readonly int[] _threeItems = [1, 2, 3];

    /// <summary>BeEquivalentTo with a null collection throws AssertionFailedException with the standard message.</summary>
    [TestMethod]
    public void BeEquivalentTo_NullCollection_Throws() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(static () => NullCollection().Should().BeEquivalentTo(_emptyArray));
        Assert.IsFalse(string.IsNullOrWhiteSpace(ex.Message));
    }

    /// <summary>BeEquivalentTo with null expected throws ArgumentNullException.</summary>
    [TestMethod]
    public void BeEquivalentTo_NullExpected_Throws() => Assert.ThrowsExactly<ArgumentNullException>(static () => _emptyArray.Should().BeEquivalentTo(NullCollection()!));

    /// <summary>BeEquivalentTo with mismatched lengths throws AssertionFailedException.</summary>
    [TestMethod]
    public void BeEquivalentTo_MismatchedLength_Throws() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(static () => _threeItems.Should().BeEquivalentTo(_singleItem));
        Assert.IsFalse(string.IsNullOrWhiteSpace(ex.Message));
    }

    /// <summary>BeEquivalentTo with differing values throws AssertionFailedException.</summary>
    [TestMethod]
    public void BeEquivalentTo_ValueMismatch_Throws() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(static () => _threeItems.Should().BeEquivalentTo([1, 2, 4]));
        Assert.IsFalse(string.IsNullOrWhiteSpace(ex.Message));
    }

    /// <summary>A custom message appears when BeEquivalentTo fails due to a mismatched length.</summary>
    [TestMethod]
    public void BeEquivalentTo_WithCustomMessage_IncludesMessage() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(static () => _threeItems.Should().BeEquivalentTo(_singleItem, "custom be msg"));
        Assert.AreEqual("custom be msg", ex.Message);
    }

    /// <summary>ContainSingle with a null collection throws AssertionFailedException with the standard message.</summary>
    [TestMethod]
    public void ContainSingle_NullCollection_Throws() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(static () => NullCollection().Should().ContainSingle());
        Assert.IsFalse(string.IsNullOrWhiteSpace(ex.Message));
    }

    /// <summary>ContainSingle with an empty collection throws AssertionFailedException.</summary>
    [TestMethod]
    public void ContainSingle_EmptyCollection_Throws() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(static () => _emptyArray.Should().ContainSingle());
        Assert.IsFalse(string.IsNullOrWhiteSpace(ex.Message));
    }

    /// <summary>ContainSingle with multiple items throws AssertionFailedException.</summary>
    [TestMethod]
    public void ContainSingle_MultipleItems_Throws() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(static () => _threeItems.Should().ContainSingle());
        Assert.IsFalse(string.IsNullOrWhiteSpace(ex.Message));
    }

    /// <summary>A custom message appears when ContainSingle fails due to multiple items.</summary>
    [TestMethod]
    public void ContainSingle_WithCustomMessage_IncludesMessage() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(static () => _threeItems.Should().ContainSingle(message: "custom single msg"));
        Assert.AreEqual("custom single msg", ex.Message);
    }

    /// <summary>ContainSingle with a predicate fails when the collection is null.</summary>
    [TestMethod]
    public void ContainSingle_Predicate_NullCollection_Throws() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(static () => NullCollection().Should().ContainSingle(static x => x == 1));
        Assert.IsFalse(string.IsNullOrWhiteSpace(ex.Message));
    }

    /// <summary>ContainSingle with a predicate fails when no item matches.</summary>
    [TestMethod]
    public void ContainSingle_Predicate_NoMatch_Throws() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(static () => _threeItems.Should().ContainSingle(static x => x == 99));
        Assert.IsFalse(string.IsNullOrWhiteSpace(ex.Message));
    }

    /// <summary>ContainSingle with a predicate fails when multiple items match.</summary>
    [TestMethod]
    public void ContainSingle_Predicate_MultipleMatches_Throws() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(static () => _threeItems.Should().ContainSingle(static x => x >= 1));
        Assert.IsFalse(string.IsNullOrWhiteSpace(ex.Message));
    }

    /// <summary>A custom message appears when the predicate overload of ContainSingle fails with no match.</summary>
    [TestMethod]
    public void ContainSingle_Predicate_WithCustomMessage_IncludesMessage() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(static () => _threeItems.Should().ContainSingle(static x => x == 99, "custom pred msg"));
        Assert.AreEqual("custom pred msg", ex.Message);
    }

    /// <summary>NotBeEmpty with a null collection throws AssertionFailedException with the standard message.</summary>
    [TestMethod]
    public void NotBeEmpty_NullCollection_Throws() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(static () => NullCollection().Should().NotBeEmpty());
        Assert.IsFalse(string.IsNullOrWhiteSpace(ex.Message));
    }

    /// <summary>NotBeEmpty with an empty collection throws AssertionFailedException.</summary>
    [TestMethod]
    public void NotBeEmpty_EmptyCollection_Throws() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(static () => _emptyArray.Should().NotBeEmpty());
        Assert.IsFalse(string.IsNullOrWhiteSpace(ex.Message));
    }

    /// <summary>A custom message appears when NotBeEmpty fails on an empty collection.</summary>
    [TestMethod]
    public void NotBeEmpty_WithCustomMessage_IncludesMessage() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(static () => _emptyArray.Should().NotBeEmpty("custom empty msg"));
        Assert.AreEqual("custom empty msg", ex.Message);
    }

    /// <summary>NotContain with a null collection throws AssertionFailedException with the standard message.</summary>
    [TestMethod]
    public void NotContain_NullCollection_Throws() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(static () => NullCollection().Should().NotContain(1));
        Assert.IsFalse(string.IsNullOrWhiteSpace(ex.Message));
    }

    /// <summary>NotContain with a matching value throws AssertionFailedException.</summary>
    [TestMethod]
    public void NotContain_ItemPresent_Throws() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(static () => _threeItems.Should().NotContain(2));
        Assert.IsFalse(string.IsNullOrWhiteSpace(ex.Message));
    }

    /// <summary>NotContain passes when the value is not in the collection.</summary>
    [TestMethod]
    public void NotContain_ItemNotPresent_Passes() => _threeItems.Should().NotContain(99);

    /// <summary>A custom message appears when NotContain fails due to item present.</summary>
    [TestMethod]
    public void NotContain_WithCustomMessage_IncludesMessage() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(static () => _threeItems.Should().NotContain(1, "custom notcontain msg"));
        Assert.AreEqual("custom notcontain msg", ex.Message);
    }

    /// <summary>ContainSubset with a null superset throws AssertionFailedException with the standard message.</summary>
    [TestMethod]
    public void ContainSubset_NullSuperset_Throws() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(static () => NullCollection().Should().ContainSubset(_threeItems));
        Assert.IsFalse(string.IsNullOrWhiteSpace(ex.Message));
    }

    /// <summary>ContainSubset with a null subset throws ArgumentNullException.</summary>
    [TestMethod]
    public void ContainSubset_NullSubset_Throws() => Assert.ThrowsExactly<ArgumentNullException>(static () => _threeItems.Should().ContainSubset(NullCollection()!));

    /// <summary>ContainSubset fails when the subset is not fully contained in the superset.</summary>
    [TestMethod]
    public void ContainSubset_NotContained_Throws() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(static () => _threeItems.Should().ContainSubset([1, 4]));
        Assert.IsFalse(string.IsNullOrWhiteSpace(ex.Message));
    }

    /// <summary>A custom message appears when ContainSubset fails due to a missing item.</summary>
    [TestMethod]
    public void ContainSubset_WithCustomMessage_IncludesMessage() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(static () => _twoItems.Should().ContainSubset([1, 99], "custom subset msg"));
        Assert.AreEqual("custom subset msg", ex.Message);
    }

    /// <summary>BeEmpty throws when the collection is null.</summary>
    [TestMethod]
    public void BeEmpty_NullCollection_Throws() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(static () => NullCollection().Should().BeEmpty());
        Assert.IsFalse(string.IsNullOrWhiteSpace(ex.Message));
    }

    /// <summary>A custom message appears when BeEmpty fails on a non-empty collection.</summary>
    [TestMethod]
    public void BeEmpty_WithCustomMessage_IncludesMessage() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(static () => _threeItems.Should().BeEmpty("custom beempty msg"));
        Assert.AreEqual("custom beempty msg", ex.Message);
    }

    private static IEnumerable<int>? NullCollection() => null;
}
