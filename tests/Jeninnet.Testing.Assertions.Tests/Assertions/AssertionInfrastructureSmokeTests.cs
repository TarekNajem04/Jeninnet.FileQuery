namespace Jeninnet.Testing.Assertions.Tests.Assertions;

[TestClass]
public sealed class AssertionInfrastructureSmokeTests {
    [TestMethod]
    public void Should_IsDiscoverable() {
        true.Should().BeTrue();
    }

    [TestMethod]
    public void PassingAssertion_DoesNotThrow() {
        "hello".Should().NotBeNull();
    }

    [TestMethod]
    public void FailingAssertion_ThrowsAssertionFailedException() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(() => false.Should().BeTrue());
        Assert.IsNotNull(ex);
        Assert.IsFalse(string.IsNullOrWhiteSpace(ex.Message));
    }

    [TestMethod]
    public void BeOfType_WithCorrectType() {
        ((object)"test").Should().Be<string>();
    }

    [TestMethod]
    public void NotBeNull_OnNull_Throws() {
        string? nullStr = null;
        Assert.ThrowsExactly<AssertionFailedException>(() => nullStr.Should().NotBeNull());
    }

    [TestMethod]
    public void BeNull_OnNonNull_Throws() {
        Assert.ThrowsExactly<AssertionFailedException>(() => "not null".Should().BeNull());
    }
}
