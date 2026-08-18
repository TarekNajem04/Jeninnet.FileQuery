//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.Testing.Assertions.Tests.IO;

/// <summary>Smoke tests for TestEnvironment: creation, disposal, and file/directory operations.</summary>
[TestClass]
public sealed class TestEnvironmentSmokeTests {
    /// <summary>Creating and disposing a TestEnvironment does not throw; the root directory is removed.</summary>
    [TestMethod]
    public void CreateAndDispose_DoesNotThrow() {
        var env = new TestEnvironment();
        var root = env.Root;
        env.Dispose();
        Assert.IsFalse(Directory.Exists(root));
    }

    /// <summary>After creation, the root directory exists on disk.</summary>
    [TestMethod]
    public void Root_ExistsAfterCreation() {
        using var env = new TestEnvironment();
        Assert.IsTrue(Directory.Exists(env.Root));
    }

    /// <summary>After disposal, the root directory is deleted.</summary>
    [TestMethod]
    public void Root_DeletedAfterDispose() {
        string root;
        var env = new TestEnvironment();
        root = env.Root;
        env.Dispose();
        Assert.IsFalse(Directory.Exists(root));
    }

    /// <summary>CreateDirectory creates the directory and returns its path.</summary>
    [TestMethod]
    public void CreateDirectory_ReturnsExistingPath() {
        using var env = new TestEnvironment();
        var dir = env.CreateDirectory("sub/test");
        Assert.IsTrue(Directory.Exists(dir));
    }

    /// <summary>CreateFile creates the file with the specified content and returns its path.</summary>
    [TestMethod]
    public void CreateFile_WithContent_ReturnsPath() {
        using var env = new TestEnvironment();
        var file = env.CreateFile("test.txt", "content");
        Assert.IsTrue(File.Exists(file));
        Assert.AreEqual("content", File.ReadAllText(file));
    }
}
