namespace Jeninnet.FileQuery.Tests.Patterns.Tokenization;

[TestClass]
public sealed class RecursiveWildcardReaderTests {
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
