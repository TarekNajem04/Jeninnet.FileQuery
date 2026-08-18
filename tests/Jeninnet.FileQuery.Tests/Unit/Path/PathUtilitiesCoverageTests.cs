namespace Jeninnet.FileQuery.Tests.Unit.Path;

/// <summary>
/// Tests for PathUtilitiesCoverageTests.
/// </summary>
[TestClass]
public class PathUtilitiesCoverageTests {
    /// <summary>
    /// Verifies that Should Throw When NormalizingNullOrEmpty.
    /// </summary>
    [TestMethod]
    public void Should_Throw_When_NormalizingNullOrEmpty() {
        try {
            PathUtilities.Normalize(null);
            Assert.Fail("Should have thrown ArgumentException");
        }
        catch(ArgumentException) { /* Ignore */ }

        try {
            PathUtilities.Normalize("");
            Assert.Fail("Should have thrown ArgumentException");
        }
        catch(ArgumentException) { /* Ignore */ }
    }

    /// <summary>
    /// Verifies that Should ConvertToForwardSlash When ToForwardCalled.
    /// </summary>
    [TestMethod]
    public void Should_ConvertToForwardSlash_When_ToForwardCalled() => Assert.AreEqual("a/b/c", PathUtilities.ToForward("a\\b\\c"));

    /// <summary>
    /// Verifies that Should HandleUncRoot When Normalized.
    /// </summary>
    [TestMethod]
    public void Should_HandleUncRoot_When_Normalized() {
        // UNC roots should keep their trailing slash
        Assert.AreEqual("//server/share/", PathUtilities.Normalize("//server/share/", true));
        Assert.AreEqual("//server/share", PathUtilities.Normalize("//server/share", true));

        // Not a root, should be trimmed
        Assert.AreEqual("//server/share/folder", PathUtilities.Normalize("//server/share/folder/", true));

        // Invalid UNC (no share)
        Assert.AreEqual("//server", PathUtilities.Normalize("//server", true));
    }

    /// <summary>
    /// Verifies that Should HandleEdgeCases When CountingSegments.
    /// </summary>
    [TestMethod]
    public void Should_HandleEdgeCases_When_CountingSegments() {
        Assert.AreEqual(0, PathUtilities.CountSegments("".AsSpan(), false));
        Assert.AreEqual(1, PathUtilities.CountSegments("a".AsSpan(), false));
        Assert.AreEqual(2, PathUtilities.CountSegments("a/b".AsSpan(), false));
        Assert.AreEqual(2, PathUtilities.CountSegments("a/b/".AsSpan(), true));
    }

    /// <summary>
    /// Verifies that Should HandleEdgeCases When SplittingNormalizedPath.
    /// </summary>
    [TestMethod]
    public void Should_HandleEdgeCases_When_SplittingNormalizedPath() {
        var empty = PathUtilities.SplitNormalizedPath("".AsSpan(), false);
        Assert.IsEmpty(empty);

        var root = PathUtilities.SplitNormalizedPath("/".AsSpan(), false);
        Assert.IsEmpty(root);

        var single = PathUtilities.SplitNormalizedPath("a".AsSpan(), false);
        Assert.HasCount(1, single);
        Assert.AreEqual("a", single[0]);

        var multi = PathUtilities.SplitNormalizedPath("a/b/c".AsSpan(), false);
        Assert.HasCount(3, multi);
        Assert.AreEqual("a", multi[0]);
        Assert.AreEqual("b", multi[1]);
        Assert.AreEqual("c", multi[2]);
    }

    /// <summary>
    /// Verifies that Should HandleLargeBuffer When BuildingRelativePath.
    /// </summary>
    [TestMethod]
    public void Should_HandleLargeBuffer_When_BuildingRelativePath() {
        // Create a path longer than 256 chars to trigger the heap allocation path
        var longSegment = new string('a', 300);
        const string root = "C:/";
        var full = $"C:/{longSegment}/file.txt";
        var entry = new FileSystemEntry(full, FileAttributes.Normal);

        var relative = PathUtilities.BuildRelativePath(root, entry);
        Assert.AreEqual($"{longSegment}/file.txt", relative);
    }
}
