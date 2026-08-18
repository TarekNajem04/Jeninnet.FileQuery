namespace Jeninnet.FileQuery.Tests.Unit.Path;

/// <summary>
/// Tests for PathUtilitiesTests.
/// </summary>
[TestClass]
public class PathUtilitiesTests {
    /// <summary>
    /// Verifies that Should ConvertBackslashes When Normalized.
    /// </summary>
    [TestMethod]
    public void Should_ConvertBackslashes_When_Normalized() => Assert.AreEqual("C:/foo/bar", PathUtilities.Normalize(@"C:\foo\bar"));

    /// <summary>
    /// Verifies that Should RemoveDoubleSeparators When Normalized.
    /// </summary>
    [TestMethod]
    public void Should_RemoveDoubleSeparators_When_Normalized() => Assert.AreEqual("C:/foo/bar", PathUtilities.Normalize(@"C:\\foo//bar"));

    /// <summary>
    /// Verifies that Should PreserveRoot When Normalized.
    /// </summary>
    [TestMethod]
    public void Should_PreserveRoot_When_Normalized() => Assert.AreEqual("C:/", PathUtilities.Normalize("C:/"));

    /// <summary>
    /// Verifies that Should HandleTrailingSlash When BuildingRelativePath.
    /// </summary>
    [TestMethod]
    public void Should_HandleTrailingSlash_When_BuildingRelativePath() {
        const string root = "C:/root";
        var entry = new FileSystemEntry("C:/root/subdir", FileAttributes.Directory);
        var actual = PathUtilities.BuildRelativePath(root, entry);
        var normalized = PathUtilities.Normalize(actual, trimTrailingSlash: false);

        Console.WriteLine($"Actual: '{actual}', Normalized: '{normalized}'");

        // Ensure it doesn't have a leading slash
        var trimmed = normalized.TrimStart('/');

        Assert.AreEqual("subdir/", trimmed, $"Path was: '{actual}'");
    }

    /// <summary>
    /// Verifies that Should HandleRootOnly When SplittingNormalizedPath.
    /// </summary>
    [TestMethod]
    public void Should_HandleRootOnly_When_SplittingNormalizedPath() {
        var result = PathUtilities.SplitNormalizedPath("/", true);
        Assert.IsEmpty(result);
    }

    /// <summary>
    /// Verifies that Should ReturnZero When CountingSegmentsOfEmptyPath.
    /// </summary>
    [TestMethod]
    public void Should_ReturnZero_When_CountingSegmentsOfEmptyPath() => Assert.AreEqual(0, PathUtilities.CountSegments([], false));
}
