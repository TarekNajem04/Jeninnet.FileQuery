//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Unit.Patterns.Tokens;

/// <summary>
/// Tests for LiteralReaderTests.
/// </summary>
[TestClass]
public sealed class LiteralReaderTests {
    private readonly LiteralReader _reader = new();

    /// <summary>
    /// Verifies that Should ReturnFalse When Empty.
    /// </summary>
    [TestMethod]
    public void Should_ReturnFalse_When_Empty() {
        var pattern = "".AsSpan();
        var i = 0;
        Assert.IsFalse(_reader.TryRead(pattern, ref i, out _));
    }

    /// <summary>
    /// Verifies that Should ReturnFalse When StartsWildcard.
    /// </summary>
    [TestMethod]
    public void Should_ReturnFalse_When_StartsWildcard() {
        var pattern = "*abc".AsSpan();
        var i = 0;
        Assert.IsFalse(_reader.TryRead(pattern, ref i, out _));
    }

    /// <summary>
    /// Verifies that Should ReturnTrueAndToken When Literal.
    /// </summary>
    [TestMethod]
    public void Should_ReturnTrueAndToken_When_Literal() {
        var pattern = "abc".AsSpan();
        var i = 0;
        Assert.IsTrue(_reader.TryRead(pattern, ref i, out var token));
        Assert.AreEqual(3, i);
        Assert.IsInstanceOfType<LiteralToken>(token);
        Assert.AreEqual("abc", ((LiteralToken)token).Text);
    }

    /// <summary>
    /// Verifies that Should StopAtWildcard When LiteralWithWildcard.
    /// </summary>
    [TestMethod]
    public void Should_StopAtWildcard_When_LiteralWithWildcard() {
        var pattern = "abc*def".AsSpan();
        var i = 0;
        Assert.IsTrue(_reader.TryRead(pattern, ref i, out var token));
        Assert.AreEqual(3, i);
        Assert.IsInstanceOfType<LiteralToken>(token);
        Assert.AreEqual("abc", ((LiteralToken)token).Text);
    }
}
