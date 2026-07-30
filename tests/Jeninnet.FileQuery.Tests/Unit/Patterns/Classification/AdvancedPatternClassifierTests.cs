namespace Jeninnet.FileQuery.Tests.Unit.Patterns.Classification;

/// <summary>
/// Unit tests for AdvancedPatternClassifier using MSTest.
/// Covers all syntactic categories: GitIgnore, Glob, Flat, and Unknown.
/// </summary>
[TestClass]
public class AdvancedPatternClassifierTests {
    private static void AssertClassification(string pattern, PatternKind expected) => Assert.AreEqual(expected, PatternClassifier.Classify(pattern));

    /// <summary>
    /// Verifies that Should DetectNegationPatterns When GitIgnoreClassifierUsed.
    /// </summary>
    [TestMethod]
    public void Should_DetectNegationPatterns_When_GitIgnoreClassifierUsed() {
        AssertClassification("!*.txt", PatternKind.GitIgnore);
        AssertClassification("!src/*.cs", PatternKind.GitIgnore);
    }

    /// <summary>
    /// Verifies that Should DetectRootAnchoredPatterns When GitIgnoreClassifierUsed.
    /// </summary>
    [TestMethod]
    public void Should_DetectRootAnchoredPatterns_When_GitIgnoreClassifierUsed() {
        AssertClassification("/bin/", PatternKind.GitIgnore);
        AssertClassification("/assets/styles.css", PatternKind.GitIgnore);
    }

    /// <summary>
    /// Verifies that Should DetectDirectoryOnlyPatterns When GitIgnoreClassifierUsed.
    /// </summary>
    [TestMethod]
    public void Should_DetectDirectoryOnlyPatterns_When_GitIgnoreClassifierUsed() {
        AssertClassification("logs/", PatternKind.GitIgnore);
        AssertClassification("build/", PatternKind.GitIgnore);
    }

    /// <summary>
    /// Verifies that Should DetectCommentLines When GitIgnoreClassifierUsed.
    /// </summary>
    [TestMethod]
    public void Should_DetectCommentLines_When_GitIgnoreClassifierUsed() => AssertClassification("# comment", PatternKind.GitIgnore);

    /// <summary>
    /// Verifies that Should DetectEscapedCharacters When GitIgnoreClassifierUsed.
    /// </summary>
    [TestMethod]
    public void Should_DetectEscapedCharacters_When_GitIgnoreClassifierUsed() {
        AssertClassification(@"\!important.txt", PatternKind.GitIgnore);
        AssertClassification(@"\#literal.md", PatternKind.GitIgnore);
        AssertClassification(@"\[abc].txt", PatternKind.GitIgnore);
        AssertClassification(@"\*.md", PatternKind.GitIgnore);
    }

    /// <summary>
    /// Verifies that Should DetectValidBracketRanges When GitIgnoreClassifierUsed.
    /// </summary>
    [TestMethod]
    public void Should_DetectValidBracketRanges_When_GitIgnoreClassifierUsed() {
        AssertClassification("[abc].txt", PatternKind.GitIgnore);
        AssertClassification("[a-z].md", PatternKind.GitIgnore);
        AssertClassification("[!abc]data", PatternKind.GitIgnore);
    }

    /// <summary>
    /// Verifies that Should DetectWindowsStylePaths When GlobClassifierUsed.
    /// </summary>
    [TestMethod]
    public void Should_DetectWindowsStylePaths_When_GlobClassifierUsed() {
        AssertClassification(@"src\main\*.cs", PatternKind.Glob);
        AssertClassification(@"assets\images\?.png", PatternKind.Glob);
    }

    /// <summary>
    /// Verifies that Should DetectStrayClosingBracket When GlobClassifierUsed.
    /// </summary>
    [TestMethod]
    public void Should_DetectStrayClosingBracket_When_GlobClassifierUsed() => AssertClassification("file].txt", PatternKind.Glob);

    /// <summary>
    /// Verifies that Should DetectFlatPrefixedPatterns When FlatClassifierUsed.
    /// </summary>
    [TestMethod]
    public void Should_DetectFlatPrefixedPatterns_When_FlatClassifierUsed() {
        AssertClassification("r:images/*.png", PatternKind.Regex);
        AssertClassification("r:raw-data", PatternKind.Regex);
    }

    /// <summary>
    /// Verifies that Should DetectUnclosedBracketRanges When UnknownClassifierUsed.
    /// </summary>
    [TestMethod]
    public void Should_DetectUnclosedBracketRanges_When_UnknownClassifierUsed() {
        AssertClassification("[abc", PatternKind.Unknown);
        AssertClassification("[a-z", PatternKind.Unknown);
    }

    /// <summary>
    /// Verifies that Should DetectEmptyBracketExpressions When UnknownClassifierUsed.
    /// </summary>
    [TestMethod]
    public void Should_DetectEmptyBracketExpressions_When_UnknownClassifierUsed() => AssertClassification("[]", PatternKind.Unknown);

    /// <summary>
    /// Verifies that Should DetectInvalidRangePatterns When UnknownClassifierUsed.
    /// </summary>
    [TestMethod]
    public void Should_DetectInvalidRangePatterns_When_UnknownClassifierUsed() {
        AssertClassification("[a-]", PatternKind.Unknown);
        AssertClassification("[--x]", PatternKind.Unknown);
        AssertClassification("[-a]", PatternKind.Unknown);
    }

    /// <summary>
    /// Verifies that Should DetectInvalidEscapeEdgeCases When UnknownClassifierUsed.
    /// </summary>
    [TestMethod]
    public void Should_DetectInvalidEscapeEdgeCases_When_UnknownClassifierUsed() => AssertClassification(@"\", PatternKind.Unknown);

    /// <summary>
    /// Verifies that Should DetectLiteralPatterns When GitIgnoreClassifierUsed.
    /// </summary>
    [TestMethod]
    public void Should_DetectLiteralPatterns_When_GitIgnoreClassifierUsed() {
        AssertClassification("file", PatternKind.GitIgnore);
        AssertClassification("README.md", PatternKind.GitIgnore);
    }

    /// <summary>
    /// Verifies that Should HandleNegationWithWindowsPath When GitIgnoreClassifierUsed.
    /// </summary>
    [TestMethod]
    public void Should_HandleNegationWithWindowsPath_When_GitIgnoreClassifierUsed() => AssertClassification(@"!\folder\file.txt", PatternKind.GitIgnore);

    /// <summary>
    /// Verifies that Should HandleMixedSeparators When GlobClassifierUsed.
    /// </summary>
    [TestMethod]
    public void Should_HandleMixedSeparators_When_GlobClassifierUsed() => AssertClassification(@"src\main/*.cs", PatternKind.Glob);

    /// <summary>
    /// Verifies that Should HandleUnicodeCharacters.
    /// </summary>
    [TestMethod]
    public void Should_HandleUnicodeCharacters() {
        AssertClassification("файл?.txt", PatternKind.GitIgnore);
        AssertClassification("src/документы/*.md", PatternKind.GitIgnore);
    }

    /// <summary>
    /// Verifies that Should HandleMultipleBrackets.
    /// </summary>
    [TestMethod]
    public void Should_HandleMultipleBrackets() {
        AssertClassification("[a-c][x-z].txt", PatternKind.GitIgnore);
        AssertClassification("[a-[z]].txt", PatternKind.Unknown);
    }

    /// <summary>
    /// Verifies that Should HandleWildcardsAlone.
    /// </summary>
    [TestMethod]
    public void Should_HandleWildcardsAlone() {
        AssertClassification("*", PatternKind.GitIgnore);
        AssertClassification("**", PatternKind.GitIgnore);
        AssertClassification("?", PatternKind.GitIgnore);
    }

    /// <summary>
    /// Verifies that Should HandleWhitespaceOnlyPatterns.
    /// </summary>
    [TestMethod]
    public void Should_HandleWhitespaceOnlyPatterns() {
        AssertClassification("   ", PatternKind.GitIgnore);
        AssertClassification("\tfile.txt", PatternKind.GitIgnore);
    }

    /// <summary>
    /// Verifies that Should HandleInlineComments.
    /// </summary>
    [TestMethod]
    public void Should_HandleInlineComments() => AssertClassification("*.log # ignore logs", PatternKind.GitIgnore);
}
