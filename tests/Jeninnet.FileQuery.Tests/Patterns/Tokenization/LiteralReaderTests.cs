//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
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
        (_reader.TryRead(pattern, ref i, out _)).Should().BeFalse();
    }

    /// <summary>Tests TryRead_StartsWildcard_ReturnsFalse.</summary>
    [TestMethod]
    public void TryRead_StartsWildcard_ReturnsFalse() {
        var pattern = "*abc".AsSpan();
        var i = 0;
        (_reader.TryRead(pattern, ref i, out _)).Should().BeFalse();
    }

    /// <summary>Tests TryRead_Literal_ReturnsTrueAndToken.</summary>
    [TestMethod]
    public void TryRead_Literal_ReturnsTrueAndToken() {
        var pattern = "abc".AsSpan();
        var i = 0;
        (_reader.TryRead(pattern, ref i, out var token)).Should().BeTrue();
        i.Should().Be(3);
        token.Should().BeAssignableTo<LiteralToken>();
        ((LiteralToken)token).Text.Should().Be("abc");
    }

    /// <summary>Tests TryRead_LiteralWithWildcard_StopsAtWildcard.</summary>
    [TestMethod]
    public void TryRead_LiteralWithWildcard_StopsAtWildcard() {
        var pattern = "abc*def".AsSpan();
        var i = 0;
        (_reader.TryRead(pattern, ref i, out var token)).Should().BeTrue();
        i.Should().Be(3);
        token.Should().BeAssignableTo<LiteralToken>();
        ((LiteralToken)token).Text.Should().Be("abc");
    }
}
