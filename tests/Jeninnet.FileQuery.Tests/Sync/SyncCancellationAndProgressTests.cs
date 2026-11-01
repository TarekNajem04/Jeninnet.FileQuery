//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Sync;

/// <summary>
/// Tests for synchronous cancellation and progress reporting.
/// </summary>
[TestClass]
public sealed class SyncCancellationAndProgressTests {
    /// <summary>
    /// A pre-cancelled token stops the synchronous enumeration.
    /// </summary>
    [TestMethod]
    public void Execute_Throws_WhenTokenIsAlreadyCancelled() {
        using var environment = CreateEnvironment();
        using var cancellation = new CancellationTokenSource();

        cancellation.Cancel();

        var query = FileQuery.From(environment.Root);

        Assert.ThrowsExactly<OperationCanceledException>(() => query.Execute(cancellation.Token).ToList());
    }

    /// <summary>
    /// Cancellation requested during enumeration stops the synchronous path promptly.
    /// </summary>
    [TestMethod]
    public void Execute_ObservesCancellation_DuringEnumeration() {
        using var environment = CreateEnvironment();
        using var cancellation = new CancellationTokenSource();
        var visited = 0;

        Assert.ThrowsExactly<OperationCanceledException>(() => {
            foreach(var _ in FileQuery.From(environment.Root).Execute(cancellation.Token)) {
                visited++;
                cancellation.Cancel();
            }
        });

        Assert.AreEqual(1, visited);
    }

    /// <summary>
    /// Synchronous progress snapshots use the same shape and final counts as the async path.
    /// </summary>
    [TestMethod]
    public async Task Execute_ReportsProgress_SameShapeAsAsync() {
        using var environment = CreateEnvironment();
        var query = FileQuery.From(environment.Root);
        var syncProgress = new RecordingProgress<FileQueryProgress>();
        var asyncProgress = new RecordingProgress<FileQueryProgress>();

        var syncResults = query.Execute(syncProgress).ToList();
        var asyncResults = new List<string>();

        await foreach(var path in query.ExecuteAsync(asyncProgress)) {
            asyncResults.Add(path);
        }

        Assert.AreEqual(syncResults.Count, asyncResults.Count);
        Assert.IsNotEmpty(syncProgress.Values);
        Assert.IsNotEmpty(asyncProgress.Values);

        var syncLast = syncProgress.Values[^1];
        var asyncLast = asyncProgress.Values[^1];

        Assert.AreEqual(asyncLast.DirectoriesVisited, syncLast.DirectoriesVisited);
        Assert.AreEqual(asyncLast.EntriesScanned, syncLast.EntriesScanned);
        Assert.AreEqual(asyncLast.FilesMatched, syncLast.FilesMatched);
    }

    /// <summary>
    /// Synchronous entry enumeration reports progress and observes cancellation.
    /// </summary>
    [TestMethod]
    public void ExecuteEntries_ReportsProgress_AndObservesCancellation() {
        using var environment = CreateEnvironment();
        var query = FileQuery.From(environment.Root);
        var progress = new RecordingProgress<FileQueryProgress>();

        var entries = query.ExecuteEntries(progress).ToList();

        Assert.IsNotEmpty(entries);
        Assert.IsNotEmpty(progress.Values);
        Assert.AreEqual(entries.Count, progress.Values[^1].FilesMatched);

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        Assert.ThrowsExactly<OperationCanceledException>(() => query.ExecuteEntries(cancellation.Token).ToList());
    }

    /// <summary>
    /// A null progress sink is rejected by the builder overloads.
    /// </summary>
    [TestMethod]
    public void Execute_Throws_WhenProgressIsNull() {
        using var environment = CreateEnvironment();
        var query = FileQuery.From(environment.Root);

        Assert.ThrowsExactly<ArgumentNullException>(() => query.Execute(progress: null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => query.ExecuteEntries(progress: null!));
    }

    /// <summary>
    /// Creates a tree with enough entries to observe mid-enumeration cancellation.
    /// </summary>
    private static TestEnvironment CreateEnvironment() {
        var environment = new TestEnvironment();
        environment.CreateFiles(
            "a.txt",
            "b.txt",
            "c.txt",
            "d.txt",
            "e.txt",
            "sub/f.txt",
            "sub/g.txt",
            "sub/deep/h.txt");

        return environment;
    }

    /// <summary>
    /// Collects progress snapshots synchronously.
    /// </summary>
    /// <typeparam name="T">The snapshot type.</typeparam>
    private sealed class RecordingProgress<T> : IProgress<T> {
        public List<T> Values { get; } = [];

        public void Report(T value) => Values.Add(value);
    }
}
