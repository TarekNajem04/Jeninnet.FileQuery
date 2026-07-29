namespace Jeninnet.FileQuery.Tests.Unit.Patterns;

/// <summary>
/// Tests for <see cref="PatternResult{T}"/>, <see cref="PatternException"/>, and <see cref="PatternSyntaxException"/>.
/// </summary>
[TestClass]
public class PatternResultCoverageTests {
    /// <summary>
    /// Verifies that a successful result has the correct properties.
    /// </summary>
    [TestMethod]
    public void Should_HaveCorrectProperties_When_PatternResultSuccess() {
        var result = PatternResult<string>.Success("ok");
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("ok", result.Value);
        Assert.IsNull(result.Error);
    }

    /// <summary>
    /// Verifies that a failed result has the correct properties and throws when accessing the value.
    /// </summary>
    [TestMethod]
    public void Should_HaveCorrectProperties_When_PatternResultFailure() {
        const string error = "something went wrong";
        var result = PatternResult<string>.Fail(error);
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(error, result.Error);

        try {
            _ = result.Value;
            Assert.Fail("Should have thrown InvalidOperationException");
        }
        catch(InvalidOperationException) { /* Ignore */ }
    }

    /// <summary>
    /// Verifies that the message and inner exception are preserved when throwing a PatternException.
    /// </summary>
    [TestMethod]
    public void Should_PreserveMessage_When_PatternExceptionThrown() {
        const string msg = "error msg";
        var ex = new PatternException(msg);
        Assert.AreEqual(msg, ex.Message);

        var inner = new InvalidOperationException("inner");
        var ex2 = new PatternException(msg, inner);
        Assert.AreEqual(msg, ex2.Message);
        Assert.AreEqual(inner, ex2.InnerException);
    }

    /// <summary>
    /// Verifies that the message and pattern are preserved when throwing a PatternSyntaxException.
    /// </summary>
    [TestMethod]
    public void Should_PreserveMessage_When_PatternSyntaxExceptionThrown() {
        const string msg = "syntax error";
        var ex = new PatternSyntaxException("*.txt", msg);
        Assert.AreEqual(msg, ex.Message);
        Assert.AreEqual("*.txt", ex.Pattern);
    }
}

