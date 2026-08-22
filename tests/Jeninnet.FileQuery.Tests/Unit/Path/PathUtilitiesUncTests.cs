//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Unit.Path;

/// <summary>
/// Tests for UNC path normalization in <see cref="PathUtilities"/>.
/// </summary>
/// <remarks>
/// UNC paths have the form <c>\\server\share\path</c> on Windows.
/// After normalization they must produce <c>//server/share/path</c>.
/// The leading double-slash must be preserved; collapsing it to a single slash
/// produces an invalid path that matchers cannot evaluate correctly.
/// </remarks>
[TestClass]
public sealed class PathUtilitiesUncTests {

    /// <summary>
    /// Verifies that Should PreserveLeadingDoubleSlash When UncPathNormalizedWithoutTrailingSlash.
    /// </summary>
    [TestMethod]
    public void Should_PreserveLeadingDoubleSlash_When_UncPathNormalizedWithoutTrailingSlash() {
        const string input = @"\\server\share\file.txt";
        const string expected = "//server/share/file.txt";

        Assert.AreEqual(expected, PathUtilities.Normalize(PathUtilities.Normalize(input)),
            "Backslashes must become forward slashes and '//' must not be collapsed to '/'.");
    }

    /// <summary>
    /// Verifies that Should PreserveTrailingSlash When UncRootNormalizedWithTrailingSlash.
    /// </summary>
    [TestMethod]
    public void Should_PreserveTrailingSlash_When_UncRootNormalizedWithTrailingSlash() {
        const string input = @"\\server\share\";
        const string expected = "//server/share/";

        Assert.AreEqual(expected, PathUtilities.Normalize(input),
            "A UNC root path's trailing slash must be preserved.");
    }

    /// <summary>
    /// Verifies that Should NotAddSlash When UncRootNormalizedWithoutTrailingSlash.
    /// </summary>
    [TestMethod]
    public void Should_NotAddSlash_When_UncRootNormalizedWithoutTrailingSlash() {
        const string input = @"\\server\share";
        const string expected = "//server/share";

        Assert.AreEqual(expected, PathUtilities.Normalize(input));
    }

    /// <summary>
    /// Verifies that Should TrimTrailingSlash When UncPathBelowRootNormalized.
    /// </summary>
    [TestMethod]
    public void Should_TrimTrailingSlash_When_UncPathBelowRootNormalized() {
        const string input = "//server/share/folder/";
        const string expected = "//server/share/folder";

        Assert.AreEqual(expected, PathUtilities.Normalize(input),
            "Trailing slash on a non-root UNC path must be trimmed.");
    }

    /// <summary>
    /// Verifies that Should NormalizeCorrectly When DeepUncPath.
    /// </summary>
    [TestMethod]
    public void Should_NormalizeCorrectly_When_DeepUncPath() {
        const string input = @"\\server\share\project\src\Program.cs";
        const string expected = "//server/share/project/src/Program.cs";

        Assert.AreEqual(expected, PathUtilities.Normalize(input));
    }

    /// <summary>
    /// Verifies that Should CollapseDuplicateSlashes When UncPathNormalized.
    /// </summary>
    [TestMethod]
    public void Should_CollapseDuplicateSlashes_When_UncPathNormalized() {
        const string input = "//server//share//file.txt";
        const string expected = "//server/share/file.txt";

        Assert.AreEqual(expected, PathUtilities.Normalize(input),
            "Internal consecutive slashes must be collapsed; leading '//' must survive.");
    }

    /// <summary>
    /// Verifies that Should PreserveSlash When LocalPathNormalizedWithTrimFalse.
    /// </summary>
    [TestMethod]
    public void Should_PreserveSlash_When_LocalPathNormalizedWithTrimFalse() {
        const string input = @"C:\Users\Test\";
        const string expected = "C:/Users/Test/";

        Assert.AreEqual(expected, PathUtilities.Normalize(input, trimTrailingSlash: false),
            "trimTrailingSlash: false must preserve a trailing slash on non-root paths.");
    }

    /// <summary>
    /// Verifies that Should PreserveSlash When UncNonRootPathNormalizedWithTrimFalse.
    /// </summary>
    [TestMethod]
    public void Should_PreserveSlash_When_UncNonRootPathNormalizedWithTrimFalse() {
        const string input = @"\\server\share\folder\";
        const string expected = "//server/share/folder/";

        Assert.AreEqual(expected, PathUtilities.Normalize(input, trimTrailingSlash: false),
            "trimTrailingSlash: false must preserve trailing slash on non-root UNC paths.");
    }

    /// <summary>
    /// Verifies that Should TrimTrailingSlash When DefaultBehavior.
    /// </summary>
    [TestMethod]
    public void Should_TrimTrailingSlash_When_DefaultBehavior() {
        const string input = @"C:\Users\Test\";
        const string expected = "C:/Users/Test";

        Assert.AreEqual(expected, PathUtilities.Normalize(input),
            "Default behavior (trimTrailingSlash: true) must not be altered by adding the optional parameter.");
    }

    /// <summary>
    /// Verifies that Should ConvertBackslashes When WindowsPathNormalized.
    /// </summary>
    [TestMethod]
    public void Should_ConvertBackslashes_When_WindowsPathNormalized() =>
        Assert.AreEqual(
            "C:/Users/Test/file.txt",
            PathUtilities.Normalize(@"C:\Users\Test\file.txt"));

    /// <summary>
    /// Verifies that Should CollapseDuplicateSlashes When LocalPathNormalized.
    /// </summary>
    [TestMethod]
    public void Should_CollapseDuplicateSlashes_When_LocalPathNormalized() =>
        Assert.AreEqual(
            "C:/Users/Test/file.txt",
            PathUtilities.Normalize(@"C:\\Users//Test\\file.txt"));

    /// <summary>
    /// Verifies that Should PreserveTrailingSlash When DriveRootNormalized.
    /// </summary>
    [TestMethod]
    public void Should_PreserveTrailingSlash_When_DriveRootNormalized() =>
        Assert.AreEqual("C:/", PathUtilities.Normalize("C:/"));

    /// <summary>
    /// Verifies that Should ThrowArgumentException When NormalizingNullOrEmpty.
    /// </summary>
    [TestMethod]
    public void Should_ThrowArgumentException_When_NormalizingNullOrEmpty() {
        Assert.ThrowsExactly<ArgumentNullException>(static () => PathUtilities.Normalize(null));
        Assert.ThrowsExactly<ArgumentException>(static () => PathUtilities.Normalize(""));
    }
}
