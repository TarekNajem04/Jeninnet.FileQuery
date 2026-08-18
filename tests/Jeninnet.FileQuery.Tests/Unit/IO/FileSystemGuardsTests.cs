namespace Jeninnet.FileQuery.Tests.Unit.IO;

/// <summary>
/// Tests for FileSystemGuardsTests.
/// </summary>
[TestClass]
public sealed class FileSystemGuardsTests {
    /// <summary>
    /// Verifies that Should NotThrow When DirectoryIsAccessible.
    /// </summary>
    [TestMethod]
    public void Should_NotThrow_When_DirectoryIsAccessible() {
        using var env = new TestEnvironment();
        FileSystemGuards.EnsureAccessible(env.Root, false);
        // Assert: If we reached here, no exception was thrown, which is correct.
        Assert.IsTrue(Directory.Exists(env.Root));
    }
}
