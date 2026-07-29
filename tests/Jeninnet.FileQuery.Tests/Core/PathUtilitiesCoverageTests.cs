namespace Jeninnet.FileQuery.Tests.Core;

/// <summary>
/// Contains coverage tests for the <see cref="PathUtilities"/> class.
/// </summary>
[TestClass]
public class PathUtilitiesCoverageTests {
    /// <summary>Tests PathUtilities_Normalize_NullOrEmpty_Throws.</summary>
    [TestMethod]
    public void PathUtilities_Normalize_NullOrEmpty_Throws() {
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

    /// <summary>Tests PathUtilities_ToForward_Works.</summary>
    [TestMethod]
    public void PathUtilities_ToForward_Works() => Assert.AreEqual("a/b/c", PathUtilities.ToForward("a\\b\\c"));

    /// <summary>Tests PathUtilities_UncRoot_EdgeCases.</summary>
    [TestMethod]
    public void PathUtilities_UncRoot_EdgeCases() {
        // UNC roots should keep their trailing slash
        Assert.AreEqual("//server/share/", PathUtilities.Normalize("//server/share/", true));
        Assert.AreEqual("//server/share", PathUtilities.Normalize("//server/share", true));

        // Not a root, should be trimmed
        Assert.AreEqual("//server/share/folder", PathUtilities.Normalize("//server/share/folder/", true));

        // Invalid UNC (no share)
        Assert.AreEqual("//server", PathUtilities.Normalize("//server", true));
    }

    /// <summary>Tests PathUtilities_CountSegments_EdgeCases.</summary>
    [TestMethod]
    public void PathUtilities_CountSegments_EdgeCases() {
        Assert.AreEqual(0, PathUtilities.CountSegments("".AsSpan(), false));
        Assert.AreEqual(1, PathUtilities.CountSegments("a".AsSpan(), false));
        Assert.AreEqual(2, PathUtilities.CountSegments("a/b".AsSpan(), false));
        Assert.AreEqual(2, PathUtilities.CountSegments("a/b/".AsSpan(), true));
    }

    /// <summary>Tests PathUtilities_SplitNormalizedPath_EdgeCases.</summary>
    [TestMethod]
    public void PathUtilities_SplitNormalizedPath_EdgeCases() {
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

    /// <summary>Tests PathUtilities_BuildRelativePath_LargeBuffer.</summary>
    [TestMethod]
    public void PathUtilities_BuildRelativePath_LargeBuffer() {
        // Create a path longer than 256 chars to trigger the heap allocation path
        var longSegment = new string('a', 300);
        const string root = "C:/";
        var full = $"C:/{longSegment}/file.txt";
        var entry = new FileSystemEntry(full, FileAttributes.Normal);

        var relative = PathUtilities.BuildRelativePath(root, entry);
        Assert.AreEqual($"{longSegment}/file.txt", relative);
    }
}

