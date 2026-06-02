namespace Jeninnet.FileQuery.Tests.Core.FileCollectorAsync.AnchoredAndMixed;

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
    public async Task EnumerateFilesAsync_AnchoredPattern_ShouldOnlyMatchAtRootAsync() {
        using var env = new TestEnvironment();
        env.CreateFiles(
            "root.txt",
            "sub/root.txt"
        );

        var options = new FileQueryOptions(
            patternInput: new(
                patterns: [
                    "*.txt",   // Exclude all .txt files"
                    "!/root.txt" // Anchored to root only
                ]
            )
        );

        var fileQueryEngine = FileQueryRuntime.Create();
        FileQuery query = new(env.Root, options);
        var results = await fileQueryEngine.ExecuteAsync(query, TestContext.CancellationToken)
                                           .ToListAsync(TestContext.CancellationToken);

        // Must include only root.txt at top-level
        TestAssertEx.ContainsSingle(results, x => x.EndsWith("root.txt"));
        TestAssertEx.DoesNotContain(results, x => x.Contains("sub/root.txt"));
    }

    /// <summary>
    /// Tests a complex mixed-token pattern.
    /// </summary>
    [TestMethod]
    public async Task EnumerateFilesAsync_MixedTokens_ShouldMatchCorrectlyAsync() {
        using var env = new TestEnvironment();

        env.CreateFiles(
            "abc1/data/file.txt",
            "abc9/data/file.txt",
            "axcZZ/data/file.txt",
            "axxc12/data/not.txt"
        );

        var options = new FileQueryOptions(
            patternInput: new(
                // a?c* matches: abc1, abc9, axcZZ
                // a?c* FAILS to match: axxc12 (because the third char is 'x', not 'c')
                patterns: [
                    "**",               // Exclude everything first
                    "!a?c*/data/*.txt"  // Include matching patterns
                ]
            )
        );

        var fileQueryEngine = FileQueryRuntime.Create();
        var results = await fileQueryEngine.ExecuteAsync(new(env.Root, options), TestContext.CancellationToken)
                                           .ToListAsync(TestContext.CancellationToken);
        TestAssertEx.HasCount(results, 3);
        TestAssertEx.Contains(results, x => x.Contains("abc1"));
        TestAssertEx.Contains(results, x => x.Contains("abc9"));
        TestAssertEx.Contains(results, x => x.Contains("axcZZ"));
        TestAssertEx.DoesNotContain(results, x => x.Contains("axxc12"));
        TestAssertEx.DoesNotContain(results, x => x.EndsWith("not.txt"));
    }

    /// <summary>
    /// Ensures anchored + wildcard paths behave correctly.
    /// </summary>
    [TestMethod]
    public async Task EnumerateFilesAsync_AnchoredWildcard_ShouldBehaveAsync() {
        using var env = new TestEnvironment();

        env.CreateFiles(
            "top/file.txt",
            "other/top/file.txt"
        );

        var options = new FileQueryOptions(
            patternInput: new(
                patterns: [
                    "**",              // Exclude everything
                    "!/top/*.txt"    // should not match nested top/
                ]
            )
        );

        var fileQueryEngine = FileQueryRuntime.Create();
        var results = await fileQueryEngine.ExecuteAsync(new(env.Root, options), TestContext.CancellationToken)
                                           .ToListAsync(TestContext.CancellationToken);
        var expected = Path.Combine("top", "file.txt");
        TestAssertEx.ContainsSingle(results, x => x.EndsWith(expected));
    }

    public TestContext TestContext { get; set; } = null!;
}
