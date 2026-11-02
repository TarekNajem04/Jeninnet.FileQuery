//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
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
        (_reader.TryRead(pattern, ref i, out _)).Should().BeFalse();
        i.Should().Be(0);
    }

    /// <summary>
    /// Verifies that Should ReturnFalse When BackslashAtEnd.
    /// </summary>
    [TestMethod]
    public void Should_ReturnFalse_When_BackslashAtEnd() {
        var pattern = "\\".AsSpan();
        var i = 0;
        (_reader.TryRead(pattern, ref i, out _)).Should().BeFalse();
        i.Should().Be(0);
    }

    /// <summary>
    /// Verifies that Should ReturnFalse When InvalidEscape.
    /// </summary>
    [TestMethod]
    public void Should_ReturnFalse_When_InvalidEscape() {
        var pattern = "\\a".AsSpan();
        var i = 0;
        (_reader.TryRead(pattern, ref i, out _)).Should().BeFalse();
        i.Should().Be(0);
    }

    /// <summary>
    /// Verifies that Should ReturnTrueAndToken When ValidEscape.
    /// </summary>
    [TestMethod]
    public void Should_ReturnTrueAndToken_When_ValidEscape() {
        var pattern = "\\*".AsSpan();
        var i = 0;
        (_reader.TryRead(pattern, ref i, out var token)).Should().BeTrue();
        i.Should().Be(2);
        token.Should().BeAssignableTo<EscapeToken>();
        ((EscapeToken)token).Escaped.Should().Be('*');
    }

    /// <summary>
    /// Verifies that Should ReturnTrueAndToken When ValidEscapeBackslash.
    /// </summary>
    [TestMethod]
    public void Should_ReturnTrueAndToken_When_ValidEscapeBackslash() {
        var pattern = "\\\\".AsSpan();
        var i = 0;
        (_reader.TryRead(pattern, ref i, out var token)).Should().BeTrue();
        i.Should().Be(2);
        token.Should().BeAssignableTo<EscapeToken>();
        ((EscapeToken)token).Escaped.Should().Be('\\');
    }
}
