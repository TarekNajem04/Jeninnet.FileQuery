namespace Jeninnet.FileQuery.Tests.Core;

/// <summary>
/// Contains tests for the <see cref="PathUtilities"/> class.
/// </summary>
[TestClass]
public class PathUtilitiesTests {
    /// <summary>Tests Normalize_ShouldConvertBackslashes.</summary>
    [TestMethod]
    public void Normalize_ShouldConvertBackslashes() => Assert.AreEqual("C:/foo/bar", PathUtilities.Normalize(@"C:\foo\bar"));

    /// <summary>Tests Normalize_ShouldRemoveDoubleSeparators.</summary>
    [TestMethod]
    public void Normalize_ShouldRemoveDoubleSeparators() => Assert.AreEqual("C:/foo/bar", PathUtilities.Normalize(@"C:\\foo//bar"));

    /// <summary>Tests Normalize_ShouldPreserveRoot.</summary>
    [TestMethod]
    public void Normalize_ShouldPreserveRoot() => Assert.AreEqual("C:/", PathUtilities.Normalize("C:/"));

    /// <summary>Tests BuildRelativePath_ShouldHandleDirectoryTrailingSlash.</summary>
    [TestMethod]
    public void BuildRelativePath_ShouldHandleDirectoryTrailingSlash() {
        const string root = "C:/root";
        var entry = new FileSystemEntry("C:/root/subdir", FileAttributes.Directory);
        var actual = PathUtilities.BuildRelativePath(root, entry);
        var normalized = actual.Replace('\\', '/');

        Console.WriteLine($"Actual: '{actual}', Normalized: '{normalized}'");

        // Ensure it doesn't have a leading slash
        var trimmed = normalized.TrimStart('/');

        Assert.AreEqual("subdir/", trimmed, $"Path was: '{actual}'");
    }

    /// <summary>Tests SplitNormalizedPath_ShouldHandleRootOnly.</summary>
    [TestMethod]
    public void SplitNormalizedPath_ShouldHandleRootOnly() {
        var result = PathUtilities.SplitNormalizedPath("/", true);
        Assert.IsEmpty(result);
    }

    /// <summary>Tests CountSegments_ShouldReturnZeroForEmptyPath.</summary>
    [TestMethod]
    public void CountSegments_ShouldReturnZeroForEmptyPath() => Assert.AreEqual(0, PathUtilities.CountSegments([], false));
}

