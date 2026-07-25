namespace Jeninnet.FileQuery.Tests.Core.FileCollectorAsync.DirectoryOnly;

/// <summary>
/// Async tests for the recursive wildcard **.
/// Ensures:
///  - ** matches zero or more directories
///  - ** + literal match
///  - ** inside complex rule sets
/// </summary>
[TestClass]
public class EnumerateFilesAsync_RecursiveWildcardTests {
    /// <summary>
    /// Tests that ** matches files at any depth.
    /// </summary>
    [TestMethod]
    public async Task EnumerateFilesAsync_DoubleStar_ShouldMatchAnyDepthAsync() {
        using var env = new TestEnvironment();

        env.CreateFiles(
            "a.txt",
            "sub/b.txt",
            "sub/deeper/c.txt"
        );

        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",       // exclude all files in root
                        "!**/*.txt" // Match any .txt file at any depth
                    ]
                ),
                RecurseSubdirectories: true
            )
        );

        var fileQueryEngine = FileQueryRuntime.Create();
        var results = await fileQueryEngine.ExecuteAsync(new(env.Root, options), TestContext.CancellationToken)
                                           .ToListAsync(TestContext.CancellationToken);

        TestAssertEx.HasCount(results, 3);
    }

    /// <summary>
    /// Tests that ** can appear in the middle of a pattern.
    /// </summary>
    [TestMethod]
    public async Task EnumerateFilesAsync_DoubleStarInMiddle_ShouldWorkAsync() {
        using var env = new TestEnvironment();

        env.CreateFiles(
            "x/y/z/file.txt",
            "x/other/z/file.txt",
            "x/nomatch.txt"
        );

        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",           // exclude all files
                        "!x/**/z/*.txt" // include .txt files in any 'z' subdirectory under 'x'
                    ]
                )
            )
        );

        var fileQueryEngine = FileQueryRuntime.Create();
        var results = await fileQueryEngine.ExecuteAsync(new(env.Root, options), TestContext.CancellationToken)
                                           .ToListAsync(TestContext.CancellationToken);

        TestAssertEx.HasCount(results, 2);
    }

    public TestContext TestContext { get; set; } = null!;
}

