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

        Assert.IsTrue(result);
        Assert.IsInstanceOfType<RecursiveWildcardToken>(token);
        Assert.AreEqual(2, i);
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

        Assert.IsFalse(result);
        Assert.AreEqual(0, i);
    }
}
