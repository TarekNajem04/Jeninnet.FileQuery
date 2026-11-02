//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.FileProviders.Tests;

/// <summary>
/// Tests the IFileProvider adapter with physical and in-memory providers.
/// </summary>
[TestClass]
public sealed class FileProviderQueryTests {
    private static readonly string[] _physicalMatches = ["a.cs", "src/c.cs"];
    private static readonly string[] _memoryMatches = ["src/b.cs", "src/nested/c.cs"];
    private static readonly string[] _scopedMatches = ["/src/keep.cs"];
    private static readonly string[] _gitignoreMatches = ["/.gitignore", "/a.txt"];

    /// <summary>
    /// Provider enumeration failures honor the configured skip/abort policy.
    /// </summary>
    [TestMethod]
    public void Provider_EnumerationFailure_HonorsErrorRecovery() {
        var provider = new InMemoryFileProvider(throwOnEnumeration: true);
        var skipped = FileProviderQuery.From(provider).Execute(TestContext.CancellationToken).ToArray();
        Assert.IsEmpty(skipped);

        var aborting = FileProviderQuery.From(provider)
            .WithErrorRecovery(FileQueryErrorRecoveryOptions.Abort);

        Assert.ThrowsExactly<UnauthorizedAccessException>(() => aborting.Execute(TestContext.CancellationToken).ToArray());
    }

    /// <summary>
    /// Null providers and empty roots are rejected at the entry point.
    /// </summary>
    [TestMethod]
    public void From_InvalidArguments_Throw() {
        Assert.ThrowsExactly<ArgumentNullException>(() => FileProviderQuery.From(null!));
        Assert.ThrowsExactly<ArgumentException>(() => FileProviderQuery.From(new InMemoryFileProvider(), " "));
    }

    /// <summary>
    /// Queries a physical provider with the normal matching pipeline.
    /// </summary>
    [TestMethod]
    public void PhysicalProvider_MatchesAndReturnsMetadata() {
        using var environment = new TestEnvironment();
        environment.CreateFile("a.cs", "class A {}");
        environment.CreateFile("b.txt", "text");
        environment.CreateFile("src/c.cs", "class C {}");
        using var provider = new PhysicalFileProvider(environment.Root);

        var entries = FileProviderQuery.From(provider)
            .UsingGlob()
            .Where(PatternKind.Glob, ["**/*.cs"])
            .ExecuteEntries(TestContext.CancellationToken)
            .OrderBy(static entry => entry.RelativePath, StringComparer.Ordinal)
            .ToArray();

        Assert.AreSequenceEqual(_physicalMatches, entries.Select(static entry => entry.RelativePath));
        Assert.IsTrue(entries.All(static entry => entry.Length > 0));
        Assert.IsTrue(entries.All(static entry => entry.LastWriteTimeUtc > DateTime.MinValue));
        Assert.IsTrue(entries.All(static entry => entry.RootPath == "/"));
    }

    /// <summary>
    /// Queries an in-memory provider without materializing files.
    /// </summary>
    [TestMethod]
    public void InMemoryProvider_MatchesNestedFiles() {
        var provider = new InMemoryFileProvider();
        provider.AddFile("a.txt", "a");
        provider.AddFile("src/b.cs", "class B {}");
        provider.AddFile("src/nested/c.cs", "class C {}");

        var entries = FileProviderQuery.From(provider)
            .UsingGlob()
            .Where(PatternKind.Glob, ["**/*.cs"])
            .ExecuteEntries(TestContext.CancellationToken)
            .OrderBy(static entry => entry.RelativePath, StringComparer.Ordinal)
            .ToArray();

        Assert.AreSequenceEqual(
            _memoryMatches, entries.Select(static entry => entry.RelativePath));
        Assert.AreEqual("class B {}".Length, entries[0].Length);
    }

    /// <summary>
    /// Git ignore files are read through the provider abstraction.
    /// </summary>
    [TestMethod]
    public void Provider_GitIgnoreFiles_Work() {
        var provider = new InMemoryFileProvider();
        provider.AddFile(".gitignore", "*.log\n");
        provider.AddFile("a.txt", "a");
        provider.AddFile("b.log", "b");

        var paths = FileProviderQuery.From(provider)
            .UseGitIgnoreFiles()
            .Execute(TestContext.CancellationToken)
            .OrderBy(static path => path, StringComparer.Ordinal)
            .ToArray();

        Assert.AreSequenceEqual(_gitignoreMatches, paths);
    }

    /// <summary>
    /// Scoped roots and metadata filters work through the provider adapter.
    /// </summary>
    [TestMethod]
    public void Provider_ScopedRootAndFilters_Work() {
        var provider = new InMemoryFileProvider();
        provider.AddFile("src/keep.cs", "keep");
        provider.AddFile("src/skip.txt", "skip");
        provider.AddFile("other/keep.cs", "other");

        var paths = FileProviderQuery.From(provider, "/src")
            .UsingGlob()
            .Where(PatternKind.Glob, ["**/*.cs"])
            .MinSize(1)
            .Execute(TestContext.CancellationToken)
            .OrderBy(static path => path, StringComparer.Ordinal)
            .ToArray();

        Assert.AreSequenceEqual(_scopedMatches, paths);
    }

    /// <summary>
    /// Directory results and async execution work through the adapter.
    /// </summary>
    [TestMethod]
    public async Task Provider_DirectoryResultsAndAsync_WorkAsync() {
        var provider = new InMemoryFileProvider();
        provider.AddFile("src/a.cs", "a");
        provider.AddFile("src/nested/b.cs", "b");

        var builder = FileProviderQuery.From(provider).IncludeDirectories();
        var entries = new List<FileQueryEntry>();

        await foreach(var entry in builder.ExecuteEntriesAsync(TestContext.CancellationToken)) {
            entries.Add(entry);
        }

        Assert.Contains(static entry => entry.RelativePath == "src" && entry.IsDirectory, entries);
        Assert.Contains(static entry => entry.RelativePath == "src/nested" && entry.IsDirectory, entries);
        Assert.Contains(static entry => entry.RelativePath == "src/nested/b.cs" && !entry.IsDirectory, entries);
    }

    /// <summary>
    /// Gets or sets the test context, which provides information about the current test run.
    /// Used here primarily for accessing the <see cref="CancellationToken"/>.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;
}
