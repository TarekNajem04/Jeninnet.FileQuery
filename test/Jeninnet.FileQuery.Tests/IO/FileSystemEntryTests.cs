namespace Jeninnet.FileQuery.Tests.IO;

[TestClass]
public sealed class FileSystemEntryTests
{
    [TestMethod]
    public void Properties_ShouldReflectAttributesCorrectly()
    {
        var entry = new FileSystemEntry("C:/test.txt", FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.Directory);

        Assert.IsTrue(entry.IsReadOnly);
        Assert.IsTrue(entry.IsHidden);
        Assert.IsTrue(entry.IsDirectory);
        Assert.AreEqual(PathKind.Directory, entry.PathKind);
        Assert.IsFalse(entry.IsSystem);
    }

    [TestMethod]
    public void ActiveAttributes_ReturnsCorrectAttributes()
    {
        var entry = new FileSystemEntry("C:/test.txt", FileAttributes.ReadOnly | FileAttributes.Hidden);
        var active = entry.ActiveAttributes.ToList();

        Assert.Contains(FileAttributes.ReadOnly, active);
        Assert.Contains(FileAttributes.Hidden, active);
        Assert.HasCount(2, active);
    }
}
