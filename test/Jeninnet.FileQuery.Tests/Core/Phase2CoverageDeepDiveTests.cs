namespace Jeninnet.FileQuery.Tests.Core;

[TestClass]
public sealed class Phase2CoverageDeepDiveTests
{
    [TestMethod]
    public void SegmentInstructionMatcher_PosixClasses_ShouldMatchCorrectly()
    {
        static void AssertPosix(string className, char match, char noMatch)
        {
            var tokens = new List<IPatternToken> { new CharacterClassToken(new CharacterClass(false, new[] { new PosixClass(className) })) };
            Assert.IsTrue(SegmentInstructionMatcher.MatchSegment(match.ToString().AsSpan(), tokens, StringComparison.Ordinal), $"Expected {match} to match {className}");
            Assert.IsFalse(SegmentInstructionMatcher.MatchSegment(noMatch.ToString().AsSpan(), tokens, StringComparison.Ordinal), $"Expected {noMatch} NOT to match {className}");
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
        var unknownTokens = new List<IPatternToken> { new CharacterClassToken(new CharacterClass(false, new[] { new PosixClass("unknown") })) };
        Assert.IsFalse(SegmentInstructionMatcher.MatchSegment("a".AsSpan(), unknownTokens, StringComparison.Ordinal));
    }

    [TestMethod]
    public void SegmentInstructionMatcher_CharRange_ShouldMatchCorrectly()
    {
        var tokens = new List<IPatternToken> { new CharacterClassToken(new CharacterClass(false, new[] { new CharRange('a', 'c') })) };
        Assert.IsTrue(SegmentInstructionMatcher.MatchSegment("a".AsSpan(), tokens, StringComparison.Ordinal));
        Assert.IsTrue(SegmentInstructionMatcher.MatchSegment("b".AsSpan(), tokens, StringComparison.Ordinal));
        Assert.IsTrue(SegmentInstructionMatcher.MatchSegment("c".AsSpan(), tokens, StringComparison.Ordinal));
        Assert.IsFalse(SegmentInstructionMatcher.MatchSegment("d".AsSpan(), tokens, StringComparison.Ordinal));
    }

    [TestMethod]
    public void TraversalFrontier_BufferResizing_ShouldWorkCorrectly()
    {
        using var frontier = new TraversalFrontier(initialCapacity: 2);

        // Fill and force resize
        frontier.Push(new TraversalFrame("/a", 0));
        frontier.Push(new TraversalFrame("/b", 0));
        frontier.Push(new TraversalFrame("/c", 0)); // Triggers resize

        Assert.AreEqual("/c", frontier.Pop().Directory);
        Assert.AreEqual("/b", frontier.Pop().Directory);
        Assert.AreEqual("/a", frontier.Pop().Directory);
        Assert.IsTrue(frontier.IsEmpty);

        // Test BFS resize behavior
        frontier.Enqueue(new TraversalFrame("/1", 0));
        frontier.Enqueue(new TraversalFrame("/2", 0));
        frontier.Enqueue(new TraversalFrame("/3", 0)); // Triggers resize

        Assert.AreEqual("/1", frontier.Dequeue().Directory);
        Assert.AreEqual("/2", frontier.Dequeue().Directory);
        Assert.AreEqual("/3", frontier.Dequeue().Directory);
    }

    [TestMethod]
    public void TraversalFrontier_EmptyPopDequeue_ShouldThrow()
    {
        using var frontier = new TraversalFrontier();
        TestAssertEx.Throws<InvalidOperationException>(() => frontier.Pop());
        TestAssertEx.Throws<InvalidOperationException>(() => frontier.Dequeue());
    }

    [TestMethod]
    public void CompiledPatternSet_VariousMethods_ShouldWorkCorrectly()
    {
        var mockPattern = new MockCompiledPattern(PatternKind.Glob, isNegated: true, directoryOnly: true, anchoredToRoot: true);
        var set = new CompiledPatternSet(new[] { mockPattern });

        Assert.AreEqual(1, set.Count);
        Assert.IsNotNull(set[0]);
        Assert.IsNotNull(set.GetEnumerator());

        var grouped = set.GroupByType().ToList();
        Assert.HasCount(1, grouped);
        Assert.AreEqual(PatternKind.Glob, grouped[0].PatternKind);

        Assert.HasCount(1, set.FindNegated());
        Assert.IsEmpty(set.FindPositive());
        Assert.HasCount(1, set.OfType(PatternKind.Glob));
        Assert.IsEmpty(set.OfType(PatternKind.Unknown));
        Assert.HasCount(1, set.DirectoryOnly());
        Assert.HasCount(1, set.AnchoredToRoot());

        // Equality
        Assert.IsTrue(set.Equals(set));
        Assert.IsFalse(set.Equals(null));
        Assert.IsFalse(set.Equals(CompiledPatternSet.Empty));

        // HashCode
        Assert.AreNotEqual(0, set.GetHashCode());
    }

    [TestMethod]
    public void MatchResult_DelegateInclusion_ShouldWorkCorrectly()
    {
        TestInclusion(true);
        TestInclusion(false);

        // Check argument null exception
        var result = MatchResult.Fail();
        try
        {
            result.Include(null!);
            Assert.Fail("Should have thrown");
        }
        catch(ArgumentNullException)
        {
            // Ignore the exception, as it's expected behavior
        }
    }

    private static void TestInclusion(bool value)
    {
        var result = MatchResult.Fail();
        result.Include(() => value);
        Assert.AreEqual(value, result.IsIncluded);
    }

    [TestMethod]
    public void MatchResult_DelegateMatch_ShouldWorkCorrectly()
    {
        TestMatch(true);
        TestMatch(false);

        var result = MatchResult.Fail();
        try
        {
            result.Match(null!);
            Assert.Fail("Should have thrown");
        }
        catch(ArgumentNullException)
        {
            // Ignore the exception, as it's expected behavior
        }
    }

    private static void TestMatch(bool value)
    {
        var result = MatchResult.Fail();
        result.Match(() => value);
        Assert.AreEqual(value, result.IsMatched);
    }

    [TestMethod]
    public void FileQueryBuilder_SemanticModes_ShouldSetCorrectOptions()
    {
        using var env = new TestEnvironment();

        var gitIgnoreQuery = FileQueryBuilder.From(env.Root).UsingGitIgnore().Build();
        Assert.AreEqual(PatternMatchingMode.GitIgnore, gitIgnoreQuery.Options.PatternMatchingMode);

        var globQuery = FileQueryBuilder.From(env.Root).UsingGlob().Build();
        Assert.AreEqual(PatternMatchingMode.Glob, globQuery.Options.PatternMatchingMode);

        var regexQuery = FileQueryBuilder.From(env.Root).UsingRegex().Build();
        Assert.AreEqual(PatternMatchingMode.Regex, regexQuery.Options.PatternMatchingMode);

        var hybridQuery = FileQueryBuilder.From(env.Root).UsingHybrid().Build();
        // InterpretationMode is internal, but we can verify it doesn't throw and uses defaults
        Assert.IsNotNull(hybridQuery);
    }

    [TestMethod]
    public void FileQueryBuilder_WhereTyped_ShouldThrowOnConflict()
    {
        using var env = new TestEnvironment();
        var builder = FileQueryBuilder.From(env.Root).UsingGlob();

        // Adding GitIgnore to a Glob-specific builder should throw
        TestAssertEx.Throws<InvalidOperationException>(() => builder.Where(PatternKind.GitIgnore, ["/out/"]));
    }

    [TestMethod]
    public void FileQueryBuilder_WhereDictionary_ShouldMergeCorrectly()
    {
        using var env = new TestEnvironment();
        var typedPatterns = new Dictionary<PatternKind, List<string>>
        {
            [PatternKind.Glob] = ["*.cs"],
            [PatternKind.GitIgnore] = ["bin/"]
        };

        var query = FileQueryBuilder.From(env.Root).Where(typedPatterns).Build();
        Assert.IsTrue(query.Options.PatternInput.TypedPatterns.ContainsKey(PatternKind.Glob));
        Assert.IsTrue(query.Options.PatternInput.TypedPatterns.ContainsKey(PatternKind.GitIgnore));
    }

    [TestMethod]
    public void FileQueryBuilder_WithErrorRecovery_ShouldSetOption()
    {
        using var env = new TestEnvironment();
        var options = FileQueryErrorRecoveryOptions.Retry(3);
        var query = FileQueryBuilder.From(env.Root).WithErrorRecovery(options).Build();
        Assert.AreEqual(FileQueryErrorAction.Retry, query.Options.ErrorRecovery.Action);
        Assert.AreEqual(3, query.Options.ErrorRecovery.MaxRetryAttempts);
    }

    [TestMethod]
    public async Task FileQueryBuilder_Execute_ShouldWorkAsync()
    {
        using var env = new TestEnvironment();
        env.CreateFile("a.txt");

        // Use an explicit mode to avoid any auto-detection ambiguity in this test
        var builder = FileQueryBuilder.From(env.Root).UsingGlob().Where("a.txt");

        // Sync
        var resultsSync = builder.Execute().ToList();
        Assert.HasCount(1, resultsSync);

        // Async
        var resultsAsync = await builder.ExecuteAsync(TestContext?.CancellationToken ?? CancellationToken.None).ToListAsync(TestContext?.CancellationToken ?? CancellationToken.None);
        Assert.HasCount(1, resultsAsync);

        // Async with progress
        var progress = new MockProgress<FileQueryProgress>();
        var resultsProgress = await builder.ExecuteAsync(progress, TestContext?.CancellationToken ?? CancellationToken.None).ToListAsync(TestContext?.CancellationToken ?? CancellationToken.None);
        Assert.HasCount(1, resultsProgress);
    }

    [TestMethod]
    public void FileQueryBuilder_Build_ShouldThrowOnMissingRoot()
    {
        using var env = new TestEnvironment();
        var builder = new FileQueryBuilder("   ", FileSystem.Instance);
        TestAssertEx.Throws<InvalidOperationException>(() => builder.Build());
    }

    private sealed class MockProgress<T> : IProgress<T>
    {
        public void Report(T value) { }
    }

    private sealed class MockCompiledPattern(
        PatternKind kind,
        bool isNegated = false,
        bool directoryOnly = false,
        bool anchoredToRoot = false
    ) : ICompiledPattern
    {
        public PatternKind PatternKind => kind;
        public bool IsNegated => isNegated;
        public bool DirectoryOnly => directoryOnly;
        public bool AnchoredToRoot => anchoredToRoot;
        public IReadOnlyList<IReadOnlyList<IPatternToken>> Segments => [];
        public CompiledMatchIntent Intent => CompiledMatchIntent.FromNegation(isNegated);
        public string SourceText => string.Empty;
        public int SourceIndex => -1;
        public string ConcretePathAnchor => string.Empty;
        public string? RegexText => null;
    }

    public TestContext? TestContext { get; set; }
}
