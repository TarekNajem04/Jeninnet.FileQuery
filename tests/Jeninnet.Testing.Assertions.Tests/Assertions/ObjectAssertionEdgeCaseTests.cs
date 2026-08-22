//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.Testing.Assertions.Tests.Assertions;

/// <summary>Verifies error paths and custom message handling in <see cref="ObjectAssertions{T}"/>.</summary>
[TestClass]
public sealed class ObjectAssertionEdgeCaseTests {
    /// <summary>A custom message appears in the exception when Be fails.</summary>
    [TestMethod]
    public void Be_WithNonMatchingValue_WithCustomMessage_IncludesMessage() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(static () => ((object)"a").Should().Be("b", "custom be msg"));
        Assert.AreEqual("custom be msg", ex.Message);
    }

    /// <summary>Be passes when both the value and expected are null.</summary>
    [TestMethod]
    public void Be_WithNullExpected_PassesWhenValueIsNull() {
        object? nullObj = null;
        nullObj.Should().Be<object?>(null);
        Assert.IsNull(nullObj);
    }

    /// <summary>BeOfType throws when the value is null.</summary>
    [TestMethod]
    public void BeOfType_WithNullValue_Throws() {
        object? nullObj = null;
        var ex = Assert.ThrowsExactly<AssertionFailedException>(() => nullObj.Should().BeOfType<string>());
        Assert.IsFalse(string.IsNullOrWhiteSpace(ex.Message));
    }

    /// <summary>BeOfType throws when the runtime type does not match.</summary>
    [TestMethod]
    public void BeOfType_WithIncorrectType_Throws() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(static () => ((object)"hello").Should().BeOfType<int>());
        Assert.IsFalse(string.IsNullOrWhiteSpace(ex.Message));
    }

    /// <summary>A custom message appears in the exception when BeOfType fails.</summary>
    [TestMethod]
    public void BeOfType_WithCustomMessage_IncludesMessage() {
        object? nullObj = null;
        var ex = Assert.ThrowsExactly<AssertionFailedException>(() => nullObj.Should().BeOfType<string>("custom oftype msg"));
        Assert.AreEqual("custom oftype msg", ex.Message);
    }

    /// <summary>A custom message appears in the exception when BeNull fails.</summary>
    [TestMethod]
    public void BeNull_OnNonNull_WithCustomMessage_IncludesMessage() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(static () => ((object)"x").Should().BeNull("custom null msg"));
        Assert.AreEqual("custom null msg", ex.Message);
    }

    /// <summary>A custom message appears in the exception when NotBeNull fails.</summary>
    [TestMethod]
    public void NotBeNull_OnNull_WithCustomMessage_IncludesMessage() {
        object? nullObj = null;
        var ex = Assert.ThrowsExactly<AssertionFailedException>(() => nullObj.Should().NotBeNull("custom notnull msg"));
        Assert.AreEqual("custom notnull msg", ex.Message);
    }

    /// <summary>Be with values of different types (int vs string) throws AssertionFailedException.</summary>
    [TestMethod]
    public void Be_WithDifferentNumericTypes_Fails() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(static () => ((object)42).Should().Be("42"));
        Assert.IsFalse(string.IsNullOrWhiteSpace(ex.Message));
    }
}
