namespace Jeninnet.FileQuery.Tests.CommandLine;

[TestClass]
public sealed class PatternSplitterTests {
    [TestMethod]
    public void Split_ShouldSplitBySemicolon() {
        const string input = "a.txt;b.txt;c.txt";
        var result = PatternSplitter.Split(input).ToList();

        Assert.HasCount(3, result);
        Assert.AreEqual("a.txt", result[0]);
        Assert.AreEqual("b.txt", result[1]);
        Assert.AreEqual("c.txt", result[2]);
    }

    [TestMethod]
    public void Split_ShouldHandleMultipleSeparators() {
        const string input = "a.txt,b.txt;c.txt";
        var separators = new[] { ',', ';' };
        var result = PatternSplitter.Split(input, separators).ToList();

        Assert.HasCount(3, result);
        Assert.AreEqual("a.txt", result[0]);
        Assert.AreEqual("b.txt", result[1]);
        Assert.AreEqual("c.txt", result[2]);
    }

    [TestMethod]
    public void Split_ShouldReturnSingleItem_WhenNoSeparatorPresent() {
        const string input = "a.txt";
        var result = PatternSplitter.Split(input).ToList();

        Assert.HasCount(1, result);
        Assert.AreEqual("a.txt", result[0]);
    }

    [TestMethod]
    public void Split_ShouldHandleEmptyAndWhitespaceInput() {
        const string input = " ; ; ";
        var result = PatternSplitter.Split(input).ToList();
        Assert.IsEmpty(result);
    }
}

