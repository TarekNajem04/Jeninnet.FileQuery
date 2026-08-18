//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.IO;

/// <summary>
/// Contains unit tests for the <see cref="FileSystemGuards"/> class.
/// </summary>
[TestClass]
public sealed class FileSystemGuardsTests {
    /// <summary>Tests EnsureAccessible_ShouldNotThrow_WhenDirectoryIsAccessible.</summary>
    [TestMethod]
    public void EnsureAccessible_ShouldNotThrow_WhenDirectoryIsAccessible() {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try {
            FileSystemGuards.EnsureAccessible(tempDir, false);
            // Assert: If we reached here, no exception was thrown, which is correct.
            Assert.IsTrue(Directory.Exists(tempDir));
        }
        finally {
            Directory.Delete(tempDir);
        }
    }
}
