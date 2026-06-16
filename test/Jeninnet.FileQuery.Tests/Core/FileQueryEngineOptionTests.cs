namespace Jeninnet.FileQuery.Tests.Core;

/// <summary>
/// Tests the functional impact of configurable options in <see cref="FileQueryOptions"/>
/// on the async file enumeration process.
/// </summary>
[TestClass]
public class FileQueryEngineOptionTests
{
    // The TestContext is necessary for CancellationToken access and required by MSTest.
    public TestContext TestContext { get; set; } = null!;

    // --- Core Options Tests ---

    /// <summary>
    /// Ensures enumeration stops at the root directory when RecurseSubdirectories is false.
    /// </summary>
    [TestMethod]
    public async Task EnumerateFilesAsync_HonorsRecurseSubdirectoriesFalseAsync()
    {
        // ARRANGE: Setup isolated environment with depth 2
        using var env = new TestEnvironment();
        env.CreateFile("file_root.txt");
        env.CreateDirectory("subdir1");
        env.CreateFile("subdir1/file_1.txt");
        env.CreateDirectory("subdir1/subdir2");
        env.CreateFile("subdir1/subdir2/file_2.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "!**/*.txt" // Include all .txt files
                    ]
                ),
                RecurseSubdirectories: false
            )
        );

        // ACT
        var results = await fileQueryEngine.ExecuteAsync(new(env.Root, options), TestContext.CancellationToken)
                                           .ToListAsync(TestContext.CancellationToken);

        // ASSERT
        CollectionAssert.AreEquivalent(
            new[] {
                env.Abs("file_root.txt")
            },
            results,
            "Only files in the root directory should be returned when recursion is disabled."
        );
    }

    /// <summary>
    /// Ensures enumeration respects a specified maximum recursion depth (MaxRecursionDepth).
    /// Depth 0 = only root files. Depth 1 = root files and files in direct subdirectories.
    /// </summary>
    [TestMethod]
    public async Task EnumerateFilesAsync_HonorsMaxRecursionDepthAsync()
    {
        // ARRANGE: Setup isolated environment with files at depth 0, 1, and 2
        using var env = new TestEnvironment();
        env.CreateFile("file_depth0.txt");
        env.CreateFile("level1/file_depth1.txt");
        env.CreateFile("level1/level2/file_depth2.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "!**/*.txt"
                    ]
                ),
                RecurseSubdirectories: true,
                MaxRecursionDepth: 1 // Max depth 1 means traversal stops AFTER level1
            )
        );

        // ACT
        var results = await fileQueryEngine.ExecuteAsync(new(env.Root, options), TestContext.CancellationToken)
                                           .ToListAsync(TestContext.CancellationToken);

        // ASSERT
        CollectionAssert.AreEquivalent(
            new[] {
                env.Abs("file_depth0.txt"),
                env.Abs("level1","file_depth1.txt")
            },
            results,
            "Should only return files at depth 0 and 1."
        );
    }

    /// <summary>
    /// Ensures pattern matching honors case sensitivity when IgnoreCase is explicitly set to false.
    /// </summary>
    [TestMethod]
    public async Task EnumerateFilesAsync_HonorsIgnoreCaseFalseAsync()
    {
        // ARRANGE: Setup isolated environment with mixed-case files
        using var env = new TestEnvironment();
        env.CreateFile("TeStFiLe.tXt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "!TestFile.txt"
                    ]
                ),
                RecurseSubdirectories: true,
                CaseSensitivity: CaseSensitivity.Insensitive
            )
        );

        // ACT
        var results = await fileQueryEngine.ExecuteAsync(new(env.Root, options), TestContext.CancellationToken)
                                           .ToListAsync(TestContext.CancellationToken);
        var comparisonType = options.CaseSensitivity.GetStringComparison();
        // ASSERT
        TestAssertEx.Contains(results, x => x.EndsWith("testfile.txt", comparisonType), "The file 'testfile.txt' must be included because the test is case-insensitive.");
    }
}

