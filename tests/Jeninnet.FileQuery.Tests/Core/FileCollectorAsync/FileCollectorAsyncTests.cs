//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Core.FileCollectorAsync;

/// <summary>
/// Async file enumeration tests for <see cref="FileQueryRuntime"/>.
/// Validates recursive enumeration, pattern matching, and cancellation.
/// </summary>
[TestClass]
public class FileQueryEngineAsyncTests {
    // Remove _tempDir field, Setup, and Cleanup methods.

    /// <summary>
    /// Gets or sets the test context, which provides information about the current test run.
    /// Used here primarily for accessing the <see cref="CancellationToken"/>.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;

    // --- Test Methods ---

    /// <summary>
    /// Ensures async enumeration respects a maximum recursion depth.
    /// Only top-level files should be returned when max depth is 0.
    /// </summary>
    [TestMethod]
    public async Task EnumerateFilesAsync_RespectsMaxDepthAsync() {
        // ARRANGE: Setup isolated environment using TestEnvironment
        using var env = new TestEnvironment();
        env.CreateFiles(
            "file1.txt",
            "file2.log",
            "file3.txt"
        );
        env.CreateFile("subdir/file3.txt");
        env.CreateFile("bin/file3.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",       //Exclude everything
                        "*.log",    // Exclude log files
                        "!*.txt"    // Include txt files
                    ]
                ),
                RecurseSubdirectories: true, // Only top-level files
                MaxRecursionDepth: 0
            )
        );

        // ACT
        var results = await fileQueryEngine.ExecuteAsync(new(env.Root, options), TestContext.CancellationToken)
                                           .ToListAsync(TestContext.CancellationToken);

        // ASSERT
        // Both file1.txt and file3.txt are at the root and match "*.txt". Expected count is 2.
        Assert.HasCount(2, results, "Should only return top-level matching files (file1.txt and file3.txt).");
        Assert.AreSequenceEqual(
            [
                env.Abs("file1.txt"),
                env.Abs("file3.txt")
            ],
            results,
            SequenceOrder.InAnyOrder
        );
        // env.Dispose() is called automatically at the end of the using block.
    }

    /// <summary>
    /// Ensures async enumeration returns all files matching a given pattern.
    /// Tests recursive matching with a wildcard.
    /// </summary>
    [TestMethod]
    public async Task EnumerateFilesAsync_ShouldReturnMatchingFilesAsync() {
        // ARRANGE: Setup isolated environment
        using var env = new TestEnvironment();
        env.CreateFiles(
            "file1.txt",
            "file2.log",
            "file3.txt"
        );
        env.CreateFile("subdir/file3.txt");
        env.CreateFile("bin/file3.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",       //Exclude everything
                        "*.log" ,   // Exclude log files
                        "!**/*.txt" // [Recursive wildcard] Include all .txt files recursively
                    ]
                ),
                RecurseSubdirectories: true
            )
        );

        // ACT
        var results = await fileQueryEngine.ExecuteAsync(new(env.Root, options), TestContext.CancellationToken)
                                           .ToListAsync(TestContext.CancellationToken);

        // ASSERT
        // The pattern "**/*.txt" matches all four .txt files in the hierarchy. Expected count is 4.
        Assert.AreSequenceEqual(
            [
                env.Abs("file1.txt"),
                env.Abs("file3.txt"),
                env.Abs("subdir", "file3.txt"),
                env.Abs("bin", "file3.txt")
            ],
            results,
            SequenceOrder.InAnyOrder
        );
    }

    /// <summary>
    /// Verifies that cancellation token correctly interrupts async enumeration.
    /// </summary>
    [TestMethod]
    public async Task EnumerateFilesAsync_CanBeCancelledAsync() {
        // ARRANGE: Setup isolated environment
        using var env = new TestEnvironment();
        env.CreateFiles(
            "file1.txt",
            "file2.log",
            "file3.txt"
        );
        env.CreateFile("subdir/file3.txt");
        env.CreateFile("bin/file3.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**/*"
                    ]
                ),
                RecurseSubdirectories: true
            )
        );

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // ACT & ASSERT
        // Cancellation should throw OperationCanceledException
        await Assert.ThrowsAsync<OperationCanceledException>(async () => {
            await foreach(var file in fileQueryEngine.ExecuteAsync(new(env.Root, options), cts.Token)) {
                // Should never reach here
            }
        });
    }

    /// <summary>
    /// Tests complex patterns including negation, character classes, and directory-only rules.
    /// Uses standard GitIgnore pruning semantics (un-negated directory-only rules exclude the subtree).
    /// </summary>
    [TestMethod]
    public async Task EnumerateFilesAsync_HandlesComplexPatternsAsync() {
        // ARRANGE: Setup isolated environment
        using var env = new TestEnvironment();
        env.CreateFiles(
            "file1.txt",
            "file2.log",
            "file3.txt"
        );
        env.CreateFile("subdir/file3.txt");
        env.CreateFile("bin/file3.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "*.txt",            // exclude txt files
                        "*.log",            // exclude log files
                        "!file1.txt",       // include file1.txt
                        "![fF]ile3.txt",    // include file3.txt everywhere
                        "subdir/**"         // directory-only rule (prunes/excludes subdir/ and its contents)
                    ]
                ),
                RecurseSubdirectories: true,
                CaseSensitivity: CaseSensitivity.Insensitive
            )
        );

        // ACT
        var results = await fileQueryEngine.ExecuteAsync(new(env.Root, options), TestContext.CancellationToken)
                                           .ToListAsync(TestContext.CancellationToken);

        // ASSERT
        Assert.AreSequenceEqual(
            [
                // 1. file1.txt is included by !file1.txt
                env.Abs("file1.txt"),
                // 2. file3.txt is included by ![fF]ile3.txt (at root)
                env.Abs("file3.txt"),
                // 3. bin/file3.txt is included by ![fF]ile3.txt (recursively) and is NOT pruned
                env.Abs("bin","file3.txt") // This file MUST be included
            ],
            results,
            SequenceOrder.InAnyOrder
        );
        Assert.HasCount(3, results, "Should return file1.txt, file3.txt (root) and bin/file3.txt after exclusions/pruning.");
    }
}
