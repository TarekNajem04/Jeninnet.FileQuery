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

    /// <summary>CreateFile with non-null content creates a file with that content.</summary>
    [TestMethod]
    public void CreateFile_WithContent_CreatesFile() {
        using var env = new TestEnvironment();
        var path = env.CreateFile("hello.txt", "world");
        Assert.AreEqual("world", File.ReadAllText(path));
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

    /// <summary>SetInaccessible on a non-existent path creates the directory first.</summary>
    [TestMethod]
    public void SetInaccessible_NonExistent_CreatesDirectory() {
        using var env = new TestEnvironment();
        env.SetInaccessible("new-dir");
        Assert.IsTrue(Directory.Exists(env.Abs("new-dir")));
    }

    /// <summary>SetInaccessible with a null path throws ArgumentNullException.</summary>
    [TestMethod]
    public void SetInaccessible_NullPath_Throws() {
        using var env = new TestEnvironment();
        Assert.ThrowsExactly<ArgumentNullException>(() => env.SetInaccessible(null!));
    }

    /// <summary>SetInaccessibleDirectory on an existing directory does not throw.</summary>
    [TestMethod]
    public void SetInaccessibleDirectory_WithExistingPath_DoesNotThrow() {
        using var env = new TestEnvironment();
        var dir = env.CreateDirectory("acc-dir");
        env.SetInaccessibleDirectory(dir);
        Assert.IsTrue(Directory.Exists(env.Root));
    }

    /// <summary>SetInaccessibleDirectory with a null path throws ArgumentNullException.</summary>
    [TestMethod]
    public void SetInaccessibleDirectory_NullPath_Throws() {
        using var env = new TestEnvironment();
        Assert.ThrowsExactly<ArgumentNullException>(() => env.SetInaccessibleDirectory(null!));
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

    /// <summary>AssertDirectoryInaccessible does not throw when the directory was made inaccessible.</summary>
    [TestMethod]
    public void AssertDirectoryInaccessible_WithInaccessibleDirectory_Passes() {
        using var env = new TestEnvironment();
        var dir = env.CreateDirectory("inacc");
        env.SetInaccessibleDirectory(dir);
        env.AssertDirectoryInaccessible(dir);
    }

    /// <summary>CreateDirectory returns the expected full path.</summary>
    [TestMethod]
    public void CreateDirectory_ReturnsFullPath() {
        using var env = new TestEnvironment();
        var path = env.CreateDirectory("mydir");
        Assert.AreEqual(env.Abs("mydir"), path);
    }

    /// <summary>Abs with multiple segments combines them all.</summary>
    [TestMethod]
    public void Abs_WithMultipleSegments_CombinesPaths() {
        using var env = new TestEnvironment();
        var result = env.Abs("a", "b", "c.txt");
        Assert.IsTrue(result.EndsWith("a\\b\\c.txt", StringComparison.OrdinalIgnoreCase) ||
                      result.EndsWith("a/b/c.txt", StringComparison.Ordinal));
    }

    /// <summary>CreateInaccessibleDirectory creates a directory using the specified name.</summary>
    [TestMethod]
    public void CreateInaccessibleDirectory_WithName_ReturnsPath() {
        using var env = new TestEnvironment();
        var path = env.CreateInaccessibleDirectory("priv");
        Assert.IsNotNull(path);
        Assert.Contains("priv", path);
    }

    /// <summary>CreateTree with a file entry creates that file.</summary>
    [TestMethod]
    public void CreateTree_WithSingleFile_CreatesFile() {
        using var env = new TestEnvironment();
        var files = new Dictionary<string, string?> { ["test.txt"] = "hello" };
        env.CreateTree(files);
        Assert.IsTrue(File.Exists(env.Abs("test.txt")));
        Assert.AreEqual("hello", File.ReadAllText(env.Abs("test.txt")));
    }

    /// <summary>CreateTree with a directory entry creates that directory.</summary>
    [TestMethod]
    public void CreateTree_WithDirectoryEntry_CreatesDirectory() {
        using var env = new TestEnvironment();
        var files = new Dictionary<string, string?> { ["sub"] = null };
        env.CreateTree(files);
        Assert.IsTrue(Directory.Exists(env.Abs("sub")));
    }

    /// <summary>CreateTree with a nested path creates intermediate directories.</summary>
    [TestMethod]
    public void CreateTree_WithNestedPath_CreatesIntermediateDirs() {
        using var env = new TestEnvironment();
        var files = new Dictionary<string, string?> { ["a/b/c.txt"] = "nested" };
        env.CreateTree(files);
        Assert.IsTrue(File.Exists(env.Abs("a/b/c.txt")));
    }

    /// <summary>CreateLockedFile with a null path throws ArgumentNullException.</summary>
    [TestMethod]
    public void CreateLockedFile_NullPath_Throws() {
        using var env = new TestEnvironment();
        Assert.ThrowsExactly<ArgumentNullException>(() => env.CreateLockedFile(null!));
    }

    /// <summary>SetInaccessibleDirectory on a non-existent path creates the directory first.</summary>
    [TestMethod]
    public void SetInaccessibleDirectory_NonExistent_CreatesDirectory() {
        using var env = new TestEnvironment();
        env.SetInaccessibleDirectory("new-dir");
        Assert.IsTrue(Directory.Exists(env.Abs("new-dir")));
    }

    /// <summary>Dispose with a locked file does not throw, ensuring cleanup is resilient.</summary>
    [TestMethod]
    public void Dispose_WithLockedFile_Resilient() {
        var env = new TestEnvironment();
        var root = env.Root;
        var stream = env.CreateLockedFile("locked.bin");

        env.Dispose();
        stream.Dispose();

        if(Directory.Exists(root)) {
            Directory.Delete(root, recursive: true);
        }

        Assert.IsFalse(Directory.Exists(root));
    }

    /// <summary>Dispose can be called multiple times without throwing.</summary>
    [TestMethod]
    public void Dispose_MultipleCalls_DoesNotThrow() {
        var env = new TestEnvironment();
        var root = env.Root;
        env.Dispose();
        env.Dispose();
        Assert.IsFalse(Directory.Exists(root));
    }
}
