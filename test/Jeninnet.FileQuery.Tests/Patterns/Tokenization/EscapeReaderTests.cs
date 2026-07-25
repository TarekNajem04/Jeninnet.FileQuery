using Jeninnet.FileQuery.Patterns.Tokenization;
using Jeninnet.FileQuery.Patterns.Syntax;

namespace Jeninnet.FileQuery.Tests.Patterns.Tokenization;

[TestClass]
public sealed class EscapeReaderTests {
    private readonly EscapeReader _reader = new();

    [TestMethod]
    public void TryRead_NotEscape_ReturnsFalse() {
        var pattern = "abc".AsSpan();
        int i = 0;
        Assert.IsFalse(_reader.TryRead(pattern, ref i, out _));
        Assert.AreEqual(0, i);
    }

    [TestMethod]
    public void TryRead_BackslashAtEnd_ReturnsFalse() {
        var pattern = "\\".AsSpan();
        int i = 0;
        Assert.IsFalse(_reader.TryRead(pattern, ref i, out _));
        Assert.AreEqual(0, i);
    }

    [TestMethod]
    public void TryRead_InvalidEscape_ReturnsFalse() {
        var pattern = "\\a".AsSpan();
        int i = 0;
        Assert.IsFalse(_reader.TryRead(pattern, ref i, out _));
        Assert.AreEqual(0, i);
    }

    [TestMethod]
    public void TryRead_ValidEscape_ReturnsTrueAndToken() {
        var pattern = "\\*".AsSpan();
        int i = 0;
        Assert.IsTrue(_reader.TryRead(pattern, ref i, out var token));
        Assert.AreEqual(2, i);
        Assert.IsInstanceOfType<EscapeToken>(token);
        Assert.AreEqual('*', ((EscapeToken)token).Escaped);
    }

    [TestMethod]
    public void TryRead_ValidEscapeBackslash_ReturnsTrueAndToken() {
        var pattern = "\\\\".AsSpan();
        int i = 0;
        Assert.IsTrue(_reader.TryRead(pattern, ref i, out var token));
        Assert.AreEqual(2, i);
        Assert.IsInstanceOfType<EscapeToken>(token);
        Assert.AreEqual('\\', ((EscapeToken)token).Escaped);
    }
}
