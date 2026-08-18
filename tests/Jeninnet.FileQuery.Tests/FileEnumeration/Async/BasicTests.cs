//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.FileEnumeration.Async;

/// <summary>
/// Basic async enumeration tests:
/// - correct async iteration
/// - correct results
/// - matching sync behavior
/// </summary>
[TestClass]
public class EnumerateFilesAsync_BasicTests {
    /// <summary>
    /// Ensures basic async enumeration returns expected .txt files.
    /// </summary>
    [TestMethod]
    public async Task Should_ReturnTxtFiles_Async() {
        using var env = new TestEnvironment();

        env.CreateFiles("a.txt", "b.txt", "c.log", "sub/d.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "*.log",    // exclude .log files
                        "!**/*.txt" // include .txt files from any directory recursively
                    ]
                ),
                RecurseSubdirectories: true
            )
        );

        var results = await fileQueryEngine.ExecuteAsync(new(env.Root, options), TestContext.CancellationToken)
                                           .ToListAsync(TestContext.CancellationToken);

        results.Should().HaveCount(3);
        results.Should().Contain(static x => x.EndsWith("a.txt", StringComparison.Ordinal));
        results.Should().Contain(static x => x.EndsWith("b.txt", StringComparison.Ordinal));
        results.Should().Contain(static x => x.EndsWith(Path.Combine("sub", "d.txt"), StringComparison.Ordinal));
    }

    /// <summary>
    /// Ensures async enumeration matches sync enumeration for the same options.
    /// </summary>
    [TestMethod]
    public async Task Should_MatchSyncResults_Async() {
        using var env = new TestEnvironment();
        env.CreateFiles("x.txt", "y.log", "sub/z.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "!**/*.txt"
                    ]
                ),
                RecurseSubdirectories: true
            )
        );

        // sync results
        var syncResults = fileQueryEngine.Execute(new(env.Root, options)).Order().ToList();

        // async results
        var asyncResults = await fileQueryEngine.ExecuteAsync(new(env.Root, options), TestContext.CancellationToken)
                                                .ToListAsync(TestContext.CancellationToken);

        Assert.AreSequenceEqual(syncResults, [.. asyncResults.Order()]);
    }

    /// <summary>
    /// Ensures async enumeration works fine on empty directory.
    /// </summary>
    [TestMethod]
    public async Task Should_ReturnEmpty_When_DirectoryIsEmpty_Async() {
        using var env = new TestEnvironment();
        env.CreateDirectory("empty");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "!**/*.txt"
                    ]
                ),
                RecurseSubdirectories: true
            )
        );

        var dir = Path.Combine(env.Root, "empty");

        var results = await fileQueryEngine.ExecuteAsync(new(dir, options), TestContext.CancellationToken)
                                           .ToListAsync(TestContext.CancellationToken);

        results.Should().BeEmpty();
    }

    /// <summary>
    /// Gets or sets the test context providing cancellation and diagnostic information.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;
}
