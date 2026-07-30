namespace Jeninnet.Testing.Assertions.Tests.IO;

/// <summary>Verifies <see cref="InaccessibleDirectorySimulator"/> methods including null guards.</summary>
[TestClass]
public sealed class InaccessibleDirectorySimulatorTests {
    /// <summary>CreatePseudoInaccessibleDir creates a file that simulates an inaccessible directory.</summary>
    [TestMethod]
    public void CreatePseudoInaccessibleDir_ReturnsExistingFile() {
        using var env = new TestEnvironment();
        var path = InaccessibleDirectorySimulator.CreatePseudoInaccessibleDir(env, "pseudo");
        Assert.IsTrue(File.Exists(path));
    }

    /// <summary>CreatePseudoInaccessibleDir with a null environment throws ArgumentNullException.</summary>
    [TestMethod]
    public void CreatePseudoInaccessibleDir_NullEnvironment_Throws() => Assert.ThrowsExactly<ArgumentNullException>(
            static () => InaccessibleDirectorySimulator.CreatePseudoInaccessibleDir(null!, "x"));

    /// <summary>CreatePseudoInaccessibleDir with a null name throws ArgumentNullException.</summary>
    [TestMethod]
    public void CreatePseudoInaccessibleDir_NullName_Throws() {
        using var env = new TestEnvironment();
        Assert.ThrowsExactly<ArgumentNullException>(
            () => InaccessibleDirectorySimulator.CreatePseudoInaccessibleDir(env, null!));
    }

    /// <summary>CreateLockedDir creates a directory with a locked file inside.</summary>
    [TestMethod]
    public void CreateLockedDir_ReturnsExistingDirectory() {
        using var env = new TestEnvironment();
        var path = InaccessibleDirectorySimulator.CreateLockedDir(env, "locked", out var handle);
        using(handle) {
            Assert.IsTrue(Directory.Exists(path));
        }
    }

    /// <summary>CreateLockedDir with a null environment throws ArgumentNullException.</summary>
    [TestMethod]
    public void CreateLockedDir_NullEnvironment_Throws() => Assert.ThrowsExactly<ArgumentNullException>(
            static () => InaccessibleDirectorySimulator.CreateLockedDir(null!, "x", out _));

    /// <summary>CreateLockedDir with a null name throws ArgumentNullException.</summary>
    [TestMethod]
    public void CreateLockedDir_NullName_Throws() {
        using var env = new TestEnvironment();
        Assert.ThrowsExactly<ArgumentNullException>(
            () => InaccessibleDirectorySimulator.CreateLockedDir(env, null!, out _));
    }
}
