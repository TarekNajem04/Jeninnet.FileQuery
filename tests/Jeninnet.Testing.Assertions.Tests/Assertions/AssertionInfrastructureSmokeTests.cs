//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.Testing.Assertions.Tests.Assertions;

/// <summary>Verifies the core assertion infrastructure: discoverability, passing/failing assertions, and type checks.</summary>
[TestClass]
public sealed class AssertionInfrastructureSmokeTests {
    /// <summary>The Should() extension is discoverable and BeTrue() accepts a true value.</summary>
    [TestMethod]
    public void Should_IsDiscoverable() {
        var value = bool.Parse("true");
        value.Should().BeTrue();
        Assert.IsTrue(value);
    }

    /// <summary>A passing fluent assertion completes without throwing.</summary>
    [TestMethod]
    public void PassingAssertion_DoesNotThrow() {
        const string value = "hello";
        value.Should().NotBeNull();
        Assert.IsNotNull(value);
    }

    /// <summary>A failing assertion throws AssertionFailedException with a non-empty message.</summary>
    [TestMethod]
    public void FailingAssertion_ThrowsAssertionFailedException() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(static () => false.Should().BeTrue());
        Assert.IsNotNull(ex);
        Assert.IsFalse(string.IsNullOrWhiteSpace(ex.Message));
    }

    /// <summary>BeOfType correctly identifies the runtime type of a value.</summary>
    [TestMethod]
    public void BeOfType_WithCorrectType() {
        object value = "test";
        value.Should().Be<string>();
        Assert.IsInstanceOfType<string>(value);
    }

    /// <summary>NotBeNull throws when the value is null.</summary>
    [TestMethod]
    public void NotBeNull_OnNull_Throws() => Assert.ThrowsExactly<AssertionFailedException>(static () => ((string?)null).Should().NotBeNull());

    /// <summary>BeNull throws when the value is non-null.</summary>
    [TestMethod]
    public void BeNull_OnNonNull_Throws() => Assert.ThrowsExactly<AssertionFailedException>(static () => "not null".Should().BeNull());
}
