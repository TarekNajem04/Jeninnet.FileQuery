namespace Jeninnet.FileQuery.Tests.FileEnumeration.Async;

/// <summary>
/// Async tests validating patterns that apply *only to directories*.
/// Covers:
///   - trailing slash rules
///   - directory-only exclusion
///   - recursive directory ignore matching
/// </summary>
[TestClass]
public class EnumerateFilesAsync_DirectoryOnlyPatternTests {
    /// <summary>
    /// Ensures directory-only pattern excludes an entire folder.
    /// </summary>
    [TestMethod]
    public async Task Should_ExcludeFolder_When_DirectoryOnlyPatternUsed_Async() {
        using var env = new TestEnvironment();

        env.CreateFiles(
            "visible.txt",
            "secret/file1.txt",
            "secret/file2.log"
        );

        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                         "secret/"     // directory-only
                        //"secret/**"   // directory-only ignore
                    ]
                )
            )
        );

        var fileQueryEngine = FileQueryRuntime.Create();
        var results = await fileQueryEngine.ExecuteAsync(new(env.Root, options), TestContext.CancellationToken)
                                           .ToListAsync(TestContext.CancellationToken);

        results.Should().ContainSingle(x => x.EndsWith("visible.txt", StringComparison.Ordinal));
        results.Should().NotContain(x => x.Contains("secret", StringComparison.Ordinal));
    }

    /// <summary>
    /// A more complex directory-only include + exclude combination.
    /// This test proves files inside a directory-excluded path can be re-included.
    /// </summary>
    [TestMethod]
    public async Task Should_RestoreSubDir_When_DirectoryOnlyRuleApplied_Async() {
        using var env = new TestEnvironment();

        env.CreateFiles(
            "ignore_me/root.txt",
            "ignore_me/recover/file.txt", // <--- Expected to be the ONLY included file
            "ignore_me/bin/file.txt"
        );

        // The full, correct pattern set for strict GitIgnore re-inclusion:
        // 1. Global inclusion (always needed when patterns are explicitly set)
        // 2. Exclusion of all contents under the directory (which allows traversal)
        // 3. Re-inclusion of the TARGET SUBDIRECTORY ITSELF (to prevent its own pruning)
        // 4. Re-inclusion of the files inside the target subdirectory
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "ignore_me/**",             // Exclude ALL contents (files/subdirs) under 'ignore_me'
                        "!ignore_me/recover/**",    // Re-include the DIRECTORY 'recover/' itself (allows traversal)
                    ]
                )
            )
        );
        var fileQueryEngine = FileQueryRuntime.Create();
        var results = await fileQueryEngine.ExecuteAsync(new(env.Root, options), TestContext.CancellationToken)
                                           .ToListAsync(TestContext.CancellationToken);

        // Only the recovered file is expected to be present.
        results.Should().ContainSingle(x => x.EndsWith(Path.Combine("recover", "file.txt"), StringComparison.Ordinal));
    }

    /// <summary>
    /// Tests directory-only patterns combined with ** recursion.
    /// </summary>
    [TestMethod]
    public async Task Should_Work_When_DirectoryOnlyWithRecursiveWildcard_Async() {
        using var env = new TestEnvironment();

        env.CreateFiles(
            "a/b/c/hidden1.txt",
            "a/b2/c2/hidden2.txt",
            "a/b3x/c2/hidden3.txt",
            "visible.txt",
            "b/200txt"
        );

        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**/[b]*/",    // ignore any folder named starting with 'b' at ANY level
                    ]
                )
            )
        );

        var fileQueryEngine = FileQueryRuntime.Create();
        var results = await fileQueryEngine.ExecuteAsync(new(env.Root, options), TestContext.CancellationToken)
                                           .ToListAsync(TestContext.CancellationToken);

        results.Should().ContainSingle(x => x.EndWithNormalized("visible.txt"));
        results.Should().HaveCount(1);
    }

    /// <summary>
    /// Gets or sets the test context providing cancellation and diagnostic information.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;
}

