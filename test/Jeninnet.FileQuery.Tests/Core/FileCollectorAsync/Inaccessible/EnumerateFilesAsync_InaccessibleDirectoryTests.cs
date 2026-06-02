namespace Jeninnet.FileQuery.Tests.Core.FileCollectorAsync.Inaccessible;

/// <summary>
/// Async tests validating behavior when encountering directories
/// that cannot be accessed (IO exceptions).
/// Ensures async implementation:
///     - respects IgnoreInaccessible
///     - throws when the option is false
///     - continues enumeration properly
/// </summary>
[TestClass]
public class EnumerateFilesAsync_InaccessibleDirectoryTests {
    /// <summary>
    /// Ensures inaccessible directories cause exceptions when IgnoreInaccessible = false.
    /// </summary>
    [TestMethod]
    public async Task EnumerateFilesAsync_InaccessibleDir_ShouldThrowAsync() {
        using var env = new TestEnvironment();

        env.CreateFiles("blocked/ok1.txt", "blocked/ok2.txt");
        env.CreateInaccessibleDirectory("blocked");
        env.AssertDirectoryInaccessible("blocked");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            patternInput: new(
                patterns: [
                    "**",
                "!**/*"
                ]
            ),
            recurseSubdirectories: true,
            ignoreInaccessible: false
        );

        await TestAssertEx.ThrowsAsync<Exception>(async () => {
            try {
                await foreach(var _ in fileQueryEngine.ExecuteAsync(
                    new(env.Root, options),
                    TestContext.CancellationToken)) {
                    // Force traversal into the inaccessible directory
                }

                // If we reach this point, no exception was thrown → fail explicitly
                throw new AssertFailedException("Expected an exception due to inaccessible directory, but none was thrown.");
            }
            catch(Exception ex) when(
                ex is DirectoryNotFoundException or IOException or UnauthorizedAccessException
            ) {
                // Valid exception → rethrow so ThrowsAsync can catch it
                throw;
            }
        });
    }

    /// <summary>
    /// Ensures inaccessible directories are skipped when IgnoreInaccessible = true.
    /// </summary>
    [TestMethod]
    public async Task EnumerateFilesAsync_InaccessibleDir_ShouldSkipAsync() {
        using var env = new TestEnvironment();

        env.CreateFiles("root.txt");
        // Use broken symlink to make directory inaccessible
        env.SetInaccessibleDirectory("locked");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            patternInput: new(
                patterns: ["**", "!**/*"]
            ),
            recurseSubdirectories: true,
            ignoreInaccessible: true
        );

        var results = await fileQueryEngine.ExecuteAsync(new(env.Root, options), TestContext.CancellationToken)
                                           .ToListAsync(TestContext.CancellationToken);

        // inaccessible dir is skipped
        TestAssertEx.ContainsSingle(results);
        TestAssertEx.EndsWith(results.Single(), "root.txt");
    }

    /// <summary>
    /// Ensures async enumeration continues after skipping multiple inaccessible dirs.
    /// </summary>
    [TestMethod]
    public async Task EnumerateFilesAsync_MultipleInaccessibleDirs_ShouldSkipAllAsync() {
        using var env = new TestEnvironment();

        env.CreateFiles("ok1.txt", "ok2.txt");
        env.CreateFiles("bad1/ok1.txt", "bad2/ok2.txt");

        // Mark bad1 and bad2 as inaccessible (broken symlinks)
        env.SetInaccessibleDirectory("bad1");
        env.SetInaccessibleDirectory("bad2");
        env.CreateFile("okSub/inner.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            patternInput: new(
                patterns: [
                    "**",
                    "!**/*"
                ]
            ),
            recurseSubdirectories: true,
            ignoreInaccessible: true
        );

        var results = await fileQueryEngine.ExecuteAsync(new(env.Root, options), TestContext.CancellationToken)
                                           .ToListAsync(TestContext.CancellationToken);

        // Should get 3 files: ok1, ok2, okSub/inner
        TestAssertEx.HasCount(results, 3);
    }

    public TestContext TestContext { get; set; } = null!;
}
