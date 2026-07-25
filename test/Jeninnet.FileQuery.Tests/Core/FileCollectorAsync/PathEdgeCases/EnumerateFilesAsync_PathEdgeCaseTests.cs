namespace Jeninnet.FileQuery.Tests.Core.FileCollectorAsync.PathEdgeCases;

/// <summary>
/// Async tests verifying behavior on tricky or unusual paths.
/// Focus:
///     - trailing slashes
///     - weird names
///     - unicode
///     - spaces and dots
///     - case sensitivity (async + options.IgnoreCase)
/// </summary>
[TestClass]
public class EnumerateFilesAsync_PathEdgeCaseTests {
    /// <summary>
    /// Ensures directory ending with a slash is accepted and normalized.
    /// </summary>
    [TestMethod]
    public async Task EnumerateFilesAsync_TrailingSlash_ShouldWorkAsync() {
        using var env = new TestEnvironment();

        env.CreateFiles("file.txt", "file1.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!file.txt"
                    ]
                )
            )
        );

        var rootWithSlash = env.Root + "/";
        var results = await fileQueryEngine.ExecuteAsync(new(rootWithSlash, options), TestContext.CancellationToken)
                                           .ToListAsync(TestContext.CancellationToken);

        TestAssertEx.ContainsSingle(results, x => x.EndsWith("file.txt", StringComparison.Ordinal));
    }

    /// <summary>
    /// Tests Unicode file name handling.
    /// </summary>
    [TestMethod]
    public async Task EnumerateFilesAsync_UnicodeNames_ShouldWorkAsync() {
        using var env = new TestEnvironment();

        env.CreateFiles("Test测试.log", "Test测试.txt", "file😍.txt", "file1😀.txT", "file2👍.TXT", "Test😁.txt", "Test07😅.txt");

        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!*.txt"
                    ]
                ),
                CaseSensitivity: CaseSensitivity.Sensitive
            )
        );

        var fileQueryEngine = FileQueryRuntime.Create();
        var results = await fileQueryEngine.ExecuteAsync(new(env.Root, options), TestContext.CancellationToken)
                                           .ToListAsync(TestContext.CancellationToken);

        TestAssertEx.ContainsSingle(results, x => x.Contains("Test测试.txt", StringComparison.Ordinal));
        TestAssertEx.ContainsSingle(results, x => x.Contains("file😍.txt", StringComparison.Ordinal));
        TestAssertEx.ContainsSingle(results, x => x.Contains("Test😁.txt", StringComparison.Ordinal));
        TestAssertEx.ContainsSingle(results, x => x.Contains("Test07😅.txt", StringComparison.Ordinal));

        TestAssertEx.DoesNotContain(results, x => x.Contains("Test测试.log", StringComparison.Ordinal));
        TestAssertEx.DoesNotContain(results, x => x.Contains("file1😀.txT", StringComparison.Ordinal));
        TestAssertEx.DoesNotContain(results, x => x.Contains("file2👍.TXT", StringComparison.Ordinal));
    }

    /// <summary>
    /// Ensures IgnoreCase works asynchronously across platforms.
    /// </summary>
    [TestMethod]
    public async Task EnumerateFilesAsync_IgnoreCase_ShouldMatchMixedCaseAsync() {
        using var env = new TestEnvironment();

        env.CreateFiles("Alpha.TXT", "beta.txt", "GAMMA.TxT");

        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!**/*"
                    ]
                ),
                CaseSensitivity: CaseSensitivity.Insensitive
            )
        );

        var fileQueryEngine = FileQueryRuntime.Create();
        var results = await fileQueryEngine.ExecuteAsync(new(env.Root, options), TestContext.CancellationToken)
                                           .ToListAsync(TestContext.CancellationToken);

        TestAssertEx.HasCount(results, 3);
    }

    public TestContext TestContext { get; set; } = null!;
}

