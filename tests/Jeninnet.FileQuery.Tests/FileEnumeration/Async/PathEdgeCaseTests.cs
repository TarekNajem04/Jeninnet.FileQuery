namespace Jeninnet.FileQuery.Tests.FileEnumeration.Async;

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
    public async Task Should_HandleTrailingSlash_When_PathHasEdgeCases_Async() {
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

        results.Should().ContainSingle(x => x.EndsWith("file.txt", StringComparison.Ordinal));
    }

    /// <summary>
    /// Tests Unicode file name handling.
    /// </summary>
    [TestMethod]
    public async Task Should_HandleUnicodeNames_When_PathHasEdgeCases_Async() {
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

        results.Should().ContainSingle(x => x.Contains("Test测试.txt", StringComparison.Ordinal));
        results.Should().ContainSingle(x => x.Contains("file😍.txt", StringComparison.Ordinal));
        results.Should().ContainSingle(x => x.Contains("Test😁.txt", StringComparison.Ordinal));
        results.Should().ContainSingle(x => x.Contains("Test07😅.txt", StringComparison.Ordinal));

        results.Should().NotContain(x => x.Contains("Test测试.log", StringComparison.Ordinal));
        results.Should().NotContain(x => x.Contains("file1😀.txT", StringComparison.Ordinal));
        results.Should().NotContain(x => x.Contains("file2👍.TXT", StringComparison.Ordinal));
    }

    /// <summary>
    /// Ensures IgnoreCase works asynchronously across platforms.
    /// </summary>
    [TestMethod]
    public async Task Should_MatchMixedCase_When_IgnoreCaseEnabled_Async() {
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

        results.Should().HaveCount(3);
    }

    /// <summary>
    /// Gets or sets the test context providing cancellation and diagnostic information.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;
}

