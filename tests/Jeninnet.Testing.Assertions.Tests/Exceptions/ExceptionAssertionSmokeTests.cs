namespace Jeninnet.Testing.Assertions.Tests.Exceptions;

[TestClass]
public sealed class ExceptionAssertionSmokeTests {
    [TestMethod]
    public void Throw_WhenActionThrows_Passes() {
        Action throws = () => throw new InvalidOperationException("fail");
        throws.Should().Throw<InvalidOperationException>();
    }

    [TestMethod]
    public void Throw_WhenActionDoesNotThrow_Throws() {
        Action doesNotThrow = () => { };
        Assert.ThrowsExactly<AssertionFailedException>(() => doesNotThrow.Should().Throw<Exception>());
    }

    [TestMethod]
    public void AssertionFailedException_ContainsMessage() {
        var ex = new AssertionFailedException("custom message");
        Assert.AreEqual("custom message", ex.Message);
    }

    [TestMethod]
    public void AssertionFailedException_IsException() {
        var ex = new AssertionFailedException();
        Assert.IsInstanceOfType<Exception>(ex);
    }
}
