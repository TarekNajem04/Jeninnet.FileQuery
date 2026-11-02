//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Entries;

/// <summary>
/// Tests for <see cref="FileQueryEntry"/> result APIs and metadata filters.
/// </summary>
[TestClass]
public sealed class FileQueryEntryTests {
    private static readonly string[] _nestedEntries = ["large.txt", "small.txt", "sub/medium.txt"];
    private static readonly string[] _sizeFilteredEntries = ["large.txt", "sub/medium.txt"];

    /// <summary>
    /// Entries expose paths and metadata captured by the enumeration.
    /// </summary>
    [TestMethod]
    public void ExecuteEntries_ReturnsMetadataAndRelativePaths() {
        using var environment = CreateEnvironment();
        var smallPath = environment.Abs("small.txt");

        var entry = FileQuery.From(environment.Root)
            .UsingGitIgnore()
            .Where(PatternKind.GitIgnore, ["**", "!**/*.txt"])
            .ExecuteEntries(TestContext.CancellationToken)
            .Single(candidate => candidate.RelativePath == "small.txt");

        Assert.AreEqual(smallPath, entry.FullPath);
        Assert.AreEqual(3, entry.Length);
        Assert.IsFalse(entry.IsDirectory);
        Assert.AreEqual(File.GetLastWriteTimeUtc(smallPath).Ticks / TimeSpan.TicksPerSecond, entry.LastWriteTimeUtc.Ticks / TimeSpan.TicksPerSecond);
    }

    /// <summary>
    /// Relative paths use forward slashes for nested entries.
    /// </summary>
    [TestMethod]
    public void ExecuteEntries_NormalizesNestedRelativePaths() {
        using var environment = CreateEnvironment();

        var entries = FileQuery.From(environment.Root)
            .UsingGitIgnore()
            .Where(PatternKind.GitIgnore, ["**", "!**/*.txt"])
            .ExecuteEntries(TestContext.CancellationToken)
            .ToArray();

        Assert.AreSequenceEqual(
            _nestedEntries, entries.Select(static entry => entry.RelativePath), Microsoft.VisualStudio.TestTools.UnitTesting.SequenceOrder.InAnyOrder);
    }

    /// <summary>
    /// The asynchronous API returns the same entries as the synchronous API.
    /// </summary>
    [TestMethod]
    public async Task ExecuteEntriesAsync_ReturnsSameEntriesAsync() {
        using var environment = CreateEnvironment();
        var query = FileQuery.From(environment.Root)
            .UsingGitIgnore()
            .Where(PatternKind.GitIgnore, ["**", "!**/*.txt"]);

        var sync = query.ExecuteEntries(TestContext.CancellationToken).Select(static entry => entry.RelativePath).OrderBy(static path => path, StringComparer.Ordinal).ToArray();
        var asyncEntries = new List<string>();

        await foreach(var entry in query.ExecuteEntriesAsync(TestContext.CancellationToken)) {
            asyncEntries.Add(entry.RelativePath);
        }

        Assert.AreSequenceEqual(sync, asyncEntries.OrderBy(static path => path, StringComparer.Ordinal));
    }

    /// <summary>
    /// The path-only result API returns the same files as the entry API.
    /// </summary>
    [TestMethod]
    public void Execute_PathOnlyApi_RemainsCompatible() {
        using var environment = CreateEnvironment();
        var builder = FileQuery.From(environment.Root)
            .UsingGitIgnore()
            .Where(PatternKind.GitIgnore, ["**", "!**/*.txt"]);

        var paths = builder.Execute(TestContext.CancellationToken).OrderBy(static path => path, StringComparer.Ordinal).ToArray();
        var entryPaths = builder.ExecuteEntries(TestContext.CancellationToken)
            .Select(static entry => entry.FullPath)
            .OrderBy(static path => path, StringComparer.Ordinal)
            .ToArray();

        Assert.AreSequenceEqual(paths, entryPaths);
    }

    /// <summary>
    /// Size filters are inclusive and prune before matching.
    /// </summary>
    [TestMethod]
    public void SizeFilters_PruneBeforeMatching() {
        using var environment = CreateEnvironment();

        var entries = FileQuery.From(environment.Root)
            .UsingGitIgnore()
            .Where(PatternKind.GitIgnore, ["**", "!**/*.txt"])
            .MinSize(5)
            .MaxSize(10)
            .ExecuteEntries(TestContext.CancellationToken)
            .Select(static entry => entry.RelativePath)
            .OrderBy(static path => path, StringComparer.Ordinal)
            .ToArray();

        Assert.AreSequenceEqual(_sizeFilteredEntries, entries);
    }

    /// <summary>
    /// Time filters are exclusive and use the last-write timestamp from the enumeration.
    /// </summary>
    [TestMethod]
    public void TimeFilters_PruneBeforeMatching() {
        using var environment = CreateEnvironment();

        var entries = FileQuery.From(environment.Root)
            .UsingGitIgnore()
            .Where(PatternKind.GitIgnore, ["**", "!**/*.txt"])
            .ModifiedAfter(_oldTimestamp)
            .ModifiedBefore(_recentTimestamp.AddDays(1))
            .ExecuteEntries(TestContext.CancellationToken)
            .Select(static entry => entry.RelativePath)
            .OrderBy(static path => path, StringComparer.Ordinal)
            .ToArray();

        Assert.AreSequenceEqual(_sizeFilteredEntries, entries);
    }

    /// <summary>
    /// Attribute filters prune matching entries and directories.
    /// </summary>
    [TestMethod]
    public void EntryFilter_SkipsMatchingAttributes() {
        var filter = new EntryFilter(
            MinSize: null,
            MaxSize: null,
            ModifiedAfterUtc: null,
            ModifiedBeforeUtc: null,
            AttributesToSkip: FileAttributes.ReadOnly);

        Assert.IsFalse(filter.ShouldInclude(new FileSystemEntry("readonly.txt", FileAttributes.ReadOnly)));
        Assert.IsTrue(filter.ShouldInclude(new FileSystemEntry("normal.txt", FileAttributes.Normal)));
        Assert.IsFalse(filter.ShouldInclude(new FileSystemEntry("readonly-dir", FileAttributes.ReadOnly | FileAttributes.Directory)));
    }

    /// <summary>
    /// Invalid size and time ranges are rejected.
    /// </summary>
    [TestMethod]
    public void Options_Reject_InvalidMetadataRanges() {
        var sizeOptions = new FileQueryOptions(new FileQueryOptionsConfig(
            new PatternInput(),
            MinSize: 10,
            MaxSize: 1));
        var timeOptions = new FileQueryOptions(new FileQueryOptionsConfig(
            new PatternInput(),
            ModifiedAfter: _recentTimestamp,
            ModifiedBefore: _oldTimestamp));

        Assert.ThrowsExactly<ArgumentException>(sizeOptions.Validate);
        Assert.ThrowsExactly<ArgumentException>(timeOptions.Validate);
    }

    private static readonly DateTimeOffset _oldTimestamp = new(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset _recentTimestamp = new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// Creates a tree with known sizes and timestamps.
    /// </summary>
    private static TestEnvironment CreateEnvironment() {
        var environment = new TestEnvironment();
        environment.CreateFile("small.txt", "abc");
        environment.CreateFile("large.txt", new string('x', 10));
        environment.CreateFile("sub/medium.txt", "12345");
        environment.CreateFile("ignored.log", "log");

        File.SetLastWriteTimeUtc(environment.Abs("small.txt"), _oldTimestamp.UtcDateTime);
        File.SetLastWriteTimeUtc(environment.Abs("large.txt"), _recentTimestamp.UtcDateTime);
        File.SetLastWriteTimeUtc(environment.Abs("sub/medium.txt"), _recentTimestamp.UtcDateTime);

        return environment;
    }

    /// <summary>
    /// Gets or sets the test context, which provides information about the current test run.
    /// Used here primarily for accessing the <see cref="CancellationToken"/>.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;
}
