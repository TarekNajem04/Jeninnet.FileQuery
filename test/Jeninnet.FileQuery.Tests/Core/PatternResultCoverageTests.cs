namespace Jeninnet.FileQuery.Tests.Core;

[TestClass]
public class PatternResultCoverageTests
{
    [TestMethod]
    public void PatternResult_Success_Properties()
    {
        var result = PatternResult<string>.Success("ok");
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("ok", result.Value);
        Assert.IsNull(result.Error);
    }

    [TestMethod]
    public void PatternResult_Failure_Properties()
    {
        const string error = "something went wrong";
        var result = PatternResult<string>.Fail(error);
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(error, result.Error);

        try
        {
            _ = result.Value;
            Assert.Fail("Should have thrown InvalidOperationException");
        }
        catch(InvalidOperationException) { /* Ignore */ }
    }

    [TestMethod]
    public void PatternException_Message_Preserved()
    {
        const string msg = "error msg";
        var ex = new PatternException(msg);
        Assert.AreEqual(msg, ex.Message);

        var inner = new InvalidOperationException("inner");
        var ex2 = new PatternException(msg, inner);
        Assert.AreEqual(msg, ex2.Message);
        Assert.AreEqual(inner, ex2.InnerException);
    }

    [TestMethod]
    public void PatternSyntaxException_Message_Preserved()
    {
        const string msg = "syntax error";
        var ex = new PatternSyntaxException("*.txt", msg);
        Assert.AreEqual(msg, ex.Message);
        Assert.AreEqual("*.txt", ex.Pattern);
    }
}
