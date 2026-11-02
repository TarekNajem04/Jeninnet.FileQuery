//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
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
        FluentActions.Invoking(() => PathUtilities.Normalize(null)).Should().Throw<ArgumentException>();
        FluentActions.Invoking(() => PathUtilities.Normalize("")).Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that Should ConvertToForwardSlash When ToForwardCalled.
    /// </summary>
    [TestMethod]
    public void Should_ConvertToForwardSlash_When_ToForwardCalled() => PathUtilities.ToForward("a\\b\\c").Should().Be("a/b/c");

    /// <summary>
    /// Verifies that Should HandleUncRoot When Normalized.
    /// </summary>
    [TestMethod]
    public void Should_HandleUncRoot_When_Normalized() {
        // UNC roots should keep their trailing slash
        (PathUtilities.Normalize("//server/share/", true)).Should().Be("//server/share/");
        (PathUtilities.Normalize("//server/share", true)).Should().Be("//server/share");

        // Not a root, should be trimmed
        (PathUtilities.Normalize("//server/share/folder/", true)).Should().Be("//server/share/folder");

        // Invalid UNC (no share)
        (PathUtilities.Normalize("//server", true)).Should().Be("//server");
    }

    /// <summary>
    /// Verifies that Should HandleEdgeCases When CountingSegments.
    /// </summary>
    [TestMethod]
    public void Should_HandleEdgeCases_When_CountingSegments() {
        (PathUtilities.CountSegments("".AsSpan(), false)).Should().Be(0);
        (PathUtilities.CountSegments("a".AsSpan(), false)).Should().Be(1);
        (PathUtilities.CountSegments("a/b".AsSpan(), false)).Should().Be(2);
        (PathUtilities.CountSegments("a/b/".AsSpan(), true)).Should().Be(2);
    }

    /// <summary>
    /// Verifies that Should HandleEdgeCases When SplittingNormalizedPath.
    /// </summary>
    [TestMethod]
    public void Should_HandleEdgeCases_When_SplittingNormalizedPath() {
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
        relative.Should().Be($"{longSegment}/file.txt");
    }
}
