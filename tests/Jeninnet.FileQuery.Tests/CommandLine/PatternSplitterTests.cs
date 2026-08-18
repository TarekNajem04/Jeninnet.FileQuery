namespace Jeninnet.FileQuery.Tests.CommandLine;

/// <summary>
/// Contains unit tests for the <see cref="PatternSplitter"/> class, focusing on splitting input strings.
/// </summary>
[TestClass]
public sealed class PatternSplitterTests {
    /// <summary>
    /// Verifies that <see cref="PatternSplitter.Split(string, char[])"/> splits a string by the default semicolon separator.
    /// </summary>
    [TestMethod]
    public void Split_ShouldSplitBySemicolon() {
        const string input = "a.txt;b.txt;c.txt";
        var result = PatternSplitter.Split(input).ToList();

        Assert.HasCount(3, result);
        Assert.AreEqual("a.txt", result[0]);
        Assert.AreEqual("b.txt", result[1]);
        Assert.AreEqual("c.txt", result[2]);
    }

    /// <summary>
    /// Verifies that <see cref="PatternSplitter.Split(string, char[])"/> correctly handles multiple specified separators.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="PatternSplitter.Split(string, char[])"/> returns a single item when no separator is present in the input.
    /// </summary>
    [TestMethod]
    public void Split_ShouldReturnSingleItem_WhenNoSeparatorPresent() {
        const string input = "a.txt";
        var result = PatternSplitter.Split(input).ToList();

        Assert.HasCount(1, result);
        Assert.AreEqual("a.txt", result[0]);
    }

    /// <summary>
    /// Verifies that <see cref="PatternSplitter.Split(string, char[])"/> correctly handles empty or whitespace-only input by returning an empty collection.
    /// </summary>
    [TestMethod]
    public void Split_ShouldHandleEmptyAndWhitespaceInput() {
        const string input = " ; ; ";
        var result = PatternSplitter.Split(input).ToList();
        Assert.IsEmpty(result);
    }
}
