//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.Testing.Assertions.Tests.Extensions;

/// <summary>Verifies <see cref="TestEnvironmentExtensions"/> methods including edge cases and null guards.</summary>
[TestClass]
public sealed class TestEnvironmentExtensionsTests {
    /// <summary>CreateDeepDirectoryTree succeeds with a positive level and default file settings.</summary>
    [TestMethod]
    public void CreateDeepDirectoryTree_WithPositiveLevels_CreatesTree() {
        using var env = new TestEnvironment();
        env.CreateDeepDirectoryTree(levels: 2);
        Assert.IsTrue(Directory.Exists(env.Root));
    }

    /// <summary>CreateDeepDirectoryTree with zero levels returns immediately without creating anything.</summary>
    [TestMethod]
    public void CreateDeepDirectoryTree_ZeroLevels_DoesNotThrow() {
        using var env = new TestEnvironment();
        env.CreateDeepDirectoryTree(levels: 0);
        Assert.IsTrue(Directory.Exists(env.Root));
    }

    /// <summary>CreateDeepDirectoryTree with zero files creates directories without files.</summary>
    [TestMethod]
    public void CreateDeepDirectoryTree_ZeroFiles_DoesNotThrow() {
        using var env = new TestEnvironment();
        env.CreateDeepDirectoryTree(levels: 1, fileCount: 0);
        Assert.IsTrue(Directory.Exists(env.Root));
    }

    /// <summary>CreateDeepDirectoryTree with a null environment throws ArgumentNullException.</summary>
    [TestMethod]
    public void CreateDeepDirectoryTree_NullEnvironment_Throws() => Assert.ThrowsExactly<ArgumentNullException>(static () => default(TestEnvironment)!.CreateDeepDirectoryTree(levels: 1));

    /// <summary>CreateDeepDirectoryTree creates the expected directory structure depth.</summary>
    [TestMethod]
    public void CreateDeepDirectoryTree_OneLevel_CreatesSubdirectory() {
        using var env = new TestEnvironment();
        env.CreateDeepDirectoryTree(levels: 1);
        Assert.HasCount(1, Directory.GetDirectories(env.Root));
    }

    /// <summary>CreateDeepDirectoryTree with a high level count does not throw.</summary>
    [TestMethod]
    public void CreateDeepDirectoryTree_FiveLevels_DoesNotThrow() {
        using var env = new TestEnvironment();
        env.CreateDeepDirectoryTree(levels: 5, fileCount: 0);
        Assert.IsTrue(Directory.Exists(env.Root));
    }

    /// <summary>CreateDeepDirectoryTree with a positive file count creates files at the leaf level.</summary>
    [TestMethod]
    public void CreateDeepDirectoryTree_WithFileCount_CreatesFiles() {
        using var env = new TestEnvironment();
        env.CreateDeepDirectoryTree(levels: 1, fileCount: 2);
        var subdirs = Directory.GetDirectories(env.Root);
        Assert.HasCount(1, subdirs);
        Assert.IsGreaterThanOrEqualTo(2, Directory.GetFiles(subdirs[0]).Length);
    }
}
