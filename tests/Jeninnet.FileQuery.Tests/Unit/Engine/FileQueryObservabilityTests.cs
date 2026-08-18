namespace Jeninnet.FileQuery.Tests.Unit.Engine;

/// <summary>
/// Tests for FileQuery observability features including progress reporting, diagnostics, and error recovery.
/// </summary>
[TestClass]
public sealed class FileQueryObservabilityTests {
    /// <summary>
    /// Verifies that async execution reports traversal statistics through the progress callback.
    /// </summary>
    [TestMethod]
    public async Task ExecuteAsync_WithProgress_ShouldReportTraversalStatisticsAsync() {
        using var env = new TestEnvironment();
        env.CreateFiles("a.txt", "sub/b.txt", "sub/c.log");

        var progress = new RecordingProgress<FileQueryProgress>();
        var engine = FileQueryRuntime.Create();
        var query = new FileQuery(
            env.Root,
            new FileQueryOptions(
                new FileQueryOptionsConfig(
                    PatternInput: new(Patterns: ["**", "!**/*.txt"]),
                    RecurseSubdirectories: true
                )
            )
        );

        var results = await engine.ExecuteAsync(query, progress, TestContext.CancellationToken)
                                  .ToListAsync(TestContext.CancellationToken);

        results.Should().HaveCount(2);
        Assert.IsNotEmpty(progress.Values);
        Assert.IsGreaterThanOrEqualTo(1, progress.Values[^1].DirectoriesVisited);
        Assert.IsGreaterThanOrEqualTo(3, progress.Values[^1].EntriesScanned);
        Assert.AreEqual(2, progress.Values[^1].FilesMatched);
    }

    /// <summary>
    /// Verifies that diagnostics report the responsible pattern and outcome for each matched file.
    /// </summary>
    [TestMethod]
    public void Should_ReportResponsiblePattern_When_DiagnosticsEnabled() {
        using var env = new TestEnvironment();
        env.CreateFiles("keep.txt", "drop.log");

        var diagnostics = new RecordingProgress<FileQueryDiagnostic>();
        var query = new FileQuery(
            env.Root,
            new FileQueryOptions(
                new FileQueryOptionsConfig(
                    PatternInput: new(Patterns: ["**", "!**/*.txt"]),
                    AuditMatches: true,
                    Diagnostics: diagnostics
                )
            )
        );

        var results = FileQueryRuntime.Create().Execute(query).ToList();

        results.Should().HaveCount(1);
        var included = diagnostics.Values.Single(static d => d.RelativePath == "keep.txt");
        var excluded = diagnostics.Values.Single(static d => d.RelativePath == "drop.log");

        Assert.AreEqual("Include", included.Outcome);
        Assert.AreEqual("!**/*.txt", included.Pattern);
        Assert.AreEqual(1, included.PatternIndex);
        Assert.AreEqual("Exclude", excluded.Outcome);
        Assert.AreEqual("**", excluded.Pattern);
        Assert.AreEqual(0, excluded.PatternIndex);
    }

    /// <summary>
    /// Verifies that cancellation tokens are propagated through filesystem enumeration during async execution.
    /// </summary>
    [TestMethod]
    public async Task ExecuteAsync_ShouldPropagateCancellationThroughFilesystemEnumerationAsync() {
        var fileSystem = new CancellationObservingFileSystem();
        var engine = new Jeninnet.FileQuery.Engine.FileQueryEngine(
            new TraversalExecutor(),
            new TraversalPlanBuilder(fileSystem)
        );
        var query = new FileQuery(
            fileSystem.Root,
            new FileQueryOptions(new FileQueryOptionsConfig(PatternInput: new(Patterns: ["!**"])))
        );

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        Func<Task> actAsync = async () => {
            await foreach(var _ in engine.ExecuteAsync(query, cts.Token)) {
                // No-op
            }
        };

        await actAsync.Should().ThrowAsync<OperationCanceledException>();
    }

    /// <summary>
    /// Verifies that a failing directory is skipped when skip recovery is configured.
    /// </summary>
    [TestMethod]
    public void Should_SkipFailingDirectory_When_SkipRecoveryConfigured() {
        var fileSystem = new RecoverableFailureFileSystem(failLockedDirectoryAttempts: 1);
        var engine = CreateEngine(fileSystem);
        var query = CreateRecoveryQuery(fileSystem, FileQueryErrorRecoveryOptions.Skip);

        var results = engine.Execute(query).ToList();

        results.Should().Contain(fileSystem.KeepFile);
        results.Should().NotContain(fileSystem.RetryFile);
    }

    /// <summary>
    /// Verifies that a failing directory propagates an IOException when abort recovery is configured.
    /// </summary>
    [TestMethod]
    public void Should_PropagateFailingDirectory_When_AbortRecoveryConfigured() {
        var fileSystem = new RecoverableFailureFileSystem(failLockedDirectoryAttempts: 1);
        var engine = CreateEngine(fileSystem);
        var query = CreateRecoveryQuery(fileSystem, FileQueryErrorRecoveryOptions.Abort);

        ((Action)(() => {
            var _ = engine.Execute(query).ToList();
        })).Should().Throw<IOException>();
    }

    /// <summary>
    /// Verifies that a failing directory is retried and succeeds when retry recovery is configured.
    /// </summary>
    [TestMethod]
    public void Should_RetryFailingDirectory_When_RetryRecoveryConfigured() {
        var fileSystem = new RecoverableFailureFileSystem(failLockedDirectoryAttempts: 1);
        var engine = CreateEngine(fileSystem);
        var query = CreateRecoveryQuery(fileSystem, FileQueryErrorRecoveryOptions.Retry(maxRetryAttempts: 1));

        var results = engine.Execute(query).ToList();

        results.Should().Contain(fileSystem.KeepFile);
        results.Should().Contain(fileSystem.RetryFile);
    }

    /// <summary>
    /// Gets or sets the MSTest test context for the current test.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;

    private static Jeninnet.FileQuery.Engine.FileQueryEngine CreateEngine(IFileSystem fileSystem) => new(
            new TraversalExecutor(),
            new TraversalPlanBuilder(fileSystem)
        );

    private static FileQuery CreateRecoveryQuery(
        RecoverableFailureFileSystem fileSystem,
        FileQueryErrorRecoveryOptions errorRecovery
    ) => new(
            fileSystem.Root,
            new FileQueryOptions(
                new FileQueryOptionsConfig(
                    PatternInput: new(Patterns: ["!**"]),
                    RecurseSubdirectories: true,
                    IgnoreInaccessible: errorRecovery.Action is FileQueryErrorAction.Skip,
                    ErrorRecovery: errorRecovery
                )
            )
        );

    private sealed class RecordingProgress<T> : IProgress<T> {
        public List<T> Values { get; } = [];

        public void Report(T value) => Values.Add(value);
    }

    private sealed class CancellationObservingFileSystem : IFileSystem {
        public string Root { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "filequery-cancel");

        public IEnumerable<FileSystemEntry> Enumerate(
            string directory,
            bool ignoreInaccessible,
            FileQueryErrorRecoveryOptions errorRecovery
        ) {
            yield return new FileSystemEntry(System.IO.Path.Combine(Root, "a.txt"), FileAttributes.Normal);
        }

        public async IAsyncEnumerable<FileSystemEntry> EnumerateAsync(
            string directory,
            bool ignoreInaccessible,
            FileQueryErrorRecoveryOptions errorRecovery,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
        ) {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Yield();
            yield return new FileSystemEntry(System.IO.Path.Combine(Root, "a.txt"), FileAttributes.Normal);
        }

        public FileAttributes GetAttributes(string path) => FileAttributes.Normal;

        public bool DirectoryExists(string path) => path == Root;

        public string ResolveRealPath(string path) => path;

        public char DirectorySeparator => System.IO.Path.DirectorySeparatorChar;
        public string GetFullPath(string path) => path;

        public string GetFullPath(string path, string basePath) => System.IO.Path.Combine(basePath, path);
    }

    private sealed class RecoverableFailureFileSystem(int failLockedDirectoryAttempts) : IFileSystem {
        private int _lockedDirectoryAttempts;

        public string Root { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "filequery-recovery");
        public string KeepFile => System.IO.Path.Combine(Root, "keep.txt");
        public string LockedDirectory => System.IO.Path.Combine(Root, "locked");
        public string RetryFile => System.IO.Path.Combine(LockedDirectory, "retry.txt");

        public IEnumerable<FileSystemEntry> Enumerate(
            string directory,
            bool ignoreInaccessible,
            FileQueryErrorRecoveryOptions errorRecovery
        ) {
            if(directory == Root) {
                yield return new FileSystemEntry(KeepFile, FileAttributes.Normal);
                yield return new FileSystemEntry(LockedDirectory, FileAttributes.Directory);
                yield break;
            }

            if(directory == LockedDirectory) {
                while(_lockedDirectoryAttempts < failLockedDirectoryAttempts) {
                    _lockedDirectoryAttempts++;

                    if(errorRecovery.Action is FileQueryErrorAction.Skip || ignoreInaccessible) {
                        yield break;
                    }

                    if(errorRecovery.Action is FileQueryErrorAction.Retry &&
                        _lockedDirectoryAttempts <= errorRecovery.MaxRetryAttempts) {
                        continue;
                    }

                    throw new IOException("Simulated directory failure.");
                }

                yield return new FileSystemEntry(RetryFile, FileAttributes.Normal);
            }
        }

        public async IAsyncEnumerable<FileSystemEntry> EnumerateAsync(
            string directory,
            bool ignoreInaccessible,
            FileQueryErrorRecoveryOptions errorRecovery,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
        ) {
            foreach(var entry in Enumerate(directory, ignoreInaccessible, errorRecovery)) {
                cancellationToken.ThrowIfCancellationRequested();
                yield return entry;
                await Task.Yield();
            }
        }

        public FileAttributes GetAttributes(string path) => path == LockedDirectory ? FileAttributes.Directory : FileAttributes.Normal;

        public bool DirectoryExists(string path) => path == Root;

        public string ResolveRealPath(string path) => path;

        public char DirectorySeparator => System.IO.Path.DirectorySeparatorChar;
        public string GetFullPath(string path) => path;

        public string GetFullPath(string path, string basePath) => System.IO.Path.Combine(basePath, path);
    }
}
