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

        Assert.IsTrue(result);
        Assert.IsInstanceOfType<RecursiveWildcardToken>(token);
        Assert.AreEqual(2, i);
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

        Assert.IsFalse(result);
        Assert.AreEqual(0, i);
    }
}

