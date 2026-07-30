namespace Jeninnet.Testing.Assertions.Tests.Exceptions;

/// <summary>Smoke tests for exception assertions: Throw and AssertionFailedException construction.</summary>
[TestClass]
public sealed class ExceptionAssertionSmokeTests {
    /// <summary>Throw passes when the action throws the expected type.</summary>
    [TestMethod]
    public void Throw_WhenActionThrows_Passes() {
        Action throws = static () => throw new InvalidOperationException("fail");
        var thrown = throws.Should().Throw<InvalidOperationException>();
        Assert.IsNotNull(thrown);
        Assert.IsInstanceOfType<InvalidOperationException>(thrown.Exception);
    }

    /// <summary>Throw throws AssertionFailedException when the action does not throw.</summary>
    [TestMethod]
    public void Throw_WhenActionDoesNotThrow_Throws() {
        Action doesNotThrow = () => { };
        Assert.ThrowsExactly<AssertionFailedException>(() => doesNotThrow.Should().Throw<Exception>());
    }

    /// <summary>AssertionFailedException stores and exposes the provided message.</summary>
    [TestMethod]
    public void AssertionFailedException_ContainsMessage() {
        var ex = new AssertionFailedException("custom message");
        Assert.AreEqual("custom message", ex.Message);
    }

    /// <summary>AssertionFailedException derives from Exception.</summary>
    [TestMethod]
    public void AssertionFailedException_IsException() {
        var ex = new AssertionFailedException();
        Assert.IsInstanceOfType<Exception>(ex);
    }

    /// <summary>AssertionFailedException stores and exposes an inner exception.</summary>
    [TestMethod]
    public void AssertionFailedException_WithInnerException_StoresInner() {
        var inner = new InvalidOperationException("inner");
        var ex = new AssertionFailedException("outer", inner);
        Assert.AreEqual("outer", ex.Message);
        Assert.AreSame(inner, ex.InnerException);
    }

    /// <summary>AssertionFailedException with null inner exception stores null.</summary>
    [TestMethod]
    public void AssertionFailedException_WithNullInner_StoresNull() {
        var ex = new AssertionFailedException("msg", null);
        Assert.AreEqual("msg", ex.Message);
        Assert.IsNull(ex.InnerException);
    }
}
