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
    /// A GitIgnore rule with an interior slash is anchored to the root, not recursive.
    /// </summary>
    [TestMethod]
    public void IsMatch_GitIgnoreInteriorSlash_IsRootAnchored() {
        var patternSet = FileQueryPatternSet.Compile(
            new PatternInput(
                TypedPatterns: new Dictionary<PatternKind, IEnumerable<string>> {
                    [PatternKind.GitIgnore] = ["src/*.cs"]
                },
                InterpretationMode: PatternInterpretationMode.Specific),
            PatternMatchingMode.GitIgnore);

        Assert.IsFalse(patternSet.IsMatch("src/a.cs"));
        Assert.IsTrue(patternSet.IsMatch("other/src/a.cs"));
    }

    /// <summary>
    /// Typed regex patterns do not require an explicit r: prefix.
    /// </summary>
    [TestMethod]
    public void IsMatch_TypedRegexWithoutPrefix_Matches() {
        var patternSet = FileQueryPatternSet.Compile(
            new PatternInput(
                TypedPatterns: new Dictionary<PatternKind, IEnumerable<string>> {
                    [PatternKind.Regex] = ["^foo$"]
                },
                InterpretationMode: PatternInterpretationMode.Specific),
            PatternMatchingMode.Regex);

        Assert.IsTrue(patternSet.IsMatch("foo"));
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
        Assert.IsFalse(patternSet.IsMatch("bin/x.cs"));
        Assert.IsFalse(patternSet.IsMatch("other/bin/x.cs"));
        Assert.IsTrue(patternSet.IsMatch("other/bin"));
    }

    /// <summary>
    /// A compiled set with no patterns matches every non-empty path but excludes the empty path.
    /// </summary>
    [TestMethod]
    public void IsMatch_EmptyPatternSet_ExcludesEmptyPath() {
        var patternSet = FileQueryPatternSet.Compile(
            new PatternInput(),
            PatternMatchingMode.GitIgnore);

        Assert.IsTrue(patternSet.IsMatch("src/Program.cs"));
        Assert.IsFalse(patternSet.IsMatch(""));
        Assert.AreEqual(FileQueryMatchOutcome.Exclude, patternSet.Explain("").Outcome);
    }

    /// <summary>
    /// A later explicit exclusion wins over an earlier directory-only negation,
    /// matching GitIgnore last-match-wins semantics.
    /// </summary>
    [TestMethod]
    public void IsMatch_DirectoryNegation_DoesNotOverrideLaterExclusion() {
        var patternSet = FileQueryPatternSet.Compile(
            new PatternInput(
                Patterns: ["**", "!keep/", "keep/file.txt"]),
            PatternMatchingMode.GitIgnore);

        Assert.IsFalse(patternSet.IsMatch("keep/file.txt"));
    }

    /// <summary>
    /// A directory-only negation still re-includes descendants when no later rule
    /// overrides it.
    /// </summary>
    [TestMethod]
    public void IsMatch_DirectoryNegation_ReincludesDescendants() {
        var patternSet = FileQueryPatternSet.Compile(
            new PatternInput(
                Patterns: ["**", "!keep/"]),
            PatternMatchingMode.GitIgnore);

        Assert.IsTrue(patternSet.IsMatch("keep/file.txt"));
        Assert.IsTrue(patternSet.IsMatch("other/keep/inside.txt"));
    }

    /// <summary>
    /// Explain attributes a Glob re-inclusion to the Glob pattern, not to the
    /// GitIgnore exclusion that it overrides.
    /// </summary>
    [TestMethod]
    public void Explain_AttributesGlobReInclusion() {
        var patternSet = FileQueryPatternSet.Compile(
            new PatternInput(
                TypedPatterns: new Dictionary<PatternKind, IEnumerable<string>> {
                    [PatternKind.GitIgnore] = ["*"],
                    [PatternKind.Glob] = ["**/*.cs"]
                }));

        var explanation = patternSet.Explain("src/Program.cs");

        Assert.AreEqual(FileQueryMatchOutcome.Include, explanation.Outcome);
        Assert.AreEqual("**/*.cs", explanation.Pattern);
        Assert.AreEqual(PatternKind.Glob, explanation.PatternKind);
    }

    /// <summary>
    /// Explain attributes a Regex re-inclusion to the Regex pattern.
    /// </summary>
    [TestMethod]
    public void Explain_AttributesRegexReInclusion() {
        var patternSet = FileQueryPatternSet.Compile(
            new PatternInput(
                TypedPatterns: new Dictionary<PatternKind, IEnumerable<string>> {
                    [PatternKind.GitIgnore] = ["*"],
                    [PatternKind.Regex] = [@"^src/.*\.txt$"]
                }));

        var explanation = patternSet.Explain("src/notes.txt");

        Assert.AreEqual(FileQueryMatchOutcome.Include, explanation.Outcome);
        Assert.AreEqual(@"r:^src/.*\.txt$", explanation.Pattern);
        Assert.AreEqual(PatternKind.Regex, explanation.PatternKind);
    }

    /// <summary>
    /// Explain attributes a GitIgnore negation to the negated rule.
    /// </summary>
    [TestMethod]
    public void Explain_AttributesGitIgnoreNegation() {
        var patternSet = FileQueryPatternSet.Compile(
            new PatternInput(
                TypedPatterns: new Dictionary<PatternKind, IEnumerable<string>> {
                    [PatternKind.GitIgnore] = ["**", "!*.cs"]
                }));

        var explanation = patternSet.Explain("Program.cs");

        Assert.AreEqual(FileQueryMatchOutcome.Include, explanation.Outcome);
        Assert.AreEqual("!*.cs", explanation.Pattern);
        Assert.AreEqual(PatternKind.GitIgnore, explanation.PatternKind);
    }

    /// <summary>
    /// A non-matching Glob subset does not erase a GitIgnore exclusion, so the
    /// explanation still attributes the exclusion to the GitIgnore rule.
    /// </summary>
    [TestMethod]
    public void Explain_GitIgnoreExclude_IsNotErasedByNonMatchingGlob() {
        var patternSet = FileQueryPatternSet.Compile(
            new PatternInput(
                TypedPatterns: new Dictionary<PatternKind, IEnumerable<string>> {
                    [PatternKind.GitIgnore] = ["**"],
                    [PatternKind.Glob] = ["**/*.cs"]
                }));

        var explanation = patternSet.Explain("notes.txt");

        Assert.AreEqual(FileQueryMatchOutcome.Exclude, explanation.Outcome);
        Assert.AreEqual("**", explanation.Pattern);
        Assert.AreEqual(PatternKind.GitIgnore, explanation.PatternKind);
    }

    /// <summary>
    /// Specific mode rejects untyped patterns instead of silently classifying them.
    /// </summary>
    [TestMethod]
    public void Compile_SpecificMode_RejectsUntypedPatterns() {
        var input = new PatternInput(
            Patterns: ["*.cs"],
            InterpretationMode: PatternInterpretationMode.Specific);

        _ = Assert.ThrowsExactly<PatternException>(() =>
            FileQueryPatternSet.Compile(input, PatternMatchingMode.GitIgnore));
    }

    /// <summary>
    /// Specific mode rejects typed patterns from a dialect other than the selected
    /// matching mode, instead of compiling patterns that can never match.
    /// </summary>
    [TestMethod]
    public void Compile_SpecificMode_RejectsMismatchedTypedDialect() {
        var input = new PatternInput(
            TypedPatterns: new Dictionary<PatternKind, IEnumerable<string>> {
                [PatternKind.Glob] = ["**/*.cs"]
            },
            InterpretationMode: PatternInterpretationMode.Specific);

        _ = Assert.ThrowsExactly<PatternException>(() =>
            FileQueryPatternSet.Compile(input, PatternMatchingMode.GitIgnore));
    }

    /// <summary>
    /// Empty and whitespace-only pattern strings are rejected at the public boundary.
    /// </summary>
    [TestMethod]
    public void PatternInput_RejectsEmptyAndWhitespacePatterns() {
        _ = Assert.ThrowsExactly<ArgumentException>(() => new PatternInput(Patterns: [""]));
        _ = Assert.ThrowsExactly<ArgumentException>(() => new PatternInput(Patterns: ["   "]));
        _ = Assert.ThrowsExactly<ArgumentException>(() =>
            new PatternInput(
                TypedPatterns: new Dictionary<PatternKind, IEnumerable<string>> {
                    [PatternKind.GitIgnore] = [" "]
                }));
    }

    /// <summary>
    /// Undefined interpretation modes are rejected at the public boundary.
    /// </summary>
    [TestMethod]
    public void PatternInput_RejectsUndefinedInterpretationMode() {
        _ = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            new PatternInput(InterpretationMode: (PatternInterpretationMode)99));
    }

    /// <summary>
    /// Undefined pattern kinds are rejected at the public boundary.
    /// </summary>
    [TestMethod]
    public void PatternInput_RejectsUndefinedPatternKind() {
        _ = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            new PatternInput(
                TypedPatterns: new Dictionary<PatternKind, IEnumerable<string>> {
                    [(PatternKind)99] = ["*.cs"]
                }));
    }

    /// <summary>
    /// Undefined case-sensitivity and matching-mode values are rejected.
    /// </summary>
    [TestMethod]
    public void Compile_RejectsUndefinedCaseSensitivityAndMatchingMode() {
        _ = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            FileQueryPatternSet.Compile(
                new PatternInput(),
                PatternMatchingMode.GitIgnore,
                (CaseSensitivity)99));

        _ = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            FileQueryPatternSet.Compile(
                new PatternInput(),
                (PatternMatchingMode)99));
    }

    /// <summary>
    /// Windows-style separators in Glob patterns fail with an actionable error
    /// instead of compiling to a pattern that can never match.
    /// </summary>
    [TestMethod]
    public void Compile_GlobWithWindowsPathSeparator_Throws() {
        var input = new PatternInput(
            TypedPatterns: new Dictionary<PatternKind, IEnumerable<string>> {
                [PatternKind.Glob] = [@"C:\projects\file.cs"]
            },
            InterpretationMode: PatternInterpretationMode.Specific);

        var exception = Assert.ThrowsExactly<PatternException>(() =>
            FileQueryPatternSet.Compile(input, PatternMatchingMode.Glob));

        StringAssert.Contains(exception.Message, "use '/' as the path separator");
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
