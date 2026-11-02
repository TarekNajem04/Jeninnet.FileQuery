//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.MultipleRoots;

/// <summary>
/// Tests for executing several roots as one ordered batch.
/// </summary>
[TestClass]
public sealed class FileQueryBatchTests {
    /// <summary>
    /// Paths are grouped by root in caller-supplied order.
    /// </summary>
    [TestMethod]
    public void Execute_PreservesRootOrder_AndPerRootOrdering() {
        using var first = CreateFirstRoot();
        using var second = CreateSecondRoot();
        var batch = FileQueryBatch.From(
            FileQuery.From(first.Root).UsingGitIgnore().Where(PatternKind.GitIgnore, ["**", "!**/*.txt"]),
            FileQuery.From(second.Root).UsingGitIgnore().Where(PatternKind.GitIgnore, ["**", "!**/*.txt"]));

        var paths = batch.Execute(cancellationToken: TestContext.CancellationToken).ToArray();

        var firstPaths = paths.TakeWhile(path => path.StartsWith(first.Root, StringComparison.Ordinal)).ToArray();
        var secondPaths = paths.Skip(firstPaths.Length).ToArray();

        Assert.IsNotEmpty(firstPaths);
        Assert.IsNotEmpty(secondPaths);
        Assert.IsTrue(secondPaths.All(path => path.StartsWith(second.Root, StringComparison.Ordinal)));
        Assert.AreSequenceEqual(FileQueryRuntime.Create().Execute(batchQuery(first), TestContext.CancellationToken), firstPaths);
        Assert.AreSequenceEqual(FileQueryRuntime.Create().Execute(batchQuery(second), TestContext.CancellationToken), secondPaths);

        static FileQuery batchQuery(TestEnvironment environment) =>
            FileQuery.From(environment.Root)
                .UsingGitIgnore()
                .Where(PatternKind.GitIgnore, ["**", "!**/*.txt"])
                .Build();
    }

    /// <summary>
    /// Entry results expose the producing root, making identical relative paths unambiguous.
    /// </summary>
    [TestMethod]
    public void ExecuteEntries_ExposesRootPath() {
        using var first = CreateFirstRoot();
        using var second = CreateSecondRoot();
        var batch = FileQueryBatch.From(
            FileQuery.From(first.Root).UsingGitIgnore().Where(PatternKind.GitIgnore, ["**", "!**/*.txt"]),
            FileQuery.From(second.Root).UsingGitIgnore().Where(PatternKind.GitIgnore, ["**", "!**/*.txt"]));

        var entries = batch.ExecuteEntries(cancellationToken: TestContext.CancellationToken).Where(static entry => entry.RelativePath == "a.txt").ToArray();

        Assert.HasCount(2, entries);
        Assert.AreSequenceEqual(
            [first.Root, second.Root], entries.Select(static entry => entry.RootPath), Microsoft.VisualStudio.TestTools.UnitTesting.SequenceOrder.InAnyOrder);
        Assert.AreEqual(Path.Combine(first.Root, "a.txt"), entries.Single(entry => entry.RootPath == first.Root).FullPath);
        Assert.AreEqual(Path.Combine(second.Root, "a.txt"), entries.Single(entry => entry.RootPath == second.Root).FullPath);
    }

    /// <summary>
    /// The asynchronous batch returns the same entries as the synchronous batch.
    /// </summary>
    [TestMethod]
    public async Task ExecuteEntriesAsync_MatchesSyncResultsAsync() {
        using var first = CreateFirstRoot();
        using var second = CreateSecondRoot();
        var batch = FileQueryBatch.From(
            FileQuery.From(first.Root).UsingGitIgnore().Where(PatternKind.GitIgnore, ["**", "!**/*.txt"]),
            FileQuery.From(second.Root).UsingGitIgnore().Where(PatternKind.GitIgnore, ["**", "!**/*.txt"]));

        var sync = batch.ExecuteEntries(cancellationToken: TestContext.CancellationToken)
            .Select(static entry => $"{entry.RootPath}|{entry.RelativePath}")
            .ToArray();
        var asyncResults = new List<string>();

        await foreach(var entry in batch.ExecuteEntriesAsync(TestContext.CancellationToken)) {
            asyncResults.Add($"{entry.RootPath}|{entry.RelativePath}");
        }

        Assert.AreSequenceEqual(sync, asyncResults);
    }

    /// <summary>
    /// Each root keeps its own pattern configuration.
    /// </summary>
    [TestMethod]
    public void Execute_UsesEachQueryConfiguration() {
        using var first = CreateFirstRoot();
        using var second = CreateSecondRoot();
        var batch = FileQueryBatch.From(
            FileQuery.From(first.Root).UsingGitIgnore().Where(PatternKind.GitIgnore, ["**", "!**/*.txt"]),
            FileQuery.From(second.Root).UsingGitIgnore().Where(PatternKind.GitIgnore, ["**", "!**/*.log"]));

        var paths = batch.Execute(cancellationToken: TestContext.CancellationToken).ToArray();

        Assert.Contains(path => path.StartsWith(first.Root, StringComparison.Ordinal) && path.EndsWith(".txt", StringComparison.Ordinal), paths);
        Assert.DoesNotContain(path => path.StartsWith(first.Root, StringComparison.Ordinal) && path.EndsWith(".log", StringComparison.Ordinal), paths);
        Assert.Contains(path => path.StartsWith(second.Root, StringComparison.Ordinal) && path.EndsWith(".log", StringComparison.Ordinal), paths);
    }

    /// <summary>
    /// A pre-cancelled token stops the batch before the first root.
    /// </summary>
    [TestMethod]
    public void Execute_ObservesCancellation() {
        using var first = CreateFirstRoot();
        using var second = CreateSecondRoot();
        var batch = FileQueryBatch.From(FileQuery.From(first.Root), FileQuery.From(second.Root));
        using var cancellation = new CancellationTokenSource();

        cancellation.Cancel();

        Assert.ThrowsExactly<OperationCanceledException>(() => batch.Execute(cancellationToken: cancellation.Token).ToList());
    }

    /// <summary>
    /// Empty batches produce no results.
    /// </summary>
    [TestMethod]
    public void Execute_EmptyBatch_ReturnsEmpty() {
        var batch = FileQueryBatch.Create([]);

        Assert.IsEmpty(batch.Roots);
        Assert.IsEmpty(batch.Execute(cancellationToken: TestContext.CancellationToken));
        Assert.IsEmpty(batch.ExecuteEntries(cancellationToken: TestContext.CancellationToken));
    }

    private static TestEnvironment CreateFirstRoot() {
        var environment = new TestEnvironment();
        environment.CreateFiles("a.txt", "sub/b.txt", "ignored.log");
        return environment;
    }

    private static TestEnvironment CreateSecondRoot() {
        var environment = new TestEnvironment();
        environment.CreateFiles("a.txt", "c.log", "nested/d.txt");
        return environment;
    }

    /// <summary>
    /// Gets or sets the test context, which provides information about the current test run.
    /// Used here primarily for accessing the <see cref="CancellationToken"/>.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;
}
