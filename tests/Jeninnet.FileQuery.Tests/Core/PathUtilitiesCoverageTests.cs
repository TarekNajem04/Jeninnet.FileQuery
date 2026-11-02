//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Core;

/// <summary>
/// Contains coverage tests for the <see cref="PathUtilities"/> class.
/// </summary>
[TestClass]
public class PathUtilitiesCoverageTests {
    /// <summary>Tests PathUtilities_Normalize_NullOrEmpty_Throws.</summary>
    [TestMethod]
    public void PathUtilities_Normalize_NullOrEmpty_Throws() {
        FluentActions.Invoking(() => PathUtilities.Normalize(null)).Should().Throw<ArgumentException>();
        FluentActions.Invoking(() => PathUtilities.Normalize("")).Should().Throw<ArgumentException>();
    }

    /// <summary>Tests PathUtilities_ToForward_Works.</summary>
    [TestMethod]
    public void PathUtilities_ToForward_Works() => PathUtilities.ToForward("a\\b\\c").Should().Be("a/b/c");

    /// <summary>Tests PathUtilities_UncRoot_EdgeCases.</summary>
    [TestMethod]
    public void PathUtilities_UncRoot_EdgeCases() {
        // UNC roots should keep their trailing slash
        (PathUtilities.Normalize("//server/share/", true)).Should().Be("//server/share/");
        (PathUtilities.Normalize("//server/share", true)).Should().Be("//server/share");

        // Not a root, should be trimmed
        (PathUtilities.Normalize("//server/share/folder/", true)).Should().Be("//server/share/folder");

        // Invalid UNC (no share)
        (PathUtilities.Normalize("//server", true)).Should().Be("//server");
    }

    /// <summary>Tests PathUtilities_CountSegments_EdgeCases.</summary>
    [TestMethod]
    public void PathUtilities_CountSegments_EdgeCases() {
        (PathUtilities.CountSegments("".AsSpan(), false)).Should().Be(0);
        (PathUtilities.CountSegments("a".AsSpan(), false)).Should().Be(1);
        (PathUtilities.CountSegments("a/b".AsSpan(), false)).Should().Be(2);
        (PathUtilities.CountSegments("a/b/".AsSpan(), true)).Should().Be(2);
    }

    /// <summary>Tests PathUtilities_SplitNormalizedPath_EdgeCases.</summary>
    [TestMethod]
    public void PathUtilities_SplitNormalizedPath_EdgeCases() {
        var empty = PathUtilities.SplitNormalizedPath("".AsSpan(), false);
        empty.Should().BeEmpty();

        var root = PathUtilities.SplitNormalizedPath("/".AsSpan(), false);
        root.Should().BeEmpty();

        var single = PathUtilities.SplitNormalizedPath("a".AsSpan(), false);
        single.Should().HaveCount(1);
        single[0].Should().Be("a");

        var multi = PathUtilities.SplitNormalizedPath("a/b/c".AsSpan(), false);
        multi.Should().HaveCount(3);
        multi[0].Should().Be("a");
        multi[1].Should().Be("b");
        multi[2].Should().Be("c");
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
        relative.Should().Be($"{longSegment}/file.txt");
    }
}
