//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Unit.Patterns.Tokens;

/// <summary>
/// Tests for RecursiveWildcardReaderTests.
/// </summary>
[TestClass]
public sealed class RecursiveWildcardReaderTests {
    /// <summary>
    /// Verifies that Should ReturnTrue When DoubleStar.
    /// </summary>
    [TestMethod]
    public void Should_ReturnTrue_When_DoubleStar() {
        var reader = new RecursiveWildcardReader();
        var pattern = "**".AsSpan();
        var i = 0;

        var result = reader.TryRead(pattern, ref i, out var token);

        result.Should().BeTrue();
        token.Should().BeAssignableTo<RecursiveWildcardToken>();
        i.Should().Be(2);
    }

    /// <summary>
    /// Verifies that Should ReturnFalse When SingleStar.
    /// </summary>
    [TestMethod]
    public void Should_ReturnFalse_When_SingleStar() {
        var reader = new RecursiveWildcardReader();
        var pattern = "*".AsSpan();
        var i = 0;

        var result = reader.TryRead(pattern, ref i, out _);

        result.Should().BeFalse();
        i.Should().Be(0);
    }
}
