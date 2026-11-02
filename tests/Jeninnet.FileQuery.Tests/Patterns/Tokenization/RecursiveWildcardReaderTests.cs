//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Patterns.Tokenization;

/// <summary>
/// Contains unit tests for the <see cref="RecursiveWildcardReader"/>, verifying correct tokenization of recursive wildcard patterns.
/// </summary>
[TestClass]
public sealed class RecursiveWildcardReaderTests {
    /// <summary>
    /// Verifies that <see cref="RecursiveWildcardReader.TryRead"/> returns <c>true</c> and produces a <see cref="RecursiveWildcardToken"/> when encountering a double-star (**) pattern.
    /// </summary>
    [TestMethod]
    public void TryRead_ReturnsTrueForDoubleStar() {
        var reader = new RecursiveWildcardReader();
        var pattern = "**".AsSpan();
        var i = 0;

        var result = reader.TryRead(pattern, ref i, out var token);

        result.Should().BeTrue();
        token.Should().BeAssignableTo<RecursiveWildcardToken>();
        i.Should().Be(2);
    }

    /// <summary>
    /// Verifies that <see cref="RecursiveWildcardReader.TryRead"/> returns <c>false</c> when encountering a single-star (*) pattern, as it is not a recursive wildcard.
    /// </summary>
    [TestMethod]
    public void TryRead_ReturnsFalseForSingleStar() {
        var reader = new RecursiveWildcardReader();
        var pattern = "*".AsSpan();
        var i = 0;

        var result = reader.TryRead(pattern, ref i, out _);

        result.Should().BeFalse();
        i.Should().Be(0);
    }
}
