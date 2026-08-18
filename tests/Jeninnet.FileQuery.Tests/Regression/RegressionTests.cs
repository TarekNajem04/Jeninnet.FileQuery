//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Regression;

/// <summary>
/// Regression tests covering bugs fixed in v1.0.
/// Each test is named after the bug it guards against.
/// </summary>
[TestClass]
public sealed class RegressionTests {
    /// <summary>
    /// When two separate <c>Where(Dictionary)</c> calls are chained and the second
    /// call contains a key already present from the first call, all patterns from
    /// the second call must still be merged — not just the first key.
    /// </summary>
    [TestMethod]
    public void Where_Dictionary_MustMergeAllKeys_WhenSomeAlreadyExist() {
        // Arrange: first call establishes GitIgnore and Glob buckets.
        var builder = FileQuery.From(Directory.GetCurrentDirectory())
                               .Where(new Dictionary<PatternKind, List<string>> {
                                   [PatternKind.GitIgnore] = ["**"],
                                   [PatternKind.Glob] = ["*.cs"]
                               });

        // Act: second call provides an additional GitIgnore pattern (key already exists)
        // AND a new Regex pattern (key is new).
        builder.Where(new Dictionary<PatternKind, List<string>> {
            [PatternKind.GitIgnore] = ["!*.log"],   // merge into existing bucket
            [PatternKind.Regex] = ["r:^data.*"] // add new bucket
        });

        var query = builder.Build();

        // Assert: all three buckets must be present with the correct patterns.
        var typedPatterns = query.Options.PatternInput.TypedPatterns;

        // GitIgnore bucket must contain both patterns.
        Assert.IsTrue(
            typedPatterns.ContainsKey(PatternKind.GitIgnore),
            "GitIgnore bucket must exist."
        );

        Assert.AreSequenceEqual(
            ["**", "!*.log"],
            [.. typedPatterns[PatternKind.GitIgnore]], SequenceOrder.InAnyOrder,
            "GitIgnore bucket must contain patterns from both Where() calls."
        );

        // Glob bucket from the first call must still be present.
        Assert.IsTrue(
            typedPatterns.ContainsKey(PatternKind.Glob),
            "Glob bucket must not be lost when a later Where() call merges a different key."
        );

        Assert.AreSequenceEqual(
            ["*.cs"],
            [.. typedPatterns[PatternKind.Glob]],
            SequenceOrder.InAnyOrder,
            "Glob bucket must be unchanged."
        );

        // Regex bucket added by the second call must be present.
        Assert.IsTrue(
            typedPatterns.ContainsKey(PatternKind.Regex),
            "Regex bucket added in the second Where() call must not be silently dropped."
        );

        Assert.AreSequenceEqual(
            ["r:^data.*"],
            [.. typedPatterns[PatternKind.Regex]],
            SequenceOrder.InAnyOrder,
            "Regex bucket must contain the pattern from the second Where() call."
        );
    }

    /// <summary>
    /// A single <c>Where(Dictionary)</c> call with multiple keys must process
    /// all of them even when none exists yet.
    /// </summary>
    [TestMethod]
    public void Where_Dictionary_MustAddAllKeys_InSingleCall() {
        // Arrange
        var builder = FileQuery.From(Directory.GetCurrentDirectory());

        // Act: single call with three distinct keys.
        builder.Where(new Dictionary<PatternKind, List<string>> {
            [PatternKind.GitIgnore] = ["**", "!*.cs"],
            [PatternKind.Glob] = ["src/**/*.ts"],
            [PatternKind.Regex] = ["r:^temp_.*\\.txt$"]
        });

        var query = builder.Build();
        var typedPatterns = query.Options.PatternInput.TypedPatterns;

        // Assert: all three keys must be present.
        Assert.HasCount(3, typedPatterns,
            "All three pattern-kind buckets must be created from a single Where() call.");
        Assert.AreSequenceEqual(["**", "!*.cs"], [.. typedPatterns[PatternKind.GitIgnore]], SequenceOrder.InAnyOrder);

        Assert.AreSequenceEqual(["src/**/*.ts"], [.. typedPatterns[PatternKind.Glob]], SequenceOrder.InAnyOrder);

        Assert.AreSequenceEqual(["r:^temp_.*\\.txt$"], [.. typedPatterns[PatternKind.Regex]], SequenceOrder.InAnyOrder);
    }

    /// <summary>
    /// Patterns that are already present in a bucket must not be duplicated
    /// when the same pattern is provided again through a second <c>Where</c> call.
    /// </summary>
    [TestMethod]
    public void Where_Dictionary_MustNotDuplicate_ExistingPatterns() {
        // Arrange
        var builder = FileQuery.From(Directory.GetCurrentDirectory())
                               .Where(new Dictionary<PatternKind, List<string>> {
                                   [PatternKind.GitIgnore] = ["**", "!*.log"]
                               });

        // Act: same patterns provided again.
        builder.Where(new Dictionary<PatternKind, List<string>> {
            [PatternKind.GitIgnore] = ["**", "!*.log", "!*.tmp"] // two duplicates + one new
        });

        var query = builder.Build();
        var gitIgnorePatterns = query.Options.PatternInput.TypedPatterns[PatternKind.GitIgnore].ToArray();

        // Assert: only three distinct patterns (no duplicates).
        Assert.HasCount(3, gitIgnorePatterns,
            "Duplicate patterns must not be added again. Expected: **, !*.log, !*.tmp.");
    }

    private static RegexInstructionMatcher CreateRegexMatcher() => new();

    private static ICompiledPatternSet CompileRegex(string pattern) => CompiledPatternFactory.Compile(PatternKind.Regex, pattern);

    private static PathMatchContext FileContext(
        string path,
        CaseSensitivity cs = CaseSensitivity.Sensitive) => new(path, PathKind.File, cs);

    /// <summary>
    /// The same regex pattern compiled with <see cref="CaseSensitivity.Sensitive"/>
    /// and then called with <see cref="CaseSensitivity.Insensitive"/> must not
    /// return the cached case-sensitive regex for the case-insensitive call.
    /// </summary>
    [TestMethod]
    public void RegexMatcher_CaseSensitiveAndInsensitive_MustNotShareCachedRegex() {
        const string pattern = "r:^README\\.md$";
        var matcher = CreateRegexMatcher();
        var patterns = CompileRegex(pattern);

        // First call: sensitive — "readme.md" must NOT match "^README\.md$" (case-sensitive).
        var sensitiveResult = matcher.Match(patterns, FileContext("readme.md", CaseSensitivity.Sensitive));
        Assert.AreEqual(
            MatchOutcome.NoMatch,
            sensitiveResult,
            "Case-sensitive match of 'readme.md' against '^README\\.md$' must not match."
        );

        // Second call: insensitive — "readme.md" MUST match "^README\.md$" (case-insensitive).
        var insensitiveResult = matcher.Match(patterns, FileContext("readme.md", CaseSensitivity.Insensitive));
        Assert.AreEqual(
            MatchOutcome.Include,
            insensitiveResult,
            "Case-insensitive match of 'readme.md' against '^README\\.md$' must match."
        );
    }

    /// <summary>
    /// Two independently compiled pattern sets with identical regex text
    /// must both match correctly — the second must not bypass evaluation
    /// due to a stale cache entry from the first.
    /// </summary>
    [TestMethod]
    public void RegexMatcher_TwoIndependentCompiledSets_MustBothMatch() {
        const string pattern = "r:^src/.*\\.cs$";
        var matcher = CreateRegexMatcher();

        // Compile the same pattern twice — produces two distinct ICompiledPatternSet instances.
        var patternSetA = CompileRegex(pattern);
        var patternSetB = CompileRegex(pattern);

        var contextMatch = FileContext("src/Program.cs");
        var contextNoMatch = FileContext("test/Program.cs");

        // Both sets must match the same path.
        Assert.AreEqual(MatchOutcome.Include, matcher.Match(patternSetA, contextMatch),
            "First compiled set must match 'src/Program.cs'.");
        Assert.AreEqual(MatchOutcome.Include, matcher.Match(patternSetB, contextMatch),
            "Second compiled set (independently compiled) must also match 'src/Program.cs'.");

        // Both sets must reject the same non-matching path.
        Assert.AreEqual(MatchOutcome.NoMatch, matcher.Match(patternSetA, contextNoMatch),
            "First compiled set must not match 'test/Program.cs'.");
        Assert.AreEqual(MatchOutcome.NoMatch, matcher.Match(patternSetB, contextNoMatch),
            "Second compiled set must not match 'test/Program.cs'.");
    }

    /// <summary>
    /// Verifies that switching from insensitive to sensitive on successive calls
    /// uses the correct <see cref="Regex"/> from the cache for each call.
    /// </summary>
    [TestMethod]
    public void RegexMatcher_AlternatingCaseSensitivity_UsesCorrectRegexEachTime() {
        const string pattern = "r:^DATA_.*\\.log$";
        var matcher = CreateRegexMatcher();
        var patterns = CompileRegex(pattern);

        // Insensitive: lowercase subject must match.
        Assert.AreEqual(
            MatchOutcome.Include,
            matcher.Match(patterns, FileContext("data_archive.log", CaseSensitivity.Insensitive)),
            "Insensitive: 'data_archive.log' must match '^DATA_.*\\.log$'."
        );

        // Sensitive: lowercase subject must NOT match uppercase pattern.
        Assert.AreEqual(
            MatchOutcome.NoMatch,
            matcher.Match(patterns, FileContext("data_archive.log", CaseSensitivity.Sensitive)),
            "Sensitive: 'data_archive.log' must NOT match '^DATA_.*\\.log$'.");

        // Insensitive again: still must match (correct cache entry reused).
        Assert.AreEqual(
            MatchOutcome.Include,
            matcher.Match(patterns, FileContext("data_archive.log", CaseSensitivity.Insensitive)),
            "Insensitive (second call): must still match using the correct cached Regex."
        );
    }
}
