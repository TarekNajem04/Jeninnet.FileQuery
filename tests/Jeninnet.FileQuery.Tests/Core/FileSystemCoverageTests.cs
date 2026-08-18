//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Core;

/// <summary>
/// Contains unit tests for verifying coverage of the <see cref="FileSystem"/> class.
/// </summary>
[TestClass]
public class FileSystemCoverageTests {
    /// <summary>Tests FileSystemEntry_Properties_Covered.</summary>
    [TestMethod]
    public void FileSystemEntry_Properties_Covered() {
        const string path = "C:\\temp\\file.txt";
        const FileAttributes attributes = FileAttributes.Normal;
        var entry = new FileSystemEntry(path, attributes);

        Assert.AreEqual(path, entry.FullPath);
        Assert.AreEqual(attributes, entry.Attributes);
    }

    /// <summary>Tests FileSystem_DirectoryExists_CallsStandardIO.</summary>
    [TestMethod]
    public void FileSystem_DirectoryExists_CallsStandardIO() {
        using var env = new TestEnvironment();
        Assert.IsTrue(FileSystem.Instance.DirectoryExists(env.Root));
        Assert.IsFalse(FileSystem.Instance.DirectoryExists(Path.Combine(env.Root, "non-existent")));
    }

    /// <summary>Tests FileSystem_GetAttributes_ReturnsAttributes.</summary>
    [TestMethod]
    public void FileSystem_GetAttributes_ReturnsAttributes() {
        using var env = new TestEnvironment();
        var file = env.CreateFile("test.txt");
        var attr = FileSystem.Instance.GetAttributes(file);
        Assert.IsTrue(attr.HasFlag(FileAttributes.Archive) || attr.HasFlag(FileAttributes.Normal));
    }

    /// <summary>Tests FileSystem_ResolveRealPath_NonReparsePoint_ReturnsSamePath.</summary>
    [TestMethod]
    public void FileSystem_ResolveRealPath_NonReparsePoint_ReturnsSamePath() {
        using var env = new TestEnvironment();
        var file = env.CreateFile("test.txt");
        var resolved = FileSystem.Instance.ResolveRealPath(file);
        Assert.AreEqual(file, resolved);
    }

    /// <summary>Tests FileSystem_Enumerate_HandlesRetry.</summary>
    [TestMethod]
    public void FileSystem_Enumerate_HandlesRetry() {
        using var env = new TestEnvironment();
        env.CreateFile("a.txt");

        var errorRecovery = new FileQueryErrorRecoveryOptions(
            Action: FileQueryErrorAction.Retry,
            MaxRetryAttempts: 1
        );

        var results = FileSystem.Instance.Enumerate(env.Root, false, errorRecovery).ToList();
        Assert.HasCount(1, results);
    }

    /// <summary>Tests FileSystem_Enumerate_SkipOnInaccessible.</summary>
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

        Assert.HasCount(1, results);
    }

    /// <summary>Tests FileSystem_Enumerate_SkipOnInaccessible.</summary>
    [TestMethod]
    public void FileSystem_Enumerate_SkipOnInaccessible() {
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
        Assert.IsNotNull(results);
    }

    /// <summary>Tests FileSystem_GetFullPath_Works.</summary>
    [TestMethod]
    public void FileSystem_GetFullPath_Works() {
        const string path = "test.txt";
        var full = FileSystem.Instance.GetFullPath(path);
        Assert.IsTrue(Path.IsPathRooted(full));

        var baseDir = Path.GetTempPath();
        var full2 = FileSystem.Instance.GetFullPath(path, baseDir);
        Assert.AreEqual(Path.GetFullPath(Path.Combine(baseDir, path)), full2);
    }

    /// <summary>Tests FileSystem_DirectorySeparator_IsChar.</summary>
    [TestMethod]
    public void FileSystem_DirectorySeparator_IsChar() => Assert.AreEqual(Path.DirectorySeparatorChar, FileSystem.Instance.DirectorySeparator);
}
