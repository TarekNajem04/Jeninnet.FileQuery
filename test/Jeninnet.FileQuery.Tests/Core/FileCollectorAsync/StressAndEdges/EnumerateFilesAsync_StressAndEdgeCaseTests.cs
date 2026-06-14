namespace Jeninnet.FileQuery.Tests.Core.FileCollectorAsync.StressAndEdges;

/// <summary>
/// Stress and edge-case tests designed to detect rare async bugs:
///   - Large number of files
///   - Patterns resembling directories but not ending with '/'
///   - Patterns with tricky ordering
///   - Performance consistency under async enumeration
/// </summary>
[TestClass]
public class EnumerateFilesAsync_StressAndEdgeCaseTests
{
    /// <summary>
    /// Stress test: Verify async enumeration works correctly with many files.
    /// </summary>
    [TestMethod]
    public async Task EnumerateFilesAsync_Stress_ShouldHandleManyFilesAsync()
    {
        using var env = new TestEnvironment();

        for(var i = 0; i < 200; i++)
        {
            env.CreateFile($"group/file{i}.txt");
        }

        var options = new FileQueryOptions(
            patternInput: new(
                patterns: [
                    "**",
                    "!**/*.txt"
                ]
            )
        );

        var fileQueryEngine = FileQueryRuntime.Create();
        var results = await fileQueryEngine.ExecuteAsync(new(env.Root, options), TestContext.CancellationToken)
                                           .ToListAsync(TestContext.CancellationToken);

        TestAssertEx.HasCount(results, 200);
    }

    /// <summary>
    /// When a pattern looks like a directory but is not actually marked as one,
    /// it must be treated as a file pattern.
    /// </summary>
    [TestMethod]
    public async Task EnumerateFilesAsync_FakeDirectoryPattern_ShouldNotIgnoreFolderAsync()
    {
        using var env = new TestEnvironment();

        env.CreateFiles(
            "folder/data.txt",
            "folderX/data.txt"
        );

        var options = new FileQueryOptions(
            patternInput: new(
                patterns: [
                    "**",
                    "folder" // not "folder/"
                ]
            )
        );

        var fileQueryEngine = FileQueryRuntime.Create();
        var results = await fileQueryEngine.ExecuteAsync(new(env.Root, options), TestContext.CancellationToken)
                                           .ToListAsync(TestContext.CancellationToken);

        // "folder" alone = match file/directory named "folder" (not its children)
        TestAssertEx.IsEmpty(results);
    }

    /// <summary>
    /// Ensures async behavior is identical to sync when combining many rules.
    /// </summary>
    [TestMethod]
    public async Task EnumerateFilesAsync_ComplexRuleSet_ShouldReturnSameAsSyncAsync()
    {
        using var env = new TestEnvironment();

        env.CreateFiles(
            "a/b/c/file.txt",
            "a/b/file.log",
            "a/c/z/file.txt"
        );

        var options = new FileQueryOptions(
            patternInput: new(
                patterns: [
                    "**",
                    "!**/*.txt",
                    "a/b/*",       // exclude b folder
                    "!a/b/c/*.txt",  // but re-include nested c inside b
                ]
            )
        );

        var fileQueryEngine = FileQueryRuntime.Create();

        // async
        var asyncResults = await fileQueryEngine.ExecuteAsync(new(env.Root, options), TestContext.CancellationToken)
                                                .ToListAsync(TestContext.CancellationToken);

        // sync
        var syncResults = fileQueryEngine.Execute(new(env.Root, options)).ToList();

        CollectionAssert.AreEquivalent(syncResults, asyncResults);
    }

    public TestContext TestContext { get; set; } = null!;
}
