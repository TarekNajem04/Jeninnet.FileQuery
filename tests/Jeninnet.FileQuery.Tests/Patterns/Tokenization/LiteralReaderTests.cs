namespace Jeninnet.FileQuery.Tests.Patterns.Tokenization;

/// <summary>
/// Contains unit tests for the <see cref="LiteralReader"/> class.
/// </summary>
[TestClass]
public sealed class LiteralReaderTests {
    private readonly LiteralReader _reader = new();

    /// <summary>Tests TryRead_Empty_ReturnsFalse.</summary>
    [TestMethod]
    public void TryRead_Empty_ReturnsFalse() {
        var pattern = "".AsSpan();
        var i = 0;
        Assert.IsFalse(_reader.TryRead(pattern, ref i, out _));
    }

    /// <summary>Tests TryRead_StartsWildcard_ReturnsFalse.</summary>
    [TestMethod]
    public void TryRead_StartsWildcard_ReturnsFalse() {
        var pattern = "*abc".AsSpan();
        var i = 0;
        Assert.IsFalse(_reader.TryRead(pattern, ref i, out _));
    }

    /// <summary>Tests TryRead_Literal_ReturnsTrueAndToken.</summary>
    [TestMethod]
    public void TryRead_Literal_ReturnsTrueAndToken() {
        var pattern = "abc".AsSpan();
        var i = 0;
        Assert.IsTrue(_reader.TryRead(pattern, ref i, out var token));
        Assert.AreEqual(3, i);
        Assert.IsInstanceOfType<LiteralToken>(token);
        Assert.AreEqual("abc", ((LiteralToken)token).Text);
    }

    /// <summary>Tests TryRead_LiteralWithWildcard_StopsAtWildcard.</summary>
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
