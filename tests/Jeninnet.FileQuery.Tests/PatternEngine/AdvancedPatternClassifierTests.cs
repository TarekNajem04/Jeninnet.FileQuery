namespace Jeninnet.FileQuery.Tests.PatternEngine;

/// <summary>
/// Unit tests for AdvancedPatternClassifier using MSTest.
/// Covers all syntactic categories: GitIgnore, Glob, Flat, and Unknown.
/// </summary>
[TestClass]
public class AdvancedPatternClassifierComprehensiveTests {
    private static void AssertClassification(string pattern, PatternKind expected) => Assert.AreEqual(expected, PatternClassifier.Classify(pattern));

    /// <summary>Tests GitIgnore_ShouldDetectNegationPatterns.</summary>
    [TestMethod]
    public void GitIgnore_ShouldDetectNegationPatterns() {
        AssertClassification("!*.txt", PatternKind.GitIgnore);
        AssertClassification("!src/*.cs", PatternKind.GitIgnore);
    }

    /// <summary>Tests GitIgnore_ShouldDetectRootAnchoredPatterns.</summary>
    [TestMethod]
    public void GitIgnore_ShouldDetectRootAnchoredPatterns() {
        AssertClassification("/bin/", PatternKind.GitIgnore);
        AssertClassification("/assets/styles.css", PatternKind.GitIgnore);
    }

    /// <summary>Tests GitIgnore_ShouldDetectDirectoryOnlyPatterns.</summary>
    [TestMethod]
    public void GitIgnore_ShouldDetectDirectoryOnlyPatterns() {
        AssertClassification("logs/", PatternKind.GitIgnore);
        AssertClassification("build/", PatternKind.GitIgnore);
    }

    /// <summary>Tests GitIgnore_ShouldDetectCommentLines.</summary>
    [TestMethod]
    public void GitIgnore_ShouldDetectCommentLines() => AssertClassification("# comment", PatternKind.GitIgnore);

    /// <summary>Tests GitIgnore_ShouldDetectEscapedCharacters.</summary>
    [TestMethod]
    public void GitIgnore_ShouldDetectEscapedCharacters() {
        AssertClassification(@"\!important.txt", PatternKind.GitIgnore);
        AssertClassification(@"\#literal.md", PatternKind.GitIgnore);
        AssertClassification(@"\[abc].txt", PatternKind.GitIgnore);
        AssertClassification(@"\*.md", PatternKind.GitIgnore);
    }

    /// <summary>Tests GitIgnore_ShouldDetectValidBracketRanges.</summary>
    [TestMethod]
    public void GitIgnore_ShouldDetectValidBracketRanges() {
        AssertClassification("[abc].txt", PatternKind.GitIgnore);
        AssertClassification("[a-z].md", PatternKind.GitIgnore);
        AssertClassification("[!abc]data", PatternKind.GitIgnore);
    }

    /// <summary>Tests Glob_ShouldDetectWindowsStylePaths.</summary>
    [TestMethod]
    public void Glob_ShouldDetectWindowsStylePaths() {
        AssertClassification(@"src\main\*.cs", PatternKind.Glob);
        AssertClassification(@"assets\images\?.png", PatternKind.Glob);
    }

    /// <summary>Tests Glob_ShouldDetectStrayClosingBracket.</summary>
    [TestMethod]
    public void Glob_ShouldDetectStrayClosingBracket() => AssertClassification("file].txt", PatternKind.Glob);

    /// <summary>Tests Flat_ShouldDetectFlatPrefixedPatterns.</summary>
    [TestMethod]
    public void Flat_ShouldDetectFlatPrefixedPatterns() {
        AssertClassification("r:images/*.png", PatternKind.Regex);
        AssertClassification("r:raw-data", PatternKind.Regex);
    }

    /// <summary>Tests Unknown_ShouldDetectUnclosedBracketRanges.</summary>
    [TestMethod]
    public void Unknown_ShouldDetectUnclosedBracketRanges() {
        AssertClassification("[abc", PatternKind.Unknown);
        AssertClassification("[a-z", PatternKind.Unknown);
    }

    /// <summary>Tests Unknown_ShouldDetectEmptyBracketExpressions.</summary>
    [TestMethod]
    public void Unknown_ShouldDetectEmptyBracketExpressions() => AssertClassification("[]", PatternKind.Unknown);

    /// <summary>Tests Unknown_ShouldDetectInvalidRangePatterns.</summary>
    [TestMethod]
    public void Unknown_ShouldDetectInvalidRangePatterns() {
        AssertClassification("[a-]", PatternKind.Unknown);
        AssertClassification("[--x]", PatternKind.Unknown);
        AssertClassification("[-a]", PatternKind.Unknown);
    }

    /// <summary>Tests Unknown_ShouldDetectInvalidEscapeEdgeCases.</summary>
    [TestMethod]
    public void Unknown_ShouldDetectInvalidEscapeEdgeCases() => AssertClassification(@"\", PatternKind.Unknown);

    /// <summary>Tests GitIgnore_ShouldDetectLiteralPatterns.</summary>
    [TestMethod]
    public void GitIgnore_ShouldDetectLiteralPatterns() {
        AssertClassification("file", PatternKind.GitIgnore);
        AssertClassification("README.md", PatternKind.GitIgnore);
    }

    /// <summary>Tests GitIgnore_ShouldHandleNegationWithWindowsPath.</summary>
    [TestMethod]
    public void GitIgnore_ShouldHandleNegationWithWindowsPath() => AssertClassification(@"!\folder\file.txt", PatternKind.GitIgnore);

    /// <summary>Tests Glob_ShouldHandleMixedSeparators.</summary>
    [TestMethod]
    public void Glob_ShouldHandleMixedSeparators() => AssertClassification(@"src\main/*.cs", PatternKind.Glob);

    /// <summary>Tests ShouldHandleUnicodeCharacters.</summary>
    [TestMethod]
    public void ShouldHandleUnicodeCharacters() {
        AssertClassification("файл?.txt", PatternKind.GitIgnore);
        AssertClassification("src/документы/*.md", PatternKind.GitIgnore);
    }

    /// <summary>Tests ShouldHandleMultipleBrackets.</summary>
    [TestMethod]
    public void ShouldHandleMultipleBrackets() {
        AssertClassification("[a-c][x-z].txt", PatternKind.GitIgnore);
        AssertClassification("[a-[z]].txt", PatternKind.Unknown);
    }

    /// <summary>Tests ShouldHandleWildcardsAlone.</summary>
    [TestMethod]
    public void ShouldHandleWildcardsAlone() {
        AssertClassification("*", PatternKind.GitIgnore);
        AssertClassification("**", PatternKind.GitIgnore);
        AssertClassification("?", PatternKind.GitIgnore);
    }

    /// <summary>Tests ShouldHandleWhitespaceOnlyPatterns.</summary>
    [TestMethod]
    public void ShouldHandleWhitespaceOnlyPatterns() {
        AssertClassification("   ", PatternKind.GitIgnore);
        AssertClassification("\tfile.txt", PatternKind.GitIgnore);
    }

    /// <summary>Tests ShouldHandleInlineComments.</summary>
    [TestMethod]
    public void ShouldHandleInlineComments() => AssertClassification("*.log # ignore logs", PatternKind.GitIgnore);
}
