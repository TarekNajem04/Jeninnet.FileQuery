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

        (patternSet.IsMatch("src/Program.cs")).Should().BeTrue();
        patternSet.IsMatch("Program.cs").Should().BeTrue();
        patternSet.IsMatch("README.md").Should().BeFalse();
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

        patternSet.IsMatch("app.log").Should().BeTrue();
        patternSet.IsMatch("app.txt").Should().BeFalse();
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

        (patternSet.IsMatch("src/a.cs")).Should().BeFalse();
        (patternSet.IsMatch("other/src/a.cs")).Should().BeTrue();
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

        patternSet.IsMatch("foo").Should().BeTrue();
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

        insensitive.IsMatch("A.txt").Should().BeTrue();
        insensitive.IsMatch("z.txt").Should().BeTrue();
        insensitive.IsMatch("1.txt").Should().BeFalse();
        sensitive.IsMatch("A.txt").Should().BeFalse();
        sensitive.IsMatch("a.txt").Should().BeTrue();
    }

    /// <summary>
    /// The span overload is allocation-free on the matching path.
    /// </summary>
    [TestMethod]
    public void IsMatch_SpanOverload_DoesNotAllocate() {
        var patternSet = CreateGlobSet("**/*.cs");
        var path = "src/Program.cs".AsSpan();

        for(var i = 0; i < 100; i++) {
            patternSet.IsMatch(path);
        }

#pragma warning disable S1215 // "GC.Collect" should not be called
        GC.Collect();
#pragma warning restore S1215 // "GC.Collect" should not be called

        var before = GC.GetAllocatedBytesForCurrentThread();
        var isMatch = patternSet.IsMatch(path);
        var after = GC.GetAllocatedBytesForCurrentThread();

        isMatch.Should().BeTrue();
        after.Should().Be(before);
    }

    /// <summary>
    /// Explain identifies the deciding pattern and outcome.
    /// </summary>
    [TestMethod]
    public void Explain_ReturnsDecidingPattern() {
        var patternSet = CreateGlobSet("**/*.cs");

        var explanation = patternSet.Explain("src/Program.cs");

        explanation.Outcome.Should().Be(FileQueryMatchOutcome.Include);
        explanation.IsMatch.Should().BeTrue();
        explanation.Pattern.Should().Be("**/*.cs");
        explanation.PatternKind.Should().Be(PatternKind.Glob);
        explanation.Reason.Should().Be("Include by Glob pattern.");
    }

    /// <summary>
    /// Explain reports NoMatch when no pattern applies.
    /// </summary>
    [TestMethod]
    public void Explain_ReturnsNoMatch_ForUnmatchedPath() {
        var patternSet = CreateGlobSet("**/*.cs");

        var explanation = patternSet.Explain("README.md");

        explanation.Outcome.Should().Be(FileQueryMatchOutcome.NoMatch);
        explanation.IsMatch.Should().BeFalse();
        explanation.Pattern.Should().BeNull();
    }

    /// <summary>
    /// Windows separators are normalized by the string overload.
    /// </summary>
    [TestMethod]
    public void IsMatch_NormalizesBackslashes() {
        var patternSet = CreateGlobSet("**/*.cs");

        patternSet.IsMatch(@"src\Program.cs").Should().BeTrue();
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

        (patternSet.IsMatch("bin/")).Should().BeFalse();
        patternSet.IsMatch("bin").Should().BeTrue();
        (patternSet.IsMatch("bin/x.cs")).Should().BeFalse();
        (patternSet.IsMatch("other/bin/x.cs")).Should().BeFalse();
        (patternSet.IsMatch("other/bin")).Should().BeTrue();
    }

    /// <summary>
    /// A compiled set with no patterns matches every non-empty path but excludes the empty path.
    /// </summary>
    [TestMethod]
    public void IsMatch_EmptyPatternSet_ExcludesEmptyPath() {
        var patternSet = FileQueryPatternSet.Compile(
            new PatternInput(),
            PatternMatchingMode.GitIgnore);

        (patternSet.IsMatch("src/Program.cs")).Should().BeTrue();
        patternSet.IsMatch("").Should().BeFalse();
        patternSet.Explain("").Outcome.Should().Be(FileQueryMatchOutcome.Exclude);
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

        (patternSet.IsMatch("keep/file.txt")).Should().BeFalse();
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

        (patternSet.IsMatch("keep/file.txt")).Should().BeTrue();
        (patternSet.IsMatch("other/keep/inside.txt")).Should().BeTrue();
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

        explanation.Outcome.Should().Be(FileQueryMatchOutcome.Include);
        explanation.Pattern.Should().Be("**/*.cs");
        explanation.PatternKind.Should().Be(PatternKind.Glob);
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

        explanation.Outcome.Should().Be(FileQueryMatchOutcome.Include);
        explanation.Pattern.Should().Be(@"r:^src/.*\.txt$");
        explanation.PatternKind.Should().Be(PatternKind.Regex);
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

        explanation.Outcome.Should().Be(FileQueryMatchOutcome.Include);
        explanation.Pattern.Should().Be("!*.cs");
        explanation.PatternKind.Should().Be(PatternKind.GitIgnore);
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

        explanation.Outcome.Should().Be(FileQueryMatchOutcome.Exclude);
        explanation.Pattern.Should().Be("**");
        explanation.PatternKind.Should().Be(PatternKind.GitIgnore);
    }

    /// <summary>
    /// Specific mode rejects untyped patterns instead of silently classifying them.
    /// </summary>
    [TestMethod]
    public void Compile_SpecificMode_RejectsUntypedPatterns() {
        var input = new PatternInput(
            Patterns: ["*.cs"],
            InterpretationMode: PatternInterpretationMode.Specific);

        FluentActions.Invoking(() =>
            FileQueryPatternSet.Compile(input, PatternMatchingMode.GitIgnore)).Should().ThrowExactly<PatternException>();
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

        FluentActions.Invoking(() =>
            FileQueryPatternSet.Compile(input, PatternMatchingMode.GitIgnore)).Should().ThrowExactly<PatternException>();
    }

    /// <summary>
    /// Empty and whitespace-only pattern strings are rejected at the public boundary.
    /// </summary>
    [TestMethod]
    public void PatternInput_RejectsEmptyAndWhitespacePatterns() {
        FluentActions.Invoking(() => { _ = new PatternInput(Patterns: [""]); }).Should().ThrowExactly<ArgumentException>();
        FluentActions.Invoking(() => { _ = new PatternInput(Patterns: ["   "]); }).Should().ThrowExactly<ArgumentException>();
        FluentActions.Invoking(() => {
            _ = new PatternInput(
                TypedPatterns: new Dictionary<PatternKind, IEnumerable<string>> {
                    [PatternKind.GitIgnore] = [" "]
                });
        }).Should().ThrowExactly<ArgumentException>();
    }

    /// <summary>
    /// Undefined interpretation modes are rejected at the public boundary.
    /// </summary>
    [TestMethod]
    public void PatternInput_RejectsUndefinedInterpretationMode() =>
        FluentActions.Invoking(() => { _ = new PatternInput(InterpretationMode: (PatternInterpretationMode)99); }).Should().ThrowExactly<ArgumentOutOfRangeException>();

    /// <summary>
    /// Undefined pattern kinds are rejected at the public boundary.
    /// </summary>
    [TestMethod]
    public void PatternInput_RejectsUndefinedPatternKind() =>
        FluentActions.Invoking(() => {
            _ = new PatternInput(
                TypedPatterns: new Dictionary<PatternKind, IEnumerable<string>> {
                    [(PatternKind)99] = ["*.cs"]
                });
        }).Should().ThrowExactly<ArgumentOutOfRangeException>();

    /// <summary>
    /// Undefined case-sensitivity and matching-mode values are rejected.
    /// </summary>
    [TestMethod]
    public void Compile_RejectsUndefinedCaseSensitivityAndMatchingMode() {
        FluentActions.Invoking(() =>
            FileQueryPatternSet.Compile(
                new PatternInput(),
                PatternMatchingMode.GitIgnore,
                (CaseSensitivity)99)).Should().ThrowExactly<ArgumentOutOfRangeException>();

        FluentActions.Invoking(() =>
            FileQueryPatternSet.Compile(
                new PatternInput(),
                (PatternMatchingMode)99)).Should().ThrowExactly<ArgumentOutOfRangeException>();
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

        var exception = FluentActions.Invoking(() =>
            FileQueryPatternSet.Compile(input, PatternMatchingMode.Glob)).Should().ThrowExactly<PatternException>().Which;

        exception.Message.Should().Contain("use '/' as the path separator");
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
