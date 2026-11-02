//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
using System.Runtime.CompilerServices;
using Jeninnet.FileQuery.IO;
using Jeninnet.FileQuery.Traversal;

namespace Jeninnet.FileQuery.Tests.Unit.Traversal;

/// <summary>
/// Tests for <see cref="TraversalPlanBuilder"/> root normalization.
/// </summary>
[TestClass]
public sealed class TraversalPlanBuilderTests {
    /// <summary>
    /// A Windows drive root keeps its trailing separator instead of becoming a drive-relative path.
    /// </summary>
    [TestMethod]
    public void Build_PreservesDriveRoot() {
        if(!OperatingSystem.IsWindows()) {
            return;
        }

        var fileSystem = new RootFileSystem(@"C:\", '\\');
        var builder = new TraversalPlanBuilder(fileSystem);
        var plan = builder.Build(
            new FileQuery(
                @"C:\",
                new FileQueryOptions(new FileQueryOptionsConfig(new PatternInput()))));

        Assert.AreEqual(@"C:\", plan.RootDirectory);
    }

    /// <summary>
    /// A non-root trailing separator is still removed for canonical relative paths.
    /// </summary>
    [TestMethod]
    public void Build_TrimsNonRootTrailingSeparator() {
        var separator = System.IO.Path.DirectorySeparatorChar;
        var root = $"{System.IO.Path.GetTempPath()}traversal-plan-root{separator}";
        var expected = root.TrimEnd(separator);

        var fileSystem = new RootFileSystem(root, separator);
        var builder = new TraversalPlanBuilder(fileSystem);
        var plan = builder.Build(
            new FileQuery(
                root,
                new FileQueryOptions(new FileQueryOptionsConfig(new PatternInput()))));

        Assert.AreEqual(expected, plan.RootDirectory);
    }

    private sealed class RootFileSystem(string root, char separator) : IFileSystem {
        public IEnumerable<FileSystemEntry> Enumerate(
            string directory,
            bool ignoreInaccessible,
            FileQueryErrorRecoveryOptions errorRecovery
        ) => [];

        public async IAsyncEnumerable<FileSystemEntry> EnumerateAsync(
            string directory,
            bool ignoreInaccessible,
            FileQueryErrorRecoveryOptions errorRecovery,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
        ) {
            await Task.CompletedTask;
            yield break;
        }

        public FileAttributes GetAttributes(string path) => FileAttributes.Normal;

        public bool DirectoryExists(string path) => true;

        public bool FileExists(string path) => false;

        public string[] ReadAllLines(string path) => [];

        public string ResolveRealPath(string path) => path;

        public char DirectorySeparator => separator;

        public string GetFullPath(string path) => root;

        public string GetFullPath(string path, string basePath) => root;
    }
}
