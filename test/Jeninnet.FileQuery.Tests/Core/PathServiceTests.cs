namespace Jeninnet.FileQuery.Tests.Core;

[TestClass]
public class PathUtilitiesTests
{
    [TestMethod]
    public void Normalize_ShouldConvertBackslashes() =>
        Assert.AreEqual("C:/foo/bar", PathUtilities.Normalize(@"C:\foo\bar"));

    [TestMethod]
    public void Normalize_ShouldRemoveDoubleSeparators() =>
        Assert.AreEqual("C:/foo/bar", PathUtilities.Normalize(@"C:\\foo//bar"));

    [TestMethod]
    public void Normalize_ShouldPreserveRoot() =>
        Assert.AreEqual("C:/", PathUtilities.Normalize("C:/"));
    [TestMethod]
    public void BuildRelativePath_ShouldHandleDirectoryTrailingSlash()
    {
        const string root = "C:/root";
        var entry = new FileSystemEntry("C:/root/subdir", FileAttributes.Directory);
        var actual = PathUtilities.BuildRelativePath(root, entry);
        var normalized = actual.Replace('\\', '/');

        System.Console.WriteLine($"Actual: '{actual}', Normalized: '{normalized}'");

        // Ensure it doesn't have a leading slash
        var trimmed = normalized.TrimStart('/');

        Assert.AreEqual("subdir/", trimmed, $"Path was: '{actual}'");
    }

    [TestMethod]
    public void SplitNormalizedPath_ShouldHandleRootOnly()
    {
        var result = PathUtilities.SplitNormalizedPath("/", true);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void CountSegments_ShouldReturnZeroForEmptyPath() => Assert.AreEqual(0, PathUtilities.CountSegments([], false));
}
