namespace Jeninnet.FileQuery.Tests.Core.FileCollectorAsync.NegationAndPrecedence;

/// <summary>
/// Async tests that validate hybrid GitIgnore-style precedence rules:
///  - Patterns are evaluated in order
///  - Last matching rule wins
///  - Negation (!) restores inclusion
/// These are the most critical tests for correctness.
/// </summary>
[TestClass]
public class EnumerateFilesAsync_NegationAndPrecedenceTests {
    /// <summary>
    /// Validates that a negated pattern re-includes a file that was previously excluded.
    /// </summary>
    [TestMethod]
    public async Task EnumerateFilesAsync_Negation_ShouldReIncludeFilesAsync() {
        using var env = new TestEnvironment();

        env.CreateFiles("a.txt", "b.txt", "c.log");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            // Exclude all .txt, but explicitly re-include b.txt
            patternInput: new(
                patterns: [
                    "**",       // exclude all files
                    "!b.txt"    // re-include b.txt
                ]
            )
        );

        var results = await fileQueryEngine.ExecuteAsync(new(env.Root, options), TestContext.CancellationToken)
                                           .ToListAsync(TestContext.CancellationToken);

        TestAssertEx.DoesNotContain(results, x => x.EndsWith("a.txt"));
        TestAssertEx.Contains(results, x => x.EndsWith("b.txt"));
        TestAssertEx.DoesNotContain(results, x => x.EndsWith("c.log"));
    }

    /// <summary>
    /// Tests "last rule wins" behavior across async traversal.
    /// </summary>
    [TestMethod]
    public async Task EnumerateFilesAsync_LastRuleWins_ShouldApplyAsync() {
        using var env = new TestEnvironment();

        env.CreateFiles("file.tmp", "file.txt");

        var options = new FileQueryOptions(
            // First rule excludes txt, second rule re-includes txt
            patternInput: new(
                patterns: [
                    "**",       // exclude all files
                    "!*.txt"    // re-include all .txt files
                ]
            )
        );

        var fileQueryEngine = FileQueryRuntime.Create();
        var results = await fileQueryEngine.ExecuteAsync(new(env.Root, options), TestContext.CancellationToken)
                                           .ToListAsync(TestContext.CancellationToken);

        // Because last rule wins, file.txt *must* be included
        TestAssertEx.ContainsSingle(results, x => x.EndsWith("file.txt"));
    }

    /// <summary>
    /// Complex case mixing exclusion, inclusion, and wildcard paths.
    /// </summary>
    [TestMethod]
    public async Task EnumerateFilesAsync_ComplexNegationOrdering_ShouldBehaveCorrectlyAsync() {
        using var env = new TestEnvironment();

        env.CreateFiles(
            "x/a.txt",
            "x/b.txt",
            "x/c.tmp",
            "y/b.txt"
        );

        var options = new FileQueryOptions(
            patternInput: new(
                patterns: [
                    "**",           // exclude all files
                    "!**/*.txt",    // include all .txt
                    "x/b.txt",      // override: exclude this specific file
                    "!x/b.txt",     // re-include it again
                    "y/*.txt",      // exclude all .txt in folder y
                ]
            )
        );

        var fileQueryEngine = FileQueryRuntime.Create();
        var results = await fileQueryEngine.ExecuteAsync(new(env.Root, options), TestContext.CancellationToken)
                                           .ToListAsync(TestContext.CancellationToken);

        TestAssertEx.Contains(results, x => x.EndsWith(Path.Combine("x", "a.txt")));
        TestAssertEx.Contains(results, x => x.EndsWith(Path.Combine("x", "b.txt")));  // re-included
        TestAssertEx.DoesNotContain(results, x => x.EndsWith(Path.Combine("y", "b.txt")));
        TestAssertEx.DoesNotContain(results, x => x.EndsWith(Path.Combine("x", "c.tmp")));
    }

    public TestContext TestContext { get; set; } = null!;
}
