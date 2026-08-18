//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.Testing.Assertions.Tests.Assertions;

/// <summary>Verifies the remaining <see cref="BoolAssertions"/> paths including BeFalse and custom messages.</summary>
[TestClass]
public sealed class BoolAssertionTests {
    /// <summary>BeFalse passes when the value is false.</summary>
    [TestMethod]
    public void BeFalse_WithFalse_Passes() {
        var value = bool.Parse("false");
        value.Should().BeFalse();
        Assert.IsFalse(value);
    }

    /// <summary>BeFalse throws when the value is true.</summary>
    [TestMethod]
    public void BeFalse_WithTrue_Throws() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(static () => true.Should().BeFalse());
        Assert.IsFalse(string.IsNullOrWhiteSpace(ex.Message));
    }

    /// <summary>A custom message appears in the exception when BeFalse fails.</summary>
    [TestMethod]
    public void BeFalse_WithCustomMessage_IncludesMessage() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(static () => true.Should().BeFalse("custom false msg"));
        Assert.AreEqual("custom false msg", ex.Message);
    }

    /// <summary>A custom message appears in the exception when BeTrue fails.</summary>
    [TestMethod]
    public void BeTrue_WithCustomMessage_IncludesMessage() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(static () => false.Should().BeTrue("custom true msg"));
        Assert.AreEqual("custom true msg", ex.Message);
    }
}
