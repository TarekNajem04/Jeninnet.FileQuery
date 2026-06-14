namespace Jeninnet.FileQuery.Tests.IO;

[TestClass]
public sealed class FileSystemGuardsTests
{
    [TestMethod]
    public void EnsureAccessible_ShouldNotThrow_WhenDirectoryIsAccessible()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            FileSystemGuards.EnsureAccessible(tempDir, false);
            // Assert: If we reached here, no exception was thrown, which is correct.
            Assert.IsTrue(Directory.Exists(tempDir));
        }
        finally
        {
            Directory.Delete(tempDir);
        }
    }
}
