//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.FileEnumeration.Async;

/// <summary>
/// Stress and edge-case tests designed to detect rare async bugs:
///   - Large number of files
///   - Patterns resembling directories but not ending with '/'
///   - Patterns with tricky ordering
///   - Performance consistency under async enumeration
/// </summary>
[TestClass]
public class EnumerateFilesAsync_StressAndEdgeCaseTests {
    /// <summary>
    /// Stress test: Verify async enumeration works correctly with many files.
    /// </summary>
    [TestMethod]
    public async Task Should_HandleManyFiles_When_StressTest_Async() {
        using var env = new TestEnvironment();

        for(var i = 0; i < 200; i++) {
            env.CreateFile($"group/file{i}.txt");
        }

        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!**/*.txt"
                    ]
                )
            )
        );

        var fileQueryEngine = FileQueryRuntime.Create();
        var results = await fileQueryEngine.ExecuteAsync(new(env.Root, options), TestContext.CancellationToken)
                                           .ToListAsync(TestContext.CancellationToken);

        results.Should().HaveCount(200);
    }

    /// <summary>
    /// When a pattern looks like a directory but is not actually marked as one,
    /// it must be treated as a file pattern.
    /// </summary>
    [TestMethod]
    public async Task Should_NotIgnoreFolder_When_FakeDirectoryPatternUsed_Async() {
        using var env = new TestEnvironment();

        env.CreateFiles(
            "folder/data.txt",
            "folderX/data.txt"
        );

        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "folder" // not "folder/"
                    ]
                )
            )
        );

        var fileQueryEngine = FileQueryRuntime.Create();
        var results = await fileQueryEngine.ExecuteAsync(new(env.Root, options), TestContext.CancellationToken)
                                           .ToListAsync(TestContext.CancellationToken);

        // "folder" alone = match file/directory named "folder" (not its children)
        results.Should().BeEmpty();
    }

    /// <summary>
    /// Ensures async behavior is identical to sync when combining many rules.
    /// </summary>
    [TestMethod]
    public async Task Should_ReturnSameAsSync_When_ComplexRuleSetUsed_Async() {
        using var env = new TestEnvironment();

        env.CreateFiles(
            "a/b/c/file.txt",
            "a/b/file.log",
            "a/c/z/file.txt"
        );

        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!**/*.txt",
                        "a/b/*",       // exclude b folder
                        "!a/b/c/*.txt",  // but re-include nested c inside b
                    ]
                )
            )
        );

        var fileQueryEngine = FileQueryRuntime.Create();

        // async
        var asyncResults = await fileQueryEngine.ExecuteAsync(new(env.Root, options), TestContext.CancellationToken)
                                                .ToListAsync(TestContext.CancellationToken);

        // sync
        var syncResults = fileQueryEngine.Execute(new(env.Root, options)).ToList();

        Assert.AreSequenceEqual(syncResults, asyncResults, SequenceOrder.InAnyOrder);
    }

    /// <summary>
    /// Gets or sets the test context providing cancellation and diagnostic information.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;
}
