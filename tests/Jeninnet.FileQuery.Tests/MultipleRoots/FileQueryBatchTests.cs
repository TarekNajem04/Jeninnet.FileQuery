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

        var paths = batch.Execute().ToArray();

        var firstPaths = paths.TakeWhile(path => path.StartsWith(first.Root, StringComparison.Ordinal)).ToArray();
        var secondPaths = paths.Skip(firstPaths.Length).ToArray();

        Assert.IsNotEmpty(firstPaths);
        Assert.IsNotEmpty(secondPaths);
        Assert.IsTrue(secondPaths.All(path => path.StartsWith(second.Root, StringComparison.Ordinal)));
        CollectionAssert.AreEqual(FileQueryRuntime.Create().Execute(batchQuery(first)).ToArray(), firstPaths);
        CollectionAssert.AreEqual(FileQueryRuntime.Create().Execute(batchQuery(second)).ToArray(), secondPaths);

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

        var entries = batch.ExecuteEntries().Where(static entry => entry.RelativePath == "a.txt").ToArray();

        Assert.AreEqual(2, entries.Length);
        CollectionAssert.AreEquivalent(
            new[] { first.Root, second.Root },
            entries.Select(static entry => entry.RootPath).ToArray());
        Assert.AreEqual(Path.Combine(first.Root, "a.txt"), entries.Single(entry => entry.RootPath == first.Root).FullPath);
        Assert.AreEqual(Path.Combine(second.Root, "a.txt"), entries.Single(entry => entry.RootPath == second.Root).FullPath);
    }

    /// <summary>
    /// The asynchronous batch returns the same entries as the synchronous batch.
    /// </summary>
    [TestMethod]
    public async Task ExecuteEntriesAsync_MatchesSyncResults() {
        using var first = CreateFirstRoot();
        using var second = CreateSecondRoot();
        var batch = FileQueryBatch.From(
            FileQuery.From(first.Root).UsingGitIgnore().Where(PatternKind.GitIgnore, ["**", "!**/*.txt"]),
            FileQuery.From(second.Root).UsingGitIgnore().Where(PatternKind.GitIgnore, ["**", "!**/*.txt"]));

        var sync = batch.ExecuteEntries()
            .Select(static entry => $"{entry.RootPath}|{entry.RelativePath}")
            .ToArray();
        var asyncResults = new List<string>();

        await foreach(var entry in batch.ExecuteEntriesAsync()) {
            asyncResults.Add($"{entry.RootPath}|{entry.RelativePath}");
        }

        CollectionAssert.AreEqual(sync, asyncResults.ToArray());
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

        var paths = batch.Execute().ToArray();

        Assert.IsTrue(paths.Any(path => path.StartsWith(first.Root, StringComparison.Ordinal) && path.EndsWith(".txt", StringComparison.Ordinal)));
        Assert.IsFalse(paths.Any(path => path.StartsWith(first.Root, StringComparison.Ordinal) && path.EndsWith(".log", StringComparison.Ordinal)));
        Assert.IsTrue(paths.Any(path => path.StartsWith(second.Root, StringComparison.Ordinal) && path.EndsWith(".log", StringComparison.Ordinal)));
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

        Assert.AreEqual(0, batch.Roots.Count);
        Assert.AreEqual(0, batch.Execute().Count());
        Assert.AreEqual(0, batch.ExecuteEntries().Count());
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
}
