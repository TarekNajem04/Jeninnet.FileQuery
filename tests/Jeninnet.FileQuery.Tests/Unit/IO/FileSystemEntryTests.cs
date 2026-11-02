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

        entry.IsReadOnly.Should().BeTrue();
        entry.IsHidden.Should().BeTrue();
        entry.IsDirectory.Should().BeTrue();
        entry.IsSystem.Should().BeFalse();
        entry.IsArchive.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that Should ReturnTrueForActiveAttribute When HasAttributeChecked.
    /// </summary>
    [TestMethod]
    public void Should_ReturnTrueForActiveAttribute_When_HasAttributeChecked() {
        var entry = new FileSystemEntry("test", FileAttributes.Encrypted);

        entry.HasAttribute(FileAttributes.Encrypted).Should().BeTrue();
        entry.HasAttribute(FileAttributes.ReadOnly).Should().BeFalse();
    }

    /// <summary>
    /// Verifies that Should FlagActiveAttributes When Set.
    /// </summary>
    [TestMethod]
    public void Should_FlagActiveAttributes_When_Set() {
        var entry = new FileSystemEntry("test", FileAttributes.ReadOnly | FileAttributes.Hidden);
        var active = entry.ActiveAttributes.ToList();

        active.Should().Contain(FileAttributes.ReadOnly);
        active.Should().Contain(FileAttributes.Hidden);
        active.Should().HaveCount(2);
    }
}
