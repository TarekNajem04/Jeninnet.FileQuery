//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Integration;

/// <summary>
/// In-depth coverage tests for internal pattern matching, traversal, and builder integration.
/// </summary>
[TestClass]
public sealed class Phase2CoverageDeepDiveTests {
    /// <summary>
    /// Verifies that POSIX character classes match and exclude the correct characters.
    /// </summary>
    [TestMethod]
    public void Should_MatchCorrectly_When_PosixClassesUsed() {
        static void AssertPosix(string className, char match, char noMatch) {
            var tokens = new List<IPatternToken> { new CharacterClassToken(new CharacterClass(false, [new PosixClass(className)])) };
            (SegmentInstructionMatcher.MatchSegment(match.ToString().AsSpan(), tokens, StringComparison.Ordinal)).Should().BeTrue(because: $"Expected {match} to match {className}");
            (SegmentInstructionMatcher.MatchSegment(noMatch.ToString().AsSpan(), tokens, StringComparison.Ordinal)).Should().BeFalse(because: $"Expected {noMatch} NOT to match {className}");
        }

        AssertPosix("digit", '5', 'a');
        AssertPosix("alpha", 'a', '5');
        AssertPosix("alnum", 'a', '!');
        AssertPosix("space", ' ', 'a');
        AssertPosix("blank", '\t', 'a');
        AssertPosix("upper", 'A', 'a');
        AssertPosix("lower", 'a', 'A');
        AssertPosix("print", ' ', '\0');
        AssertPosix("graph", 'a', ' ');
        AssertPosix("punct", '.', 'a');
        AssertPosix("cntrl", '\0', 'a');
        AssertPosix("xdigit", 'f', 'g');

        // Unknown class should never match anything
        var unknownTokens = new List<IPatternToken> { new CharacterClassToken(new CharacterClass(false, [new PosixClass("unknown")])) };
        (SegmentInstructionMatcher.MatchSegment("a".AsSpan(), unknownTokens, StringComparison.Ordinal)).Should().BeFalse();
    }

    /// <summary>
    /// Verifies that character range patterns match characters within the specified range.
    /// </summary>
    [TestMethod]
    public void Should_MatchCorrectly_When_CharRangeUsed() {
        var tokens = new List<IPatternToken> { new CharacterClassToken(new CharacterClass(false, [new CharRange('a', 'c')])) };
        (SegmentInstructionMatcher.MatchSegment("a".AsSpan(), tokens, StringComparison.Ordinal)).Should().BeTrue();
        (SegmentInstructionMatcher.MatchSegment("b".AsSpan(), tokens, StringComparison.Ordinal)).Should().BeTrue();
        (SegmentInstructionMatcher.MatchSegment("c".AsSpan(), tokens, StringComparison.Ordinal)).Should().BeTrue();
        (SegmentInstructionMatcher.MatchSegment("d".AsSpan(), tokens, StringComparison.Ordinal)).Should().BeFalse();
    }

    /// <summary>
    /// Verifies that the traversal frontier correctly handles buffer resizing for both stack and queue operations.
    /// </summary>
    [TestMethod]
    public void Should_WorkCorrectly_When_BufferResizing() {
        using var frontier = new TraversalFrontier(initialCapacity: 2);

        // Fill and force resize
        frontier.Push(new TraversalFrame("/a", 0));
        frontier.Push(new TraversalFrame("/b", 0));
        frontier.Push(new TraversalFrame("/c", 0)); // Triggers resize

        frontier.Pop().Directory.Should().Be("/c");
        frontier.Pop().Directory.Should().Be("/b");
        frontier.Pop().Directory.Should().Be("/a");
        frontier.IsEmpty.Should().BeTrue();

        // Test BFS resize behavior
        frontier.Enqueue(new TraversalFrame("/1", 0));
        frontier.Enqueue(new TraversalFrame("/2", 0));
        frontier.Enqueue(new TraversalFrame("/3", 0)); // Triggers resize

        frontier.Dequeue().Directory.Should().Be("/1");
        frontier.Dequeue().Directory.Should().Be("/2");
        frontier.Dequeue().Directory.Should().Be("/3");
    }

    /// <summary>
    /// Verifies that popping or dequeuing from an empty frontier throws an invalid operation exception.
    /// </summary>
    [TestMethod]
    public void Should_Throw_When_EmptyPopDequeue() {
        using var frontier = new TraversalFrontier();
        ((Action)(() => frontier.Pop())).Should().Throw<InvalidOperationException>();
        ((Action)(() => frontier.Dequeue())).Should().Throw<InvalidOperationException>();
    }

    /// <summary>
    /// Verifies that compiled pattern set methods such as grouping, filtering, and equality work correctly.
    /// </summary>
    [TestMethod]
    public void Should_WorkCorrectly_When_VariousMethodsCalled() {
        var mockPattern = new MockCompiledPattern(PatternKind.Glob, isNegated: true, directoryOnly: true, anchoredToRoot: true);
        var set = new CompiledPatternSet([mockPattern]);

        set.Count.Should().Be(1);
        set[0].Should().NotBeNull();
        set.GetEnumerator().Should().NotBeNull();

        var grouped = set.GroupByType().ToList();
        grouped.Should().HaveCount(1);
        grouped[0].PatternKind.Should().Be(PatternKind.Glob);

        set.FindNegated().Should().HaveCount(1);
        set.FindPositive().Should().BeEmpty();
        set.OfType(PatternKind.Glob).Should().HaveCount(1);
        set.OfType(PatternKind.Unknown).Should().BeEmpty();
        set.DirectoryOnly().Should().HaveCount(1);
        set.AnchoredToRoot().Should().HaveCount(1);

        // Equality
        set.Equals(set).Should().BeTrue();
        set.Equals(null).Should().BeFalse();
        set!.Equals(CompiledPatternSet.Empty).Should().BeFalse();

        // HashCode
        set.GetHashCode().Should().NotBe(0);
    }

    /// <summary>
    /// Verifies that delegate-based inclusion on match results behaves as expected.
    /// </summary>
    /// <exception cref="AssertionFailedException">Thrown when the test assertion fails.</exception>
    [TestMethod]
    public void Should_WorkCorrectly_When_DelegateInclusion() {
        TestInclusion(true);
        TestInclusion(false);

        // Check argument null exception
        var result = MatchResult.Fail();
        try {
            result.Include(null!);
            throw new AssertionFailedException("Should have thrown");
        }
        catch(ArgumentNullException) {
            // Ignore the exception, as it's expected behavior
        }
    }

    private static void TestInclusion(bool value) {
        var result = MatchResult.Fail();
        result.Include(() => value);
        (result.IsIncluded == value).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that delegate-based match evaluation on match results behaves as expected.
    /// </summary>
    [TestMethod]
    public void Should_WorkCorrectly_When_DelegateMatch() {
        TestMatch(true);
        TestMatch(false);

        var result = MatchResult.Fail();
        try {
            result.Match(null!);
            throw new AssertionFailedException("Should have thrown");
        }
        catch(ArgumentNullException) {
            // Ignore the exception, as it's expected behavior
        }
    }

    private static void TestMatch(bool value) {
        var result = MatchResult.Fail();
        result.Match(() => value);
        (result.IsMatched == value).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that semantic mode options are set correctly on the built query.
    /// </summary>
    [TestMethod]
    public void Should_SetCorrectOptions_When_SemanticModesUsed() {
        using var env = new TestEnvironment();

        var gitIgnoreQuery = FileQueryBuilder.From(env.Root).UsingGitIgnore().Build();
        gitIgnoreQuery.Options.PatternMatchingMode.Should().Be(PatternMatchingMode.GitIgnore);

        var globQuery = FileQueryBuilder.From(env.Root).UsingGlob().Build();
        globQuery.Options.PatternMatchingMode.Should().Be(PatternMatchingMode.Glob);

        var regexQuery = FileQueryBuilder.From(env.Root).UsingRegex().Build();
        regexQuery.Options.PatternMatchingMode.Should().Be(PatternMatchingMode.Regex);

        var hybridQuery = FileQueryBuilder.From(env.Root).UsingHybrid().Build();
        // InterpretationMode is internal, but we can verify it doesn't throw and uses defaults
        hybridQuery.Should().NotBeNull();
    }

    /// <summary>
    /// Verifies that a conflicting where-typed call throws an invalid operation exception.
    /// </summary>
    [TestMethod]
    public void Should_ThrowOnConflict_When_WhereTypedConflict() {
        using var env = new TestEnvironment();
        var builder = FileQueryBuilder.From(env.Root).UsingGlob();

        // Adding GitIgnore to a Glob-specific builder should throw
        ((Action)(() => builder.Where(PatternKind.GitIgnore, ["/out/"]))).Should().Throw<InvalidOperationException>();
    }

    /// <summary>
    /// Verifies that calling Where with a typed dictionary merges patterns correctly.
    /// </summary>
    [TestMethod]
    public void Should_MergeCorrectly_When_WhereDictionaryCalled() {
        using var env = new TestEnvironment();
        var typedPatterns = new Dictionary<PatternKind, List<string>> {
            [PatternKind.Glob] = ["*.cs"],
            [PatternKind.GitIgnore] = ["bin/"]
        };

        var query = FileQueryBuilder.From(env.Root).Where(typedPatterns).Build();
        query.Options.PatternInput.TypedPatterns.ContainsKey(PatternKind.Glob).Should().BeTrue();
        query.Options.PatternInput.TypedPatterns.ContainsKey(PatternKind.GitIgnore).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that error recovery options are set correctly on the built query.
    /// </summary>
    [TestMethod]
    public void Should_SetOption_When_WithErrorRecoveryCalled() {
        using var env = new TestEnvironment();
        var options = FileQueryErrorRecoveryOptions.Retry(3);
        var query = FileQueryBuilder.From(env.Root).WithErrorRecovery(options).Build();
        query.Options.ErrorRecovery.Action.Should().Be(FileQueryErrorAction.Retry);
        query.Options.ErrorRecovery.MaxRetryAttempts.Should().Be(3);
    }

    /// <summary>
    /// Verifies that the builder executes correctly in both synchronous and asynchronous modes.
    /// </summary>
    [TestMethod]
    public async Task FileQueryBuilder_Execute_ShouldWorkAsync() {
        using var env = new TestEnvironment();
        env.CreateFile("a.txt");

        // Use an explicit mode to avoid any auto-detection ambiguity in this test
        var builder = FileQueryBuilder.From(env.Root).UsingGlob().Where("a.txt");

        // Sync
        var resultsSync = builder.Execute(TestContext.CancellationToken).ToList();
        resultsSync.Should().HaveCount(1);

        // Async
        var resultsAsync = await builder.ExecuteAsync(TestContext.CancellationToken).ToListAsync(TestContext.CancellationToken);
        resultsAsync.Should().HaveCount(1);

        // Async with progress
        var progress = new MockProgress<FileQueryProgress>();
        var resultsProgress = await builder.ExecuteAsync(progress, TestContext.CancellationToken).ToListAsync(TestContext.CancellationToken);
        resultsProgress.Should().HaveCount(1);
    }

    /// <summary>
    /// Verifies that building a query with a missing or blank root path throws an invalid operation exception.
    /// </summary>
    [TestMethod]
    public void Should_Throw_When_MissingRoot() {
        using var env = new TestEnvironment();
        var builder = new FileQueryBuilder("   ", FileSystem.Instance);
        ((Action)(() => builder.Build())).Should().Throw<InvalidOperationException>();
    }

    private sealed class MockProgress<T> : IProgress<T> {
        public void Report(T value) { }
    }

    private sealed class MockCompiledPattern(
        PatternKind kind,
        bool isNegated = false,
        bool directoryOnly = false,
        bool anchoredToRoot = false
    ) : ICompiledPattern {
        public PatternKind PatternKind => kind;
        public bool IsNegated => isNegated;
        public bool DirectoryOnly => directoryOnly;
        public bool AnchoredToRoot => anchoredToRoot;
        public IReadOnlyList<IReadOnlyList<IPatternToken>> Segments => [];
        public CompiledMatchIntent Intent => CompiledMatchIntent.FromNegation(isNegated);
        public string SourceText => string.Empty;
        public int SourceIndex => -1;
        public string ConcretePathAnchor => string.Empty;
        public string LiteralSuffix => string.Empty;
        public string? RegexText => null;
    }

    /// <summary>
    /// Gets or sets the test context for the current test.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;
}
