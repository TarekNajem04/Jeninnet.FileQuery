namespace Jeninnet.Testing.Assertions.Tests.IO;

[TestClass]
public sealed class TestEnvironmentSmokeTests {
    [TestMethod]
    public void CreateAndDispose_DoesNotThrow() {
        var env = new TestEnvironment();
        env.Dispose();
        Assert.IsTrue(true);
    }

    [TestMethod]
    public void Root_ExistsAfterCreation() {
        using var env = new TestEnvironment();
        Assert.IsTrue(Directory.Exists(env.Root));
    }

    [TestMethod]
    public void Root_DeletedAfterDispose() {
        string root;
        var env = new TestEnvironment();
        root = env.Root;
        env.Dispose();
        Assert.IsFalse(Directory.Exists(root));
    }

    [TestMethod]
    public void CreateDirectory_ReturnsExistingPath() {
        using var env = new TestEnvironment();
        var dir = env.CreateDirectory("sub/test");
        Assert.IsTrue(Directory.Exists(dir));
    }

    [TestMethod]
    public void CreateFile_WithContent_ReturnsPath() {
        using var env = new TestEnvironment();
        var file = env.CreateFile("test.txt", "content");
        Assert.IsTrue(File.Exists(file));
        Assert.AreEqual("content", File.ReadAllText(file));
    }
}
