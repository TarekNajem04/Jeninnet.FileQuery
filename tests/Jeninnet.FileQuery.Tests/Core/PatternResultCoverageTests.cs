namespace Jeninnet.FileQuery.Tests.Core;

/// <summary>
/// Contains unit tests for the <see cref="PatternResult{T}"/> class and related exception types.
/// </summary>
[TestClass]
public class PatternResultCoverageTests {
    /// <summary>
    /// Verifies that the <see cref="PatternResult{T}.Success(T)"/> factory method correctly sets the success state and value.
    /// </summary>
    [TestMethod]
    public void PatternResult_Success_Properties() {
        var result = PatternResult<string>.Success("ok");
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("ok", result.Value);
        Assert.IsNull(result.Error);
    }

    /// <summary>
    /// Verifies that the <see cref="PatternResult{T}.Fail(string)"/> factory method correctly sets the failure state and error message.
    /// </summary>
    [TestMethod]
    public void PatternResult_Failure_Properties() {
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
    /// Verifies that <see cref="PatternException"/> correctly preserves the error message and inner exception.
    /// </summary>
    [TestMethod]
    public void PatternException_Message_Preserved() {
        const string msg = "error msg";
        var ex = new PatternException(msg);
        Assert.AreEqual(msg, ex.Message);

        var inner = new InvalidOperationException("inner");
        var ex2 = new PatternException(msg, inner);
        Assert.AreEqual(msg, ex2.Message);
        Assert.AreEqual(inner, ex2.InnerException);
    }

    /// <summary>
    /// Verifies that <see cref="PatternSyntaxException"/> correctly preserves the error message and pattern.
    /// </summary>
    [TestMethod]
    public void PatternSyntaxException_Message_Preserved() {
        const string msg = "syntax error";
        var ex = new PatternSyntaxException("*.txt", msg);
        Assert.AreEqual(msg, ex.Message);
        Assert.AreEqual("*.txt", ex.Pattern);
    }
}

