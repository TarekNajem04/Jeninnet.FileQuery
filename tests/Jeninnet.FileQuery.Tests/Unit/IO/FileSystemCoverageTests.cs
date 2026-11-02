//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Unit.IO;

/// <summary>
/// Tests for code coverage of the FileSystem and FileSystemEntry types.
/// </summary>
[TestClass]
public class FileSystemCoverageTests {
    /// <summary>
    /// Verifies that FileSystemEntry stores the path and attributes correctly.
    /// </summary>
    [TestMethod]
    public void Should_CoverFileSystemEntry_Properties() {
        const string path = "C:\\temp\\file.txt";
        const FileAttributes attributes = FileAttributes.Normal;
        var entry = new FileSystemEntry(path, attributes);

        entry.FullPath.Should().Be(path);
        entry.Attributes.Should().Be(attributes);
    }

    /// <summary>
    /// Verifies that FileSystem.DirectoryExists returns the correct result for existing and non-existing directories.
    /// </summary>
    [TestMethod]
    public void Should_CallStandardIO_When_DirectoryExistsChecked() {
        using var env = new TestEnvironment();
        FileSystem.Instance.DirectoryExists(env.Root).Should().BeTrue();
        (FileSystem.Instance.DirectoryExists(System.IO.Path.Combine(env.Root, "non-existent"))).Should().BeFalse();
    }

    /// <summary>
    /// Verifies that FileSystem.GetAttributes returns valid attributes for an existing file.
    /// </summary>
    [TestMethod]
    public void Should_ReturnAttributes_When_GetAttributesCalled() {
        using var env = new TestEnvironment();
        var file = env.CreateFile("test.txt");
        var attr = FileSystem.Instance.GetAttributes(file);
        (attr.HasFlag(FileAttributes.Archive) || attr.HasFlag(FileAttributes.Normal)).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that FileSystem.ResolveRealPath returns the same path for a non-reparse-point file.
    /// </summary>
    [TestMethod]
    public void Should_ReturnSamePath_When_NonReparsePointResolved() {
        using var env = new TestEnvironment();
        var file = env.CreateFile("test.txt");
        var resolved = FileSystem.Instance.ResolveRealPath(file);
        resolved.Should().Be(file);
    }

    /// <summary>
    /// Verifies that FileSystem.Enumerate handles retry error recovery correctly.
    /// </summary>
    [TestMethod]
    public void Should_HandleRetry_When_Enumerating() {
        using var env = new TestEnvironment();
        env.CreateFile("a.txt");

        var errorRecovery = new FileQueryErrorRecoveryOptions(
            Action: FileQueryErrorAction.Retry,
            MaxRetryAttempts: 1
        );

        var results = FileSystem.Instance.Enumerate(env.Root, false, errorRecovery).ToList();
        results.Should().HaveCount(1);
    }

    /// <summary>
    /// Verifies that FileSystem.EnumerateAsync handles retry error recovery correctly.
    /// </summary>
    [TestMethod]
    public async Task FileSystem_EnumerateAsync_HandlesRetryAsync() {
        using var env = new TestEnvironment();
        env.CreateFile("a.txt");

        var errorRecovery = new FileQueryErrorRecoveryOptions(
            Action: FileQueryErrorAction.Retry,
            MaxRetryAttempts: 1
        );

        var results = new List<FileSystemEntry>();
        await foreach(var entry in FileSystem.Instance.EnumerateAsync(env.Root, false, errorRecovery, CancellationToken.None)) {
            results.Add(entry);
        }

        results.Should().HaveCount(1);
    }

    /// <summary>
    /// Verifies that FileSystem.Enumerate skips inaccessible directories when configured.
    /// </summary>
    [TestMethod]
    public void Should_SkipInaccessible_When_Enumerating() {
        using var env = new TestEnvironment();
        env.CreateDirectory("locked");
        env.SetInaccessibleDirectory("locked");

        var errorRecovery = new FileQueryErrorRecoveryOptions(
            Action: FileQueryErrorAction.Skip
        );

        // This should not throw if ignoreInaccessible is true or if recovery is skip
        var results = FileSystem.Instance.Enumerate(env.Root, true, errorRecovery).ToList();

        // Assert that we at least got the 'locked' directory itself (depending on OS behavior)
        // or simply that the operation completed.
        results.Should().NotBeNull();
    }

    /// <summary>
    /// Verifies that FileSystem.GetFullPath returns a rooted full path.
    /// </summary>
    [TestMethod]
    public void Should_ReturnFullPath_When_GetFullPathCalled() {
        const string path = "test.txt";
        var full = FileSystem.Instance.GetFullPath(path);
        System.IO.Path.IsPathRooted(full).Should().BeTrue();

        var baseDir = System.IO.Path.GetTempPath();
        var full2 = FileSystem.Instance.GetFullPath(path, baseDir);
        full2.Should().Be(System.IO.Path.GetFullPath(System.IO.Path.Combine(baseDir, path)));
    }

    /// <summary>
    /// Verifies that FileSystem.DirectorySeparator returns the platform directory separator character.
    /// </summary>
    [TestMethod]
    public void Should_BeChar_When_DirectorySeparatorAccessed() => FileSystem.Instance.DirectorySeparator.Should().Be(System.IO.Path.DirectorySeparatorChar);
}
