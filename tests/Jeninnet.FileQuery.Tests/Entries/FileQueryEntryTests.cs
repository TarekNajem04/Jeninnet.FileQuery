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

        entry.FullPath.Should().Be(smallPath);
        entry.Length.Should().Be(3);
        entry.IsDirectory.Should().BeFalse();
        (entry.LastWriteTimeUtc.Ticks / TimeSpan.TicksPerSecond).Should().Be(File.GetLastWriteTimeUtc(smallPath).Ticks / TimeSpan.TicksPerSecond);
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

        (entries.Select(static entry => entry.RelativePath)).Should().BeEquivalentTo(_nestedEntries);
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

        (asyncEntries.OrderBy(static path => path, StringComparer.Ordinal)).Should().Equal(sync);
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

        entryPaths.Should().Equal(paths);
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

        entries.Should().Equal(_sizeFilteredEntries);
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

        entries.Should().Equal(_sizeFilteredEntries);
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

        (filter.ShouldInclude(new FileSystemEntry("readonly.txt", FileAttributes.ReadOnly))).Should().BeFalse();
        (filter.ShouldInclude(new FileSystemEntry("normal.txt", FileAttributes.Normal))).Should().BeTrue();
        (filter.ShouldInclude(new FileSystemEntry("readonly-dir", FileAttributes.ReadOnly | FileAttributes.Directory))).Should().BeFalse();
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

        FluentActions.Invoking(() => sizeOptions.Validate()).Should().ThrowExactly<ArgumentException>();
        FluentActions.Invoking(() => timeOptions.Validate()).Should().ThrowExactly<ArgumentException>();
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
