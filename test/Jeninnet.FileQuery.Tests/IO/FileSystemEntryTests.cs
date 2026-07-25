namespace Jeninnet.FileQuery.Tests.IO;

[TestClass]
public class FileSystemEntryTests {
    [TestMethod]
    public void FileSystemEntry_Properties_TestAllAttributes() {
        var entry = new FileSystemEntry("test", FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.Directory);

        Assert.IsTrue(entry.IsReadOnly);
        Assert.IsTrue(entry.IsHidden);
        Assert.IsTrue(entry.IsDirectory);
        Assert.IsFalse(entry.IsSystem);
        Assert.IsFalse(entry.IsArchive);
    }

    [TestMethod]
    public void FileSystemEntry_HasAttribute_Test() {
        var entry = new FileSystemEntry("test", FileAttributes.Encrypted);

        Assert.IsTrue(entry.HasAttribute(FileAttributes.Encrypted));
        Assert.IsFalse(entry.HasAttribute(FileAttributes.ReadOnly));
    }

    [TestMethod]
    public void FileSystemEntry_ActiveAttributes_Test() {
        var entry = new FileSystemEntry("test", FileAttributes.ReadOnly | FileAttributes.Hidden);
        var active = entry.ActiveAttributes.ToList();

        Assert.Contains(FileAttributes.ReadOnly, active);
        Assert.Contains(FileAttributes.Hidden, active);
        Assert.HasCount(2, active);
    }
}
