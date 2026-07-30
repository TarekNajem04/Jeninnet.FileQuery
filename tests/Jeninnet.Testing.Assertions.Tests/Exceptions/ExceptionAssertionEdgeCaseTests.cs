namespace Jeninnet.Testing.Assertions.Tests.Exceptions;

/// <summary>Verifies error paths and edge cases in <see cref="ExceptionAssertions{T}"/>.</summary>
[TestClass]
public sealed class ExceptionAssertionEdgeCaseTests {
    /// <summary>The constructor throws AssertionFailedException when given a null exception.</summary>
    [TestMethod]
    public void Constructor_WithNullException_Throws() {
        var ex = Assert.ThrowsExactly<AssertionFailedException>(static () => new ExceptionAssertions<InvalidOperationException>(null!));
        Assert.IsFalse(string.IsNullOrWhiteSpace(ex.Message));
    }

    /// <summary>The Exception property returns the instance passed to the constructor.</summary>
    [TestMethod]
    public void ExceptionProperty_ReturnsOriginalException() {
        var inner = new InvalidOperationException("test");
        var wrapper = new ExceptionAssertions<InvalidOperationException>(inner);
        Assert.AreSame(inner, wrapper.Exception);
    }
}
