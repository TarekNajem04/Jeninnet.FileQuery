//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
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

        entry.IsReadOnly.Should().BeTrue();
        entry.IsHidden.Should().BeTrue();
        entry.IsDirectory.Should().BeTrue();
        entry.IsSystem.Should().BeFalse();
        entry.IsArchive.Should().BeFalse();
    }

    /// <summary>Tests FileSystemEntry_HasAttribute_Test.</summary>
    [TestMethod]
    public void FileSystemEntry_HasAttribute_Test() {
        var entry = new FileSystemEntry("test", FileAttributes.Encrypted);

        entry.HasAttribute(FileAttributes.Encrypted).Should().BeTrue();
        entry.HasAttribute(FileAttributes.ReadOnly).Should().BeFalse();
    }

    /// <summary>Tests FileSystemEntry_ActiveAttributes_Test.</summary>
    [TestMethod]
    public void FileSystemEntry_ActiveAttributes_Test() {
        var entry = new FileSystemEntry("test", FileAttributes.ReadOnly | FileAttributes.Hidden);
        var active = entry.ActiveAttributes.ToList();

        active.Should().Contain(FileAttributes.ReadOnly);
        active.Should().Contain(FileAttributes.Hidden);
        active.Should().HaveCount(2);
    }
}
