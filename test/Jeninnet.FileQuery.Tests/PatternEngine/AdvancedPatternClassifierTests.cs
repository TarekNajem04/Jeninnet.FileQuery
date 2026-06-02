namespace Jeninnet.FileQuery.Tests.PatternEngine;

/// <summary>
/// Unit tests for AdvancedPatternClassifier using MSTest.
/// Covers all syntactic categories: GitIgnore, Glob, Flat, and Unknown.
/// </summary>
[TestClass]
public class AdvancedPatternClassifierComprehensiveTests {
    private static void AssertClassification(string pattern, PatternKind expected) => Assert.AreEqual(expected, PatternClassifier.Classify(pattern));

    [TestMethod]
    public void GitIgnore_ShouldDetectNegationPatterns() {
        AssertClassification("!*.txt", PatternKind.GitIgnore);
        AssertClassification("!src/*.cs", PatternKind.GitIgnore);
    }

    [TestMethod]
    public void GitIgnore_ShouldDetectRootAnchoredPatterns() {
        AssertClassification("/bin/", PatternKind.GitIgnore);
        AssertClassification("/assets/styles.css", PatternKind.GitIgnore);
    }

    [TestMethod]
    public void GitIgnore_ShouldDetectDirectoryOnlyPatterns() {
        AssertClassification("logs/", PatternKind.GitIgnore);
        AssertClassification("build/", PatternKind.GitIgnore);
    }

    [TestMethod]
    public void GitIgnore_ShouldDetectCommentLines() =>
        AssertClassification("# comment", PatternKind.GitIgnore);

    [TestMethod]
    public void GitIgnore_ShouldDetectEscapedCharacters() {
        AssertClassification(@"\!important.txt", PatternKind.GitIgnore);
        AssertClassification(@"\#literal.md", PatternKind.GitIgnore);
        AssertClassification(@"\[abc].txt", PatternKind.GitIgnore);
        AssertClassification(@"\*.md", PatternKind.GitIgnore);
    }

    [TestMethod]
    public void GitIgnore_ShouldDetectValidBracketRanges() {
        AssertClassification("[abc].txt", PatternKind.GitIgnore);
        AssertClassification("[a-z].md", PatternKind.GitIgnore);
        AssertClassification("[!abc]data", PatternKind.GitIgnore);
    }

    [TestMethod]
    public void Glob_ShouldDetectWindowsStylePaths() {
        AssertClassification(@"src\main\*.cs", PatternKind.Glob);
        AssertClassification(@"assets\images\?.png", PatternKind.Glob);
    }

    [TestMethod]
    public void Glob_ShouldDetectStrayClosingBracket() =>
        AssertClassification("file].txt", PatternKind.Glob);

    [TestMethod]
    public void Flat_ShouldDetectFlatPrefixedPatterns() {
        AssertClassification("r:images/*.png", PatternKind.Regex);
        AssertClassification("r:raw-data", PatternKind.Regex);
    }

    [TestMethod]
    public void Unknown_ShouldDetectUnclosedBracketRanges() {
        AssertClassification("[abc", PatternKind.Unknown);
        AssertClassification("[a-z", PatternKind.Unknown);
    }

    [TestMethod]
    public void Unknown_ShouldDetectEmptyBracketExpressions() =>
        AssertClassification("[]", PatternKind.Unknown);

    [TestMethod]
    public void Unknown_ShouldDetectInvalidRangePatterns() {
        AssertClassification("[a-]", PatternKind.Unknown);
        AssertClassification("[--x]", PatternKind.Unknown);
        AssertClassification("[-a]", PatternKind.Unknown);
    }

    [TestMethod]
    public void Unknown_ShouldDetectInvalidEscapeEdgeCases() =>
        AssertClassification(@"\", PatternKind.Unknown);

    [TestMethod]
    public void GitIgnore_ShouldDetectLiteralPatterns() {
        AssertClassification("file", PatternKind.GitIgnore);
        AssertClassification("README.md", PatternKind.GitIgnore);
    }

    [TestMethod]
    public void GitIgnore_ShouldHandleNegationWithWindowsPath() =>
        AssertClassification(@"!\folder\file.txt", PatternKind.GitIgnore);

    [TestMethod]
    public void Glob_ShouldHandleMixedSeparators() =>
        AssertClassification(@"src\main/*.cs", PatternKind.Glob);

    [TestMethod]
    public void ShouldHandleUnicodeCharacters() {
        AssertClassification("файл?.txt", PatternKind.GitIgnore);
        AssertClassification("src/документы/*.md", PatternKind.GitIgnore);
    }

    [TestMethod]
    public void ShouldHandleMultipleBrackets() {
        AssertClassification("[a-c][x-z].txt", PatternKind.GitIgnore);
        AssertClassification("[a-[z]].txt", PatternKind.Unknown);
    }

    [TestMethod]
    public void ShouldHandleWildcardsAlone() {
        AssertClassification("*", PatternKind.GitIgnore);
        AssertClassification("**", PatternKind.GitIgnore);
        AssertClassification("?", PatternKind.GitIgnore);
    }

    [TestMethod]
    public void ShouldHandleWhitespaceOnlyPatterns() {
        AssertClassification("   ", PatternKind.GitIgnore);
        AssertClassification("\tfile.txt", PatternKind.GitIgnore);
    }

    [TestMethod]
    public void ShouldHandleInlineComments() =>
        AssertClassification("*.log # ignore logs", PatternKind.GitIgnore);
}
