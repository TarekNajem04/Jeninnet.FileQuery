namespace Jeninnet.FileQuery.Tests.FileEnumeration.Async;

/// <summary>
/// Tests complex glob rule combinations:
///   - Anchored patterns starting with '/'
///   - Mixed '?', '*', '**', and character classes
///   - Multi-token directory/file name patterns
/// </summary>
[TestClass]
public class EnumerateFilesAsync_AnchoredAndMixedPatternTests {
    /// <summary>
    /// Checks anchored patterns — those starting with '/' should only match from root.
    /// </summary>
    [TestMethod]
    public async Task Should_OnlyMatchAtRoot_When_AnchoredPatternUsed_Async() {
        using var env = new TestEnvironment();
        env.CreateFiles(
            "root.txt",
            "sub/root.txt"
        );

        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "*.txt",   // Exclude all .txt files"
                        "!/root.txt" // Anchored to root only
                    ]
                )
            )
        );

        var fileQueryEngine = FileQueryRuntime.Create();
        FileQuery query = new(env.Root, options);
        var results = await fileQueryEngine.ExecuteAsync(query, TestContext.CancellationToken)
                                           .ToListAsync(TestContext.CancellationToken);

        // Must include only root.txt at top-level
        results.Should().ContainSingle(x => x.EndsWith("root.txt", StringComparison.Ordinal));
        results.Should().NotContain(x => x.Contains("sub/root.txt", StringComparison.Ordinal));
    }

    /// <summary>
    /// Tests a complex mixed-token pattern.
    /// </summary>
    [TestMethod]
    public async Task Should_MatchCorrectly_When_MixedTokensUsed_Async() {
        using var env = new TestEnvironment();

        env.CreateFiles(
            "abc1/data/file.txt",
            "abc9/data/file.txt",
            "axcZZ/data/file.txt",
            "axxc12/data/not.txt"
        );

        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    // a?c* matches: abc1, abc9, axcZZ
                    // a?c* FAILS to match: axxc12 (because the third char is 'x', not 'c')
                    Patterns: [
                        "**",               // Exclude everything first
                        "!a?c*/data/*.txt"  // Include matching patterns
                    ]
                )
            )
        );

        var fileQueryEngine = FileQueryRuntime.Create();
        var results = await fileQueryEngine.ExecuteAsync(new(env.Root, options), TestContext.CancellationToken)
                                           .ToListAsync(TestContext.CancellationToken);
        results.Should().HaveCount(3);
        results.Should().Contain(x => x.Contains("abc1", StringComparison.Ordinal));
        results.Should().Contain(x => x.Contains("abc9", StringComparison.Ordinal));
        results.Should().Contain(x => x.Contains("axcZZ", StringComparison.Ordinal));
        results.Should().NotContain(x => x.Contains("axxc12", StringComparison.Ordinal));
        results.Should().NotContain(x => x.EndsWith("not.txt", StringComparison.Ordinal));
    }

    /// <summary>
    /// Ensures anchored + wildcard paths behave correctly.
    /// </summary>
    [TestMethod]
    public async Task Should_BehaveCorrectly_When_AnchoredWildcardUsed_Async() {
        using var env = new TestEnvironment();

        env.CreateFiles(
            "top/file.txt",
            "other/top/file.txt"
        );

        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",              // Exclude everything
                        "!/top/*.txt"    // should not match nested top/
                    ]
                )
            )
        );

        var fileQueryEngine = FileQueryRuntime.Create();
        var results = await fileQueryEngine.ExecuteAsync(new(env.Root, options), TestContext.CancellationToken)
                                           .ToListAsync(TestContext.CancellationToken);
        var expected = Path.Combine("top", "file.txt");
        results.Should().ContainSingle(x => x.EndsWith(expected, StringComparison.Ordinal));
    }

    /// <summary>
    /// Gets or sets the test context providing cancellation and diagnostic information.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;
}

