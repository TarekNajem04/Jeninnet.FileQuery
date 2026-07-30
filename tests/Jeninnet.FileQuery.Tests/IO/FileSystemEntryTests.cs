namespace Jeninnet.FileQuery.Tests.IO;

/// <summary>
/// Contains unit tests for the <see cref="FileSystemEntry"/> class.
/// </summary>
[TestClass]
public class FileSystemEntryTests {
    /// <summary>Tests FileSystemEntry_Properties_TestAllAttributes.</summary>
    [TestMethod]
    public void FileSystemEntry_Properties_TestAllAttributes() {
        var entry = new FileSystemEntry("test", FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.Directory);

        Assert.IsTrue(entry.IsReadOnly);
        Assert.IsTrue(entry.IsHidden);
        Assert.IsTrue(entry.IsDirectory);
        Assert.IsFalse(entry.IsSystem);
        Assert.IsFalse(entry.IsArchive);
    }

    /// <summary>Tests FileSystemEntry_HasAttribute_Test.</summary>
    [TestMethod]
    public void FileSystemEntry_HasAttribute_Test() {
        var entry = new FileSystemEntry("test", FileAttributes.Encrypted);

        Assert.IsTrue(entry.HasAttribute(FileAttributes.Encrypted));
        Assert.IsFalse(entry.HasAttribute(FileAttributes.ReadOnly));
    }

    /// <summary>Tests FileSystemEntry_ActiveAttributes_Test.</summary>
    [TestMethod]
    public void FileSystemEntry_ActiveAttributes_Test() {
        var entry = new FileSystemEntry("test", FileAttributes.ReadOnly | FileAttributes.Hidden);
        var active = entry.ActiveAttributes.ToList();

        Assert.Contains(FileAttributes.ReadOnly, active);
        Assert.Contains(FileAttributes.Hidden, active);
        Assert.HasCount(2, active);
    }
}

