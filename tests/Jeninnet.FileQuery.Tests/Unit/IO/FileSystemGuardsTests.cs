//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
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
