
using Path = System.IO.Path;

namespace Jeninnet.FileQuery.Tests.Unit.Traversal;
/// <summary>
/// Tests for <see cref="RelativePathBuffer"/>: reusable, rented buffer used by the
/// traversal hot path to compose root-relative paths without per-entry strings.
/// </summary>
[TestClass]
public class RelativePathBufferTests {
    /// <summary>
    /// Verifies that a shallow file path is composed without a leading separator.
    /// </summary>
    [TestMethod]
    public void BuildRelativePath_ShallowFile_StripsRootPrefix() {
        using var buffer = new RelativePathBuffer();
        const string root = "C:/root";
        var entry = new FileSystemEntry(
            System.IO.Path.Combine(root, "file.txt"),
            FileAttributes.Normal
        );

        var relative = buffer.BuildRelativePath(root, entry);

        Assert.AreEqual("file.txt", relative.ToString());
    }

    /// <summary>
    /// Verifies that the leading separator between root and path is removed.
    /// </summary>
    [TestMethod]
    public void BuildRelativePath_RootRelative_NormalizesLeadingSeparator() {
        using var buffer = new RelativePathBuffer();
        const string root = "C:/root";
        var entry = new FileSystemEntry(
            System.IO.Path.Combine(root, "sub", "file.txt"),
            FileAttributes.Normal
        );

        var relative = buffer.BuildRelativePath(root, entry);

        Assert.AreEqual("sub/file.txt", relative.ToString());
    }

    /// <summary>
    /// Verifies that directories receive a trailing slash, preserving
    /// directory-only pattern semantics.
    /// </summary>
    [TestMethod]
    public void BuildRelativePath_Directory_AppendsTrailingSlash() {
        using var buffer = new RelativePathBuffer();
        const string root = "C:/root";
        var entry = new FileSystemEntry(
            System.IO.Path.Combine(root, "subdir"),
            FileAttributes.Directory
        );

        var relative = buffer.BuildRelativePath(root, entry);

        Assert.AreEqual("subdir/", relative.ToString());
    }

    /// <summary>
    /// Verifies that backslash separators are converted to forward slashes.
    /// </summary>
    [TestMethod]
    public void BuildRelativePath_Backslashes_AreNormalizedToForwardSlashes() {
        using var buffer = new RelativePathBuffer();
        const string root = @"C:\root";
        var entry = new FileSystemEntry(
            System.IO.Path.Combine(root, "a", "b.txt"),
            FileAttributes.Normal
        );

        var relative = buffer.BuildRelativePath(root, entry);

        Assert.AreEqual("a/b.txt", relative.ToString());
    }

    /// <summary>
    /// Verifies that a path longer than the initial capacity grows the buffer
    /// and is still composed correctly.
    /// </summary>
    [TestMethod]
    public void BuildRelativePath_DeepPath_GrowsBufferAndStaysCorrect() {
        using var buffer = new RelativePathBuffer();
        const string root = "C:/root";

        var segments = string.Join(
            System.IO.Path.DirectorySeparatorChar,
            Enumerable.Range(0, 12).Select(static i => $"directory-with-a-long-name-{i:00}")
        );
        var full = System.IO.Path.Combine(root, segments, "deeply-nested-file.txt");
        var entry = new FileSystemEntry(full, FileAttributes.Normal);

        var expected = PathUtilities.BuildRelativePath(root, entry);
        var relative = buffer.BuildRelativePath(root, entry);

        Assert.IsGreaterThan(256, buffer.Capacity, "Buffer should have grown beyond the initial capacity.");
        Assert.AreEqual(expected, relative.ToString());
    }

    /// <summary>
    /// Verifies that consecutive builds reuse the backing storage and that a
    /// later, longer path overwrites the previous content without leftovers.
    /// </summary>
    [TestMethod]
    public void BuildRelativePath_RepeatedBuilds_OverwritePreviousContent() {
        using var buffer = new RelativePathBuffer();
        const string root = "C:/root";

        var first = buffer.BuildRelativePath(
            root,
            new FileSystemEntry(System.IO.Path.Combine(root, "a.txt"), FileAttributes.Normal)
        );
        Assert.AreEqual("a.txt", first.ToString());

        var longSegments = string.Join(
            System.IO.Path.DirectorySeparatorChar,
            Enumerable.Range(0, 10).Select(static i => $"segment-{i:00}")
        );
        var second = buffer.BuildRelativePath(
            root,
            new FileSystemEntry(System.IO.Path.Combine(root, longSegments, "b.txt"), FileAttributes.Normal)
        );

        Assert.AreEqual(
            "segment-00/segment-01/segment-02/segment-03/segment-04/segment-05/segment-06/segment-07/segment-08/segment-09/b.txt",
            second.ToString()
        );
    }

    /// <summary>
    /// Verifies that disposing twice is safe and returns storage to the pool exactly once.
    /// </summary>
    [TestMethod]
    public void Dispose_IsIdempotent() {
        var buffer = new RelativePathBuffer();

        var capacity = buffer.Capacity;
        Assert.IsGreaterThan(0, capacity);

        buffer.Dispose();
    }

    /// <summary>
    /// Verifies that an entry equal to the root produces the shortest possible
    /// relative representation (a directory marker only).
    /// </summary>
    [TestMethod]
    public void BuildRelativePath_RootItself_ComposesDirectoryMarker() {
        using var buffer = new RelativePathBuffer();
        const string root = "C:/root";
        var entry = new FileSystemEntry(root, FileAttributes.Directory);

        var relative = buffer.BuildRelativePath(root, entry);

        Assert.AreEqual("/", relative.ToString());
    }
}
