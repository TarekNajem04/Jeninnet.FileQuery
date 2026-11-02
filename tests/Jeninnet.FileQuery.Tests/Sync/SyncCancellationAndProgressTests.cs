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

        FluentActions.Invoking(() => { _ = query.Execute(cancellation.Token).ToList(); }).Should().ThrowExactly<OperationCanceledException>();
    }

    /// <summary>
    /// Cancellation requested during enumeration stops the synchronous path promptly.
    /// </summary>
    [TestMethod]
    public void Execute_ObservesCancellation_DuringEnumeration() {
        using var environment = CreateEnvironment();
        using var cancellation = new CancellationTokenSource();
        var visited = 0;

        FluentActions.Invoking(() => {
            foreach(var _ in FileQuery.From(environment.Root).Execute(cancellation.Token)) {
                visited++;
                cancellation.Cancel();
            }
        }).Should().ThrowExactly<OperationCanceledException>();

        visited.Should().Be(1);
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

        var syncResults = query.Execute(syncProgress, TestContext.CancellationToken).ToList();
        var asyncResults = new List<string>();

        await foreach(var path in query.ExecuteAsync(asyncProgress, TestContext.CancellationToken)) {
            asyncResults.Add(path);
        }

        asyncResults.Should().HaveCount(syncResults.Count);
        syncProgress.Values.Should().NotBeEmpty();
        asyncProgress.Values.Should().NotBeEmpty();

        var syncLast = syncProgress.Values[^1];
        var asyncLast = asyncProgress.Values[^1];

        syncLast.DirectoriesVisited.Should().Be(asyncLast.DirectoriesVisited);
        syncLast.EntriesScanned.Should().Be(asyncLast.EntriesScanned);
        syncLast.FilesMatched.Should().Be(asyncLast.FilesMatched);
    }

    /// <summary>
    /// Synchronous entry enumeration reports progress and observes cancellation.
    /// </summary>
    [TestMethod]
    public void ExecuteEntries_ReportsProgress_AndObservesCancellation() {
        using var environment = CreateEnvironment();
        var query = FileQuery.From(environment.Root);
        var progress = new RecordingProgress<FileQueryProgress>();

        var entries = query.ExecuteEntries(progress, TestContext.CancellationToken).ToList();

        entries.Should().NotBeEmpty();
        progress.Values.Should().NotBeEmpty();
        progress.Values[^1].FilesMatched.Should().Be(entries.Count);

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        FluentActions.Invoking(() => { _ = query.ExecuteEntries(cancellation.Token).ToList(); }).Should().ThrowExactly<OperationCanceledException>();
    }

    /// <summary>
    /// A null progress sink is rejected by the builder overloads.
    /// </summary>
    [TestMethod]
    public void Execute_Throws_WhenProgressIsNull() {
        using var environment = CreateEnvironment();
        var query = FileQuery.From(environment.Root);

        FluentActions.Invoking(() => query.Execute(progress: null!, TestContext.CancellationToken)).Should().ThrowExactly<ArgumentNullException>();
        FluentActions.Invoking(() => query.ExecuteEntries(progress: null!, TestContext.CancellationToken)).Should().ThrowExactly<ArgumentNullException>();
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

    /// <summary>
    /// Gets or sets the test context, which provides information about the current test run.
    /// Used here primarily for accessing the <see cref="CancellationToken"/>.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;
}
