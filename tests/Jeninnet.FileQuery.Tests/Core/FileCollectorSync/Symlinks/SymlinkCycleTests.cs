//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Core.FileCollectorSync.Symlinks;

/// <summary>
/// Contains tests for cycle detection when following symbolic links.
/// </summary>
[TestClass]
public class SymlinkCycleTests {
    /// <summary>Tests ShouldDetectCycle_AndStop_WhenFollowWithCycleDetectionEnabled.</summary>
    [TestMethod]
    public void ShouldDetectCycle_AndStop_WhenFollowWithCycleDetectionEnabled() {
        if(!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            // Test uses Windows path patterns for simplicity, but cycle detection is cross-platform.
        }

        using var env = new TestEnvironment();

        // Create a directory 'a'
        var dirA = env.CreateDirectory("a");
        env.CreateFile("a/file1.txt");

        // Create a symlink 'a/link_to_a' pointing back to 'a'
        var linkPath = Path.Combine(dirA, "link_to_a");
        Directory.CreateSymbolicLink(linkPath, dirA);

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(Patterns: ["**", "!**/*.txt"]),
                RecurseSubdirectories: true,
                IgnoreInaccessible: true,
                Traversal: new TraversalOptions(
                    SymlinkPolicy: SymlinkPolicy.FollowWithCycleDetection
                )
            )
        );

        var results = fileQueryEngine.Execute(new(env.Root, options)).ToList();

        // Should find only one 'file1.txt' (at 'a/file1.txt')
        // If cycle detection fails, this will hang or throw StackOverflow (if recursive)
        // or just keep yielding 'a/link_to_a/link_to_a/...' if iterative but not detected.
        Assert.HasCount(1, results);
        Assert.EndsWith("file1.txt", results[0]);
    }

    /// <summary>Tests method.</summary>
    [TestMethod]
    public async Task ShouldDetectCycle_AndStop_WhenFollowWithCycleDetectionEnabledAsync() {
        using var env = new TestEnvironment();

        var dirA = env.CreateDirectory("a");
        env.CreateFile("a/file1.txt");

        var linkPath = Path.Combine(dirA, "link_to_a");
        Directory.CreateSymbolicLink(linkPath, dirA);

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(Patterns: ["**", "!**/*.txt"]),
                RecurseSubdirectories: true,
                IgnoreInaccessible: true,
                Traversal: new TraversalOptions(
                    SymlinkPolicy: SymlinkPolicy.FollowWithCycleDetection
                )
            )
        );

        var results = await fileQueryEngine.ExecuteAsync(new(env.Root, options), TestContext.CancellationToken).ToListAsync(TestContext.CancellationToken);

        Assert.HasCount(1, results);
        Assert.EndsWith("file1.txt", results[0]);
    }

    /// <summary>
    /// Gets or sets the test context for the current test.
    /// </summary>
    /// <value>The test context.</value>
    public TestContext TestContext { get; set; } = null!;
}
