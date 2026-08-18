//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Unit.IO;

/// <summary>
/// Tests for FileSystemEntryTests.
/// </summary>
[TestClass]
public class FileSystemEntryTests {
    /// <summary>
    /// Verifies that Should SetAllAttributes When FileSystemEntryCreated.
    /// </summary>
    [TestMethod]
    public void Should_SetAllAttributes_When_FileSystemEntryCreated() {
        var entry = new FileSystemEntry("test", FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.Directory);

        Assert.IsTrue(entry.IsReadOnly);
        Assert.IsTrue(entry.IsHidden);
        Assert.IsTrue(entry.IsDirectory);
        Assert.IsFalse(entry.IsSystem);
        Assert.IsFalse(entry.IsArchive);
    }

    /// <summary>
    /// Verifies that Should ReturnTrueForActiveAttribute When HasAttributeChecked.
    /// </summary>
    [TestMethod]
    public void Should_ReturnTrueForActiveAttribute_When_HasAttributeChecked() {
        var entry = new FileSystemEntry("test", FileAttributes.Encrypted);

        Assert.IsTrue(entry.HasAttribute(FileAttributes.Encrypted));
        Assert.IsFalse(entry.HasAttribute(FileAttributes.ReadOnly));
    }

    /// <summary>
    /// Verifies that Should FlagActiveAttributes When Set.
    /// </summary>
    [TestMethod]
    public void Should_FlagActiveAttributes_When_Set() {
        var entry = new FileSystemEntry("test", FileAttributes.ReadOnly | FileAttributes.Hidden);
        var active = entry.ActiveAttributes.ToList();

        Assert.Contains(FileAttributes.ReadOnly, active);
        Assert.Contains(FileAttributes.Hidden, active);
        Assert.HasCount(2, active);
    }
}
