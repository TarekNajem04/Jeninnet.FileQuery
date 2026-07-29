namespace Jeninnet.Testing.Assertions.Tests.Strings;

[TestClass]
public sealed class StringAssertionSmokeTests {
    private const string Hello = "hello";
    private const string HelloWorld = "hello world";

    [TestMethod]
    public void Be_WithMatchingString_Passes() {
        Hello.Should().Be(Hello);
    }

    [TestMethod]
    public void Be_WithNonMatchingString_Throws() {
        Assert.ThrowsExactly<AssertionFailedException>(() => Hello.Should().Be("world"));
    }

    [TestMethod]
    public void Contain_Substring_Passes() {
        HelloWorld.Should().Contain("world");
    }

    [TestMethod]
    public void Contain_MissingSubstring_Throws() {
        Assert.ThrowsExactly<AssertionFailedException>(() => Hello.Should().Contain("xyz"));
    }

    [TestMethod]
    public void EndsWith_Passes() {
        HelloWorld.Should().EndsWith("world");
    }

    [TestMethod]
    public void NotBeNull_OnNonNull_Passes() {
        Hello.Should().NotBeNull();
    }

    [TestMethod]
    public void NotBeNull_OnNull_Throws() {
        string? nullStr = null;
        Assert.ThrowsExactly<AssertionFailedException>(() => nullStr.Should().NotBeNull());
    }

    [TestMethod]
    public void BeNull_OnNull_Passes() {
        string? nullStr = null;
        nullStr.Should().BeNull();
    }
}
