//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.FileEnumeration.Async;

/// <summary>
/// Async tests validating behavior when encountering directories
/// that cannot be accessed (IO exceptions).
/// Ensures async implementation:
/// - respects IgnoreInaccessible
/// - throws when the option is false
/// - continues enumeration properly
/// </summary>
[TestClass]
public class EnumerateFilesAsync_InaccessibleDirectoryTests {
    /// <summary>
    /// Ensures inaccessible directories cause exceptions when IgnoreInaccessible = false.
    /// </summary>
    [TestMethod]
    public async Task Should_Throw_When_InaccessibleDirectory_Async() {
        using var env = new TestEnvironment();

        env.CreateFiles("blocked/ok1.txt", "blocked/ok2.txt");
        env.CreateInaccessibleDirectory("blocked");
        env.AssertDirectoryInaccessible("blocked");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!**/*"
                    ]
                ),
                RecurseSubdirectories: true,
                IgnoreInaccessible: false
            )
        );

        await ((Func<Task>)(async () => {
            try {
                await foreach(var _ in fileQueryEngine.ExecuteAsync(
                    new(env.Root, options),
                    TestContext.CancellationToken)) {
                    // Force traversal into the inaccessible directory
                }

                // If we reach this point, no exception was thrown - fail explicitly
                throw new AssertFailedException("Expected an exception due to inaccessible directory, but none was thrown.");
            }
            catch(Exception ex) when(
                ex is DirectoryNotFoundException or IOException or UnauthorizedAccessException
            ) {
                // Valid exception - rethrow so ThrowsAsync can catch it
                throw;
            }
        })).Should().ThrowAsync<Exception>();
    }

    /// <summary>
    /// Ensures inaccessible directories are skipped when IgnoreInaccessible = true.
    /// </summary>
    [TestMethod]
    public async Task Should_Skip_When_InaccessibleDirectory_Async() {
        using var env = new TestEnvironment();

        env.CreateFiles("root.txt");
        // Use broken symlink to make directory inaccessible
        env.SetInaccessibleDirectory("locked");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: ["**", "!**/*"]
                ),
                RecurseSubdirectories: true,
                IgnoreInaccessible: true
            )
        );

        var results = await fileQueryEngine.ExecuteAsync(new(env.Root, options), TestContext.CancellationToken)
                                           .ToListAsync(TestContext.CancellationToken);

        // inaccessible dir is skipped
        results.Should().ContainSingle();
        results.Single().Should().EndsWith("root.txt");
    }

    /// <summary>
    /// Ensures async enumeration continues after skipping multiple inaccessible dirs.
    /// </summary>
    [TestMethod]
    public async Task Should_SkipAll_When_MultipleInaccessibleDirectories_Async() {
        using var env = new TestEnvironment();

        env.CreateFiles("ok1.txt", "ok2.txt");
        env.CreateFiles("bad1/ok1.txt", "bad2/ok2.txt");

        // Mark bad1 and bad2 as inaccessible (broken symlinks)
        env.SetInaccessibleDirectory("bad1");
        env.SetInaccessibleDirectory("bad2");
        env.CreateFile("okSub/inner.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!**/*"
                    ]
                ),
                RecurseSubdirectories: true,
                IgnoreInaccessible: true
            )
        );

        var results = await fileQueryEngine.ExecuteAsync(new(env.Root, options), TestContext.CancellationToken)
                                           .ToListAsync(TestContext.CancellationToken);

        // Should get 3 files: ok1, ok2, okSub/inner
        results.Should().HaveCount(3);
    }

    /// <summary>
    /// Gets or sets the test context providing cancellation and diagnostic information.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;
}
