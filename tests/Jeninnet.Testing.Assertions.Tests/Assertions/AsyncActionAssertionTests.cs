namespace Jeninnet.Testing.Assertions.Tests.Assertions;

/// <summary>Verifies that <see cref="AsyncActionAssertions"/> correctly handles all ThrowAsync paths.</summary>
[TestClass]
public sealed class AsyncActionAssertionTests {
    /// <summary>When the async action throws the exact expected exception type, ThrowAsync returns the exception.</summary>
    [TestMethod]
    public async Task ThrowAsync_WhenActionThrowsExpectedType_ReturnsExceptionAssertionsAsync() {
        Func<Task> act = static () => throw new InvalidOperationException("fail");
        var result = await act.Should().ThrowAsync<InvalidOperationException>();
        Assert.IsNotNull(result);
        Assert.AreEqual("fail", result.Exception.Message);
    }

    /// <summary>When the async action throws a derived type, ThrowAsync matches the base type.</summary>
    [TestMethod]
    public async Task ThrowAsync_WhenActionThrowsDerivedType_ReturnsExceptionAssertionsAsync() {
        Func<Task> act = static () => throw new InvalidOperationException("derived");
        var result = await act.Should().ThrowAsync<Exception>();
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<InvalidOperationException>(result.Exception);
    }

    /// <summary>When the async action throws a different exception type, ThrowAsync throws AssertionFailedException.</summary>
    [TestMethod]
    public async Task ThrowAsync_WhenActionThrowsWrongType_ThrowsAssertionFailedExceptionAsync() {
        Func<Task> act = () => throw new InvalidOperationException("wrong");
        var ex = await Assert.ThrowsExactlyAsync<AssertionFailedException>(() => act.Should().ThrowAsync<ArgumentException>());
        Assert.IsFalse(string.IsNullOrWhiteSpace(ex.Message));
    }

    /// <summary>When the async action does not throw, ThrowAsync throws AssertionFailedException.</summary>
    [TestMethod]
    public async Task ThrowAsync_WhenActionDoesNotThrow_ThrowsAssertionFailedExceptionAsync() {
        Func<Task> act = () => Task.CompletedTask;
        var ex = await Assert.ThrowsExactlyAsync<AssertionFailedException>(() => act.Should().ThrowAsync<Exception>());
        Assert.IsFalse(string.IsNullOrWhiteSpace(ex.Message));
    }

    /// <summary>A custom message appears in the exception when the wrong exception type is thrown.</summary>
    [TestMethod]
    public async Task ThrowAsync_WithCustomMessage_WrongType_IncludesMessageAsync() {
        Func<Task> act = () => throw new InvalidOperationException();
        var ex = await Assert.ThrowsExactlyAsync<AssertionFailedException>(
            () => act.Should().ThrowAsync<ArgumentException>("custom message"));
        Assert.AreEqual("custom message", ex.Message);
    }

    /// <summary>A custom message appears in the exception when no exception is thrown.</summary>
    [TestMethod]
    public async Task ThrowAsync_WithCustomMessage_NoThrow_IncludesMessageAsync() {
        Func<Task> act = () => Task.CompletedTask;
        var ex = await Assert.ThrowsExactlyAsync<AssertionFailedException>(
            () => act.Should().ThrowAsync<Exception>("no throw custom"));
        Assert.AreEqual("no throw custom", ex.Message);
    }
}
