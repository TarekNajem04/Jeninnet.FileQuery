//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Traversal;

/// <summary>
/// Immutable metadata predicate applied before pattern matching.
/// </summary>
/// <remarks>
/// Filters use metadata already captured by the enumeration. Size and time filters apply to
/// files only: directory entries do not represent the aggregate size or timestamp of their
/// contents, so applying them to directories would incorrectly prune entire subtrees.
/// Attribute filters apply to both files and directories, matching the semantics of
/// <see cref="EnumerationOptions.AttributesToSkip"/>.
/// </remarks>
/// <param name="MinSize">The inclusive minimum file size, or <see langword="null"/>.</param>
/// <param name="MaxSize">The inclusive maximum file size, or <see langword="null"/>.</param>
/// <param name="ModifiedAfterUtc">The exclusive lower last-write boundary in UTC, or <see langword="null"/>.</param>
/// <param name="ModifiedBeforeUtc">The exclusive upper last-write boundary in UTC, or <see langword="null"/>.</param>
/// <param name="AttributesToSkip">The attribute flags that exclude an entry.</param>
internal readonly record struct EntryFilter(
    long? MinSize,
    long? MaxSize,
    DateTime? ModifiedAfterUtc,
    DateTime? ModifiedBeforeUtc,
    FileAttributes AttributesToSkip
) {
    /// <summary>
    /// Creates the filter projection for the supplied query options.
    /// </summary>
    /// <param name="options">The query options.</param>
    /// <returns>The immutable filter.</returns>
    public static EntryFilter From(FileQueryOptions options) => new(
        options.MinSize,
        options.MaxSize,
        options.ModifiedAfter?.UtcDateTime,
        options.ModifiedBefore?.UtcDateTime,
        options.AttributesToSkip);

    /// <summary>
    /// Gets a value indicating whether the filter accepts every entry.
    /// </summary>
    public bool IsEmpty =>
        MinSize is null &&
        MaxSize is null &&
        ModifiedAfterUtc is null &&
        ModifiedBeforeUtc is null &&
        AttributesToSkip is FileAttributes.None;

    /// <summary>
    /// Determines whether an entry passes the filter.
    /// </summary>
    /// <param name="entry">The enumerated entry.</param>
    /// <returns><see langword="true"/> when the entry may be matched; otherwise <see langword="false"/>.</returns>
    public bool ShouldInclude(FileSystemEntry entry) {
        if(IsEmpty) {
            return true;
        }

        if(AttributesToSkip is not FileAttributes.None &&
            (entry.Attributes & AttributesToSkip) is not 0) {
            return false;
        }

        if(entry.IsDirectory) {
            return true;
        }

        if(MinSize is { } minSize && entry.Length < minSize) {
            return false;
        }

        if(MaxSize is { } maxSize && entry.Length > maxSize) {
            return false;
        }

        if(ModifiedAfterUtc is { } modifiedAfter && entry.LastWriteTimeUtc <= modifiedAfter) {
            return false;
        }

        if(ModifiedBeforeUtc is { } modifiedBefore && entry.LastWriteTimeUtc >= modifiedBefore) {
            return false;
        }

        return true;
    }
}
