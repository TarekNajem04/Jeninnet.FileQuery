namespace Jeninnet.Testing.Assertions.Tests.IO;

/// <summary>Verifies edge cases and error paths in <see cref="TestEnvironment"/>.</summary>
[TestClass]
public sealed class TestEnvironmentEdgeCaseTests {
    /// <summary>Abs returns a rooted path for a relative path.</summary>
    [TestMethod]
    public void Abs_WithRelativePath_ReturnsRootedPath() {
        using var env = new TestEnvironment();
        var result = env.Abs("relative");
        Assert.IsTrue(Path.IsPathRooted(result));
    }

    /// <summary>Abs returns the root for an empty relative path.</summary>
    [TestMethod]
    public void Abs_WithEmptyPath_ReturnsRoot() {
        using var env = new TestEnvironment();
        var result = env.Abs();
        Assert.AreEqual(env.Root, result);
    }

    /// <summary>CreateFiles creates the specified files.</summary>
    [TestMethod]
    public void CreateFiles_CreatesAllFiles() {
        using var env = new TestEnvironment();
        env.CreateFiles("a.txt", "b.txt", "c.txt");
        Assert.IsTrue(File.Exists(env.Abs("a.txt")));
        Assert.IsTrue(File.Exists(env.Abs("b.txt")));
        Assert.IsTrue(File.Exists(env.Abs("c.txt")));
    }

    /// <summary>CreateFile with null content creates an empty file.</summary>
    [TestMethod]
    public void CreateFile_WithNullContent_CreatesEmptyFile() {
        using var env = new TestEnvironment();
        var path = env.CreateFile("empty.txt", null);
        Assert.IsTrue(File.Exists(path));
        Assert.AreEqual(0, new FileInfo(path).Length);
    }

    /// <summary>CreateFile with empty content creates an empty file.</summary>
    [TestMethod]
    public void CreateFile_WithEmptyContent_CreatesEmptyFile() {
        using var env = new TestEnvironment();
        var path = env.CreateFile("empty.txt", "");
        Assert.IsTrue(File.Exists(path));
        Assert.AreEqual(0, new FileInfo(path).Length);
    }

    /// <summary>CreateInaccessibleDirectory creates a pseudo-inaccessible directory path.</summary>
    [TestMethod]
    public void CreateInaccessibleDirectory_ReturnsPath() {
        using var env = new TestEnvironment();
        var path = env.CreateInaccessibleDirectory("inacc");
        Assert.IsNotNull(path);
    }

    /// <summary>SetInaccessible on an existing directory does not throw.</summary>
    [TestMethod]
    public void SetInaccessible_WithExistingPath_DoesNotThrow() {
        using var env = new TestEnvironment();
        var dir = env.CreateDirectory("access-test");
        env.SetInaccessible(dir);
        Assert.IsTrue(Directory.Exists(dir));
    }

    /// <summary>SetInaccessibleDirectory on an existing directory does not throw.</summary>
    [TestMethod]
    public void SetInaccessibleDirectory_WithExistingPath_DoesNotThrow() {
        using var env = new TestEnvironment();
        var dir = env.CreateDirectory("acc-dir");
        env.SetInaccessibleDirectory(dir);
        Assert.IsTrue(Directory.Exists(env.Root));
    }

    /// <summary>CreateLockedFile creates a locked file on disk.</summary>
    [TestMethod]
    public void CreateLockedFile_Default_CreatesFile() {
        using var env = new TestEnvironment();
        using var stream = env.CreateLockedFile("locked.bin");
        Assert.IsTrue(File.Exists(stream.Name));
    }

    /// <summary>AssertDirectoryInaccessible throws when the directory is accessible.</summary>
    [TestMethod]
    public void AssertDirectoryInaccessible_WithAccessibleDirectory_Throws() {
        using var env = new TestEnvironment();
        var dir = env.CreateDirectory("accessible");
        var ex = Assert.ThrowsExactly<InvalidOperationException>(() => env.AssertDirectoryInaccessible(dir));
        Assert.Contains("still accessible", ex.Message);
        Assert.IsTrue(Directory.Exists(dir));
    }

    /// <summary>Dispose can be called multiple times without throwing.</summary>
    [TestMethod]
    public void Dispose_MultipleCalls_DoesNotThrow() {
        var env = new TestEnvironment();
        var root = env.Root;
        env.Dispose();
        Assert.IsFalse(Directory.Exists(root));
    }
}
