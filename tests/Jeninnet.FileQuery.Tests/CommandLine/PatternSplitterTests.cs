//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
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

        result.Should().HaveCount(3);
        result[0].Should().Be("a.txt");
        result[1].Should().Be("b.txt");
        result[2].Should().Be("c.txt");
    }

    /// <summary>
    /// Verifies that <see cref="PatternSplitter.Split(string, char[])"/> correctly handles multiple specified separators.
    /// </summary>
    [TestMethod]
    public void Split_ShouldHandleMultipleSeparators() {
        const string input = "a.txt,b.txt;c.txt";
        var separators = new[] { ',', ';' };
        var result = PatternSplitter.Split(input, separators).ToList();

        result.Should().HaveCount(3);
        result[0].Should().Be("a.txt");
        result[1].Should().Be("b.txt");
        result[2].Should().Be("c.txt");
    }

    /// <summary>
    /// Verifies that <see cref="PatternSplitter.Split(string, char[])"/> returns a single item when no separator is present in the input.
    /// </summary>
    [TestMethod]
    public void Split_ShouldReturnSingleItem_WhenNoSeparatorPresent() {
        const string input = "a.txt";
        var result = PatternSplitter.Split(input).ToList();

        result.Should().HaveCount(1);
        result[0].Should().Be("a.txt");
    }

    /// <summary>
    /// Verifies that <see cref="PatternSplitter.Split(string, char[])"/> correctly handles empty or whitespace-only input by returning an empty collection.
    /// </summary>
    [TestMethod]
    public void Split_ShouldHandleEmptyAndWhitespaceInput() {
        const string input = " ; ; ";
        var result = PatternSplitter.Split(input).ToList();
        result.Should().BeEmpty();
    }
}
