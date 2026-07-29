namespace Jeninnet.FileQuery.Tests.Patterns.Tokenization;

/// <summary>
/// Contains tests for the <see cref="EscapeReader"/> class.
/// </summary>
[TestClass]
public sealed class EscapeReaderTests {
    private readonly EscapeReader _reader = new();

    /// <summary>Tests TryRead_NotEscape_ReturnsFalse.</summary>
    [TestMethod]
    public void TryRead_NotEscape_ReturnsFalse() {
        var pattern = "abc".AsSpan();
        var i = 0;
        Assert.IsFalse(_reader.TryRead(pattern, ref i, out _));
        Assert.AreEqual(0, i);
    }

    /// <summary>Tests TryRead_BackslashAtEnd_ReturnsFalse.</summary>
    [TestMethod]
    public void TryRead_BackslashAtEnd_ReturnsFalse() {
        var pattern = "\\".AsSpan();
        var i = 0;
        Assert.IsFalse(_reader.TryRead(pattern, ref i, out _));
        Assert.AreEqual(0, i);
    }

    /// <summary>Tests TryRead_InvalidEscape_ReturnsFalse.</summary>
    [TestMethod]
    public void TryRead_InvalidEscape_ReturnsFalse() {
        var pattern = "\\a".AsSpan();
        var i = 0;
        Assert.IsFalse(_reader.TryRead(pattern, ref i, out _));
        Assert.AreEqual(0, i);
    }

    /// <summary>Tests TryRead_ValidEscape_ReturnsTrueAndToken.</summary>
    [TestMethod]
    public void TryRead_ValidEscape_ReturnsTrueAndToken() {
        var pattern = "\\*".AsSpan();
        var i = 0;
        Assert.IsTrue(_reader.TryRead(pattern, ref i, out var token));
        Assert.AreEqual(2, i);
        Assert.IsInstanceOfType<EscapeToken>(token);
        Assert.AreEqual('*', ((EscapeToken)token).Escaped);
    }

    /// <summary>Tests TryRead_ValidEscapeBackslash_ReturnsTrueAndToken.</summary>
    [TestMethod]
    public void TryRead_ValidEscapeBackslash_ReturnsTrueAndToken() {
        var pattern = "\\\\".AsSpan();
        var i = 0;
        Assert.IsTrue(_reader.TryRead(pattern, ref i, out var token));
        Assert.AreEqual(2, i);
        Assert.IsInstanceOfType<EscapeToken>(token);
        Assert.AreEqual('\\', ((EscapeToken)token).Escaped);
    }
}

