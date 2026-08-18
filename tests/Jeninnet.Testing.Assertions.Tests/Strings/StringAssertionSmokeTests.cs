//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.Testing.Assertions.Tests.Strings;

/// <summary>Smoke tests for string assertions: equality, containment, suffix, and null checks.</summary>
[TestClass]
public sealed class StringAssertionSmokeTests {
    private const string HELLO = "hello";
    private const string HELLO_WORLD = "hello world";

    /// <summary>Be passes when strings match exactly.</summary>
    [TestMethod]
    public void Be_WithMatchingString_Passes() {
        HELLO.Should().Be(HELLO);
        Assert.IsFalse(string.IsNullOrEmpty(HELLO));
    }

    /// <summary>Be throws when strings differ.</summary>
    [TestMethod]
    public void Be_WithNonMatchingString_Throws() => Assert.ThrowsExactly<AssertionFailedException>(static () => HELLO.Should().Be("world"));

    /// <summary>Contain passes when the substring is found.</summary>
    [TestMethod]
    public void Contain_Substring_Passes() {
        HELLO_WORLD.Should().Contain("world");
        Assert.Contains("world", HELLO_WORLD);
    }

    /// <summary>Contain throws when the substring is absent.</summary>
    [TestMethod]
    public void Contain_MissingSubstring_Throws() => Assert.ThrowsExactly<AssertionFailedException>(static () => HELLO.Should().Contain("xyz"));

    /// <summary>EndsWith passes when the string has the expected suffix.</summary>
    [TestMethod]
    public void EndsWith_Passes() {
        HELLO_WORLD.Should().EndsWith("world");
        Assert.IsTrue(HELLO_WORLD.EndsWith("world", StringComparison.Ordinal));
    }

    /// <summary>NotBeNull passes on a non-null string.</summary>
    [TestMethod]
    public void NotBeNull_OnNonNull_Passes() {
        HELLO.Should().NotBeNull();
        Assert.IsNotNull(HELLO);
    }

    /// <summary>NotBeNull throws on a null string.</summary>
    [TestMethod]
    public void NotBeNull_OnNull_Throws() {
        const string? nullStr = null;
        Assert.ThrowsExactly<AssertionFailedException>(static () => nullStr.Should().NotBeNull());
    }

    /// <summary>BeNull passes on a null string.</summary>
    [TestMethod]
    public void BeNull_OnNull_Passes() {
        var nullStr = bool.Parse("true") ? null : string.Empty;
        nullStr.Should().BeNull();
        Assert.IsNull(nullStr);
    }
}
