namespace Jeninnet.FileQuery.Tests.Patterns.Tokenization;

[TestClass]
public sealed class LiteralReaderTests {
    private readonly LiteralReader _reader = new();

    [TestMethod]
    public void TryRead_Empty_ReturnsFalse() {
        var pattern = "".AsSpan();
        var i = 0;
        Assert.IsFalse(_reader.TryRead(pattern, ref i, out _));
    }

    [TestMethod]
    public void TryRead_StartsWildcard_ReturnsFalse() {
        var pattern = "*abc".AsSpan();
        var i = 0;
        Assert.IsFalse(_reader.TryRead(pattern, ref i, out _));
    }

    [TestMethod]
    public void TryRead_Literal_ReturnsTrueAndToken() {
        var pattern = "abc".AsSpan();
        var i = 0;
        Assert.IsTrue(_reader.TryRead(pattern, ref i, out var token));
        Assert.AreEqual(3, i);
        Assert.IsInstanceOfType<LiteralToken>(token);
        Assert.AreEqual("abc", ((LiteralToken)token).Text);
    }

    [TestMethod]
    public void TryRead_LiteralWithWildcard_StopsAtWildcard() {
        var pattern = "abc*def".AsSpan();
        var i = 0;
        Assert.IsTrue(_reader.TryRead(pattern, ref i, out var token));
        Assert.AreEqual(3, i);
        Assert.IsInstanceOfType<LiteralToken>(token);
        Assert.AreEqual("abc", ((LiteralToken)token).Text);
    }
}
