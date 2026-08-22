//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.IO;

internal readonly ref struct PathView {
    public ReadOnlySpan<char> Path { get; }
    public bool IsDirectory { get; }
    public int SegmentCount { get; }

    public PathView(ReadOnlySpan<char> path, bool isDirectory) {
        Path = path;
        IsDirectory = isDirectory;
        SegmentCount = PathUtilities.CountSegments(path, isDirectory);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PathSegmentEnumerator EnumerateSegments() => PathUtilities.EnumerateSegments(Path, IsDirectory);

    internal static PathView Create(ReadOnlySpan<char> path, bool isDirectory) => new(path, isDirectory);
}
