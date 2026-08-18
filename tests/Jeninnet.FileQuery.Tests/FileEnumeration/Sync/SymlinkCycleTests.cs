namespace Jeninnet.FileQuery.Tests.FileEnumeration.Sync;

/// <summary>
/// Tests symlink cycle detection behavior to ensure traversal stops when a cycle is detected.
/// </summary>
[TestClass]
public class SymlinkCycleTests {
    /// <summary>
    /// Verifies that symlink cycles are detected and traversal stops when FollowWithCycleDetection is enabled.
    /// </summary>
    [TestMethod]
    public void Should_DetectCycleAndStop_When_FollowWithCycleDetectionEnabled() {
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

    /// <summary>
    /// Verifies that symlink cycles are detected and traversal stops asynchronously
    /// when FollowWithCycleDetection is enabled.
    /// </summary>
    [TestMethod]
    public async Task Should_DetectCycleAndStop_When_FollowWithCycleDetectionEnabledAsync() {
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
    public TestContext TestContext { get; set; } = null!;
}
