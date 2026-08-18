//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Core.FileCollectorAsync.RecursionDepth;

/// <summary>
/// Async-focused tests validating recursion depth enforcement.
/// Ensures:
///  - async traversal respects MaxRecursionDepth
///  - async deep enumeration stops correctly
///  - directories at disallowed levels are not visited
/// </summary>
[TestClass]
public class EnumerateFilesAsync_RecursionDepthTests {
    /// <summary>
    /// Ensures async enumeration stops at depth 0 (only root files).
    /// </summary>
    [TestMethod]
    public async Task EnumerateFilesAsync_Depth0_ShouldOnlyReturnRootFilesAsync() {
        using var env = new TestEnvironment();

        env.CreateFiles(
            "root.txt",
            "sub/sub1.txt",
            "sub/sub2/subfile.txt"
        );

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!**/*.txt"
                    ]
                ),
                RecurseSubdirectories: true,    // only root
                MaxRecursionDepth: 0
            )
        );

        var results = await fileQueryEngine.ExecuteAsync(new(env.Root, options), TestContext.CancellationToken)
                                           .ToListAsync(TestContext.CancellationToken);

        TestAssertEx.ContainsSingle(results, static x => x.EndsWith("root.txt", StringComparison.Ordinal));
        TestAssertEx.HasCount(results, 1);
    }

    /// <summary>
    /// Ensures async enumeration stops after 1 level of recursion.
    /// </summary>
    [TestMethod]
    public async Task EnumerateFilesAsync_Depth1_ShouldIncludeSubdirectoriesButNotDeepOnesAsync() {
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
                        "**",
                        "!**/*.txt"
                    ]
                ),
                RecurseSubdirectories: true,
                MaxRecursionDepth: 1
            )
        );

        var fileQueryEngine = FileQueryRuntime.Create();
        var results = await fileQueryEngine.ExecuteAsync(new(env.Root, options), TestContext.CancellationToken)
                                           .ToListAsync(TestContext.CancellationToken);

        TestAssertEx.HasCount(results, 2);
        TestAssertEx.Contains(results, static x => x.EndsWith("a.txt", StringComparison.Ordinal));
        TestAssertEx.Contains(results, static x => x.EndsWith(Path.Combine("sub", "b.txt"), StringComparison.Ordinal));
        Assert.DoesNotContain(static x => x.EndsWith(Path.Combine("deeper", "c.txt"), StringComparison.Ordinal), results);
    }

    /// <summary>
    /// Ensures async enumeration honors depth limit even with many sibling folders.
    /// </summary>
    [TestMethod]
    public async Task EnumerateFilesAsync_ShouldRespectDepthLimitWithWideTreeAsync() {
        using var env = new TestEnvironment();

        // root files
        env.CreateFiles("1.txt", "2.txt");

        // depth 1
        env.CreateFiles("A/a1.txt", "B/b1.txt");

        // depth 2
        env.CreateFiles("A/A2/a2.txt", "B/B2/b2.txt");

        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!**/*.txt"
                    ]
                ),
                RecurseSubdirectories: true,
                MaxRecursionDepth: 1
            )
        );

        var fileQueryEngine = FileQueryRuntime.Create();
        var results = await fileQueryEngine.ExecuteAsync(new(env.Root, options), TestContext.CancellationToken)
                                           .ToListAsync(TestContext.CancellationToken);

        TestAssertEx.HasCount(results, 4); // 2 root + 2 at depth 1
        Assert.DoesNotContain(static x => x.Contains("A2", StringComparison.Ordinal), results);
        Assert.DoesNotContain(static x => x.Contains("B2", StringComparison.Ordinal), results);
    }

    /// <summary>
    /// Gets or sets the test context.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;
}
