//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.PatternSet;

/// <summary>
/// Tests for the in-memory <see cref="FileQueryPatternSet"/> API.
/// </summary>
[TestClass]
public sealed class FileQueryPatternSetTests {
    /// <summary>
    /// A glob pattern set matches files without traversing a filesystem.
    /// </summary>
    [TestMethod]
    public void IsMatch_MatchesGlobPatterns() {
        var patternSet = CreateGlobSet("**/*.cs");

        Assert.IsTrue(patternSet.IsMatch("src/Program.cs"));
        Assert.IsTrue(patternSet.IsMatch("Program.cs"));
        Assert.IsFalse(patternSet.IsMatch("README.md"));
    }

    /// <summary>
    /// GitIgnore last-match-wins and negation are preserved.
    /// </summary>
    [TestMethod]
    public void IsMatch_PreservesGitIgnoreNegation() {
        var patternSet = FileQueryPatternSet.Compile(
            new PatternInput(
                TypedPatterns: new Dictionary<PatternKind, IEnumerable<string>> {
                    [PatternKind.GitIgnore] = ["**", "!*.log"]
                },
                InterpretationMode: PatternInterpretationMode.Specific),
            PatternMatchingMode.GitIgnore);

        Assert.IsTrue(patternSet.IsMatch("app.log"));
        Assert.IsFalse(patternSet.IsMatch("app.txt"));
    }

    /// <summary>
    /// Character-class literals and ranges honour the requested case sensitivity.
    /// </summary>
    [TestMethod]
    public void IsMatch_CharacterClass_HonoursCaseSensitivity() {
        var insensitive = FileQueryPatternSet.Compile(
            new PatternInput(
                TypedPatterns: new Dictionary<PatternKind, IEnumerable<string>> {
                    [PatternKind.Glob] = ["[a-z].txt"]
                },
                InterpretationMode: PatternInterpretationMode.Specific),
            PatternMatchingMode.Glob,
            CaseSensitivity.Insensitive);
        var sensitive = FileQueryPatternSet.Compile(
            new PatternInput(
                TypedPatterns: new Dictionary<PatternKind, IEnumerable<string>> {
                    [PatternKind.Glob] = ["[a-z].txt"]
                },
                InterpretationMode: PatternInterpretationMode.Specific),
            PatternMatchingMode.Glob,
            CaseSensitivity.Sensitive);

        Assert.IsTrue(insensitive.IsMatch("A.txt"));
        Assert.IsTrue(insensitive.IsMatch("z.txt"));
        Assert.IsFalse(insensitive.IsMatch("1.txt"));
        Assert.IsFalse(sensitive.IsMatch("A.txt"));
        Assert.IsTrue(sensitive.IsMatch("a.txt"));
    }

    /// <summary>
    /// The span overload is allocation-free on the matching path.
    /// </summary>
    [TestMethod]
    public void IsMatch_SpanOverload_DoesNotAllocate() {
        var patternSet = CreateGlobSet("**/*.cs");
        var path = "src/Program.cs".AsSpan();

        for(var i = 0; i < 100; i++) {
            _ = patternSet.IsMatch(path);
        }

#pragma warning disable S1215 // "GC.Collect" should not be called
        GC.Collect();
#pragma warning restore S1215 // "GC.Collect" should not be called

        var before = GC.GetAllocatedBytesForCurrentThread();
        var isMatch = patternSet.IsMatch(path);
        var after = GC.GetAllocatedBytesForCurrentThread();

        Assert.IsTrue(isMatch);
        Assert.AreEqual(before, after);
    }

    /// <summary>
    /// Explain identifies the deciding pattern and outcome.
    /// </summary>
    [TestMethod]
    public void Explain_ReturnsDecidingPattern() {
        var patternSet = CreateGlobSet("**/*.cs");

        var explanation = patternSet.Explain("src/Program.cs");

        Assert.AreEqual(FileQueryMatchOutcome.Include, explanation.Outcome);
        Assert.IsTrue(explanation.IsMatch);
        Assert.AreEqual("**/*.cs", explanation.Pattern);
        Assert.AreEqual(PatternKind.Glob, explanation.PatternKind);
        Assert.AreEqual("Include by Glob pattern.", explanation.Reason);
    }

    /// <summary>
    /// Explain reports NoMatch when no pattern applies.
    /// </summary>
    [TestMethod]
    public void Explain_ReturnsNoMatch_ForUnmatchedPath() {
        var patternSet = CreateGlobSet("**/*.cs");

        var explanation = patternSet.Explain("README.md");

        Assert.AreEqual(FileQueryMatchOutcome.NoMatch, explanation.Outcome);
        Assert.IsFalse(explanation.IsMatch);
        Assert.IsNull(explanation.Pattern);
    }

    /// <summary>
    /// Windows separators are normalized by the string overload.
    /// </summary>
    [TestMethod]
    public void IsMatch_NormalizesBackslashes() {
        var patternSet = CreateGlobSet("**/*.cs");

        Assert.IsTrue(patternSet.IsMatch(@"src\Program.cs"));
    }

    /// <summary>
    /// A trailing slash marks a directory, so a directory-only exclusion applies to it but not
    /// to a same-named file.
    /// </summary>
    [TestMethod]
    public void IsMatch_TrailingSlash_MarksDirectory() {
        var patternSet = FileQueryPatternSet.Compile(
            new PatternInput(
                TypedPatterns: new Dictionary<PatternKind, IEnumerable<string>> {
                    [PatternKind.GitIgnore] = ["bin/"]
                },
                InterpretationMode: PatternInterpretationMode.Specific),
            PatternMatchingMode.GitIgnore);

        Assert.IsFalse(patternSet.IsMatch("bin/"));
        Assert.IsTrue(patternSet.IsMatch("bin"));
    }

    /// <summary>
    /// Creates a compiled Glob pattern set.
    /// </summary>
    /// <param name="pattern">The Glob pattern.</param>
    private static FileQueryPatternSet CreateGlobSet(string pattern) =>
        FileQueryPatternSet.Compile(
            new PatternInput(
                TypedPatterns: new Dictionary<PatternKind, IEnumerable<string>> {
                    [PatternKind.Glob] = [pattern]
                },
                InterpretationMode: PatternInterpretationMode.Specific),
            PatternMatchingMode.Glob);
}
