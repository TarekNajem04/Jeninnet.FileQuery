namespace Jeninnet.FileQuery.Tests.Core.FileCollectorAsync.ConcurrencyIsolation;

/// <summary>
/// Ensures async enumeration instances do not interfere with each other
/// when executed concurrently.
/// </summary>
[TestClass]
public class EnumerateFilesAsync_ConcurrencyIsolationTests {
    /// <summary>
    /// Multiple asynchronous enumerations running in parallel
    /// must NOT share internal state.
    /// </summary>
    [TestMethod]
    public async Task EnumerateFilesAsync_MultiInstanceParallel_ShouldNotInterfereAsync() {
        using var env = new TestEnvironment();

        env.CreateFiles("x1.txt", "x2.txt");
        env.CreateFile("sub/y1.txt");
        env.CreateFile("sub/deep/z1.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",        // Exclude everything first
                        "!**/*.txt"  // Then include all .txt files
                    ]
                ),
                RecurseSubdirectories: true
            )
        );

        // Will capture results from 3 parallel runs
        var bag = new ConcurrentBag<List<string>>();

        async Task RunOneAsync() {
            var items = new List<string>();
            await foreach(var path in fileQueryEngine.ExecuteAsync(new(env.Root, options), TestContext.CancellationToken)) {
                items.Add(path);
            }

            bag.Add(items);
        }

        // run all in parallel
        var tasks = Enumerable.Range(0, 3)
                              .Select(_ => Task.Run(RunOneAsync, TestContext.CancellationToken))
                              .ToArray();

        await Task.WhenAll(tasks);

        // All three result sets must contain the *same 4 files*
        foreach(var result in bag) {
            TestAssertEx.HasCount(result, 4);
            Assert.Contains(x => x.EndsWith("x1.txt", StringComparison.Ordinal), result);
            Assert.Contains(x => x.EndsWith("x2.txt", StringComparison.Ordinal), result);
            Assert.Contains(x => x.EndsWith(Path.Combine("sub", "deep", "z1.txt"), StringComparison.Ordinal), result);
        }
    }

    /// <summary>
    /// Ensures no async state is leaked between instances with different patterns.
    /// </summary>
    [TestMethod]
    public async Task EnumerateFilesAsync_DifferentPatterns_ShouldRemainIsolatedAsync() {
        using var env = new TestEnvironment();

        env.CreateFiles("a.txt", "b.log", "c.md");

        var fileQueryEngine = FileQueryRuntime.Create();

        var txtOptions = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",       // exclude everything
                        "!*.txt"    // include only .txt files
                    ]
                )
            )
        );

        var logOptions = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",       // exclude everything
                        "!*.log"    // include only .log files
                    ]
                )
            )
        );

        var mdOptions = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",       // exclude everything
                        "!*.md"     // include only .md files
                    ]
                )
            )
        );

        var txtTask = CollectAsync(txtOptions);
        var logTask = CollectAsync(logOptions);
        var mdTask = CollectAsync(mdOptions);

        var (txt, log, md) = (await txtTask, await logTask, await mdTask);

        TestAssertEx.ContainsSingle(txt, x => x.EndsWith("a.txt", StringComparison.Ordinal));
        TestAssertEx.ContainsSingle(log, x => x.EndsWith("b.log", StringComparison.Ordinal));
        TestAssertEx.ContainsSingle(md, x => x.EndsWith("c.md", StringComparison.Ordinal));
        // Local function for clarity
        async Task<List<string>> CollectAsync(FileQueryOptions options) =>
            await fileQueryEngine.ExecuteAsync(new(env.Root, options), TestContext.CancellationToken)
                                 .ToListAsync(TestContext.CancellationToken);
    }

    /// <summary>
    /// Gets or sets the test context.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;
}
