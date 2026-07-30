namespace Jeninnet.Testing.Assertions.Tests.Strings;

/// <summary>Verifies error paths, edge cases, and custom messages in <see cref="StringAssertions"/>.</summary>
[TestClass]
public sealed class StringAssertionEdgeCaseTests {
    /// <summary>Contain with a null actual string throws AssertionFailedException.</summary>
    [TestMethod]
    public void Contain_NullActual_Throws() {
        const string? nullStr = null;
        var ex = Assert.ThrowsExactly<AssertionFailedException>(static () => nullStr.Should().Contain("x"));
        Assert.IsFalse(string.IsNullOrWhiteSpace(ex.Message));
    }

    /// <summary>Contain with a substring not present in the value throws AssertionFailedException.</summary>
    [TestMethod]
    public void Contain_NotPresent_Throws() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(static () => "hello".Should().Contain("world"));
        Assert.IsFalse(string.IsNullOrWhiteSpace(ex.Message));
    }

    /// <summary>A custom message appears when Contain fails.</summary>
    [TestMethod]
    public void Contain_WithCustomMessage_IncludesMessage() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(static () => "a".Should().Contain("b", "custom contain msg"));
        Assert.AreEqual("custom contain msg", ex.Message);
    }

    /// <summary>A custom message appears when Be fails on strings.</summary>
    [TestMethod]
    public void Be_WithCustomMessage_IncludesMessage() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(static () => "a".Should().Be("b", "custom be msg"));
        Assert.AreEqual("custom be msg", ex.Message);
    }

    /// <summary>A custom message appears when EndsWith fails.</summary>
    [TestMethod]
    public void EndsWith_WithCustomMessage_IncludesMessage() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(static () => "abc".Should().EndsWith("x", "custom end msg"));
        Assert.AreEqual("custom end msg", ex.Message);
    }

    /// <summary>BeNull passes when the actual string is null.</summary>
    [TestMethod]
    public void BeNull_OnNullString_Passes() {
        var nullStr = bool.Parse("true") ? null : string.Empty;
        nullStr.Should().BeNull();
        Assert.IsNull(nullStr);
    }

    /// <summary>EndsWith on a null actual string throws AssertionFailedException.</summary>
    [TestMethod]
    public void EndsWith_NullActual_Throws() {
        const string? nullStr = null;
        var ex = Assert.ThrowsExactly<AssertionFailedException>(static () => nullStr.Should().EndsWith("x"));
        Assert.IsFalse(string.IsNullOrWhiteSpace(ex.Message));
    }

    /// <summary>EndsWith against an actual string shorter than expected throws AssertionFailedException.</summary>
    [TestMethod]
    public void EndsWith_ShorterActual_Throws() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(static () => "ab".Should().EndsWith("abc"));
        Assert.IsFalse(string.IsNullOrWhiteSpace(ex.Message));
    }

    /// <summary>A custom message appears when BeNull fails.</summary>
    [TestMethod]
    public void BeNull_WithCustomMessage_IncludesMessage() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(static () => "x".Should().BeNull("custom null msg"));
        Assert.AreEqual("custom null msg", ex.Message);
    }

    /// <summary>A custom message appears when NotBeNull fails.</summary>
    [TestMethod]
    public void NotBeNull_WithCustomMessage_IncludesMessage() {
        const string? nullStr = null;
        var ex = Assert.ThrowsExactly<AssertionFailedException>(static () => nullStr.Should().NotBeNull("custom notnull msg"));
        Assert.AreEqual("custom notnull msg", ex.Message);
    }
}
