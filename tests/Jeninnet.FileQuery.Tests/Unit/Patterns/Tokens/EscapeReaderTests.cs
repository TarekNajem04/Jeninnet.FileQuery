namespace Jeninnet.FileQuery.Tests.Unit.Patterns.Tokens;

/// <summary>
/// Tests for EscapeReaderTests.
/// </summary>
[TestClass]
public sealed class EscapeReaderTests {
    private readonly EscapeReader _reader = new();

    /// <summary>
    /// Verifies that Should ReturnFalse When NotEscape.
    /// </summary>
    [TestMethod]
    public void Should_ReturnFalse_When_NotEscape() {
        var pattern = "abc".AsSpan();
        var i = 0;
        Assert.IsFalse(_reader.TryRead(pattern, ref i, out _));
        Assert.AreEqual(0, i);
    }

    /// <summary>
    /// Verifies that Should ReturnFalse When BackslashAtEnd.
    /// </summary>
    [TestMethod]
    public void Should_ReturnFalse_When_BackslashAtEnd() {
        var pattern = "\\".AsSpan();
        var i = 0;
        Assert.IsFalse(_reader.TryRead(pattern, ref i, out _));
        Assert.AreEqual(0, i);
    }

    /// <summary>
    /// Verifies that Should ReturnFalse When InvalidEscape.
    /// </summary>
    [TestMethod]
    public void Should_ReturnFalse_When_InvalidEscape() {
        var pattern = "\\a".AsSpan();
        var i = 0;
        Assert.IsFalse(_reader.TryRead(pattern, ref i, out _));
        Assert.AreEqual(0, i);
    }

    /// <summary>
    /// Verifies that Should ReturnTrueAndToken When ValidEscape.
    /// </summary>
    [TestMethod]
    public void Should_ReturnTrueAndToken_When_ValidEscape() {
        var pattern = "\\*".AsSpan();
        var i = 0;
        Assert.IsTrue(_reader.TryRead(pattern, ref i, out var token));
        Assert.AreEqual(2, i);
        Assert.IsInstanceOfType<EscapeToken>(token);
        Assert.AreEqual('*', ((EscapeToken)token).Escaped);
    }

    /// <summary>
    /// Verifies that Should ReturnTrueAndToken When ValidEscapeBackslash.
    /// </summary>
    [TestMethod]
    public void Should_ReturnTrueAndToken_When_ValidEscapeBackslash() {
        var pattern = "\\\\".AsSpan();
        var i = 0;
        Assert.IsTrue(_reader.TryRead(pattern, ref i, out var token));
        Assert.AreEqual(2, i);
        Assert.IsInstanceOfType<EscapeToken>(token);
        Assert.AreEqual('\\', ((EscapeToken)token).Escaped);
    }
}
