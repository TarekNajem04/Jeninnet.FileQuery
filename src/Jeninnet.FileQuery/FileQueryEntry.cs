//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery;

/// <summary>
/// Describes a matched filesystem entry together with metadata captured from the
/// operating-system enumeration.
/// </summary>
/// <param name="FullPath">The absolute path reported by the operating system.</param>
/// <param name="RelativePath">The path relative to the query root, using forward slashes and no trailing slash.</param>
/// <param name="Length">The entry size in bytes, or <c>0</c> for directories and when the platform does not report it.</param>
/// <param name="CreationTimeUtc">The creation time in UTC, or <see cref="DateTime.MinValue"/> when unavailable.</param>
/// <param name="LastWriteTimeUtc">The last-write time in UTC, or <see cref="DateTime.MinValue"/> when unavailable.</param>
/// <param name="Attributes">The file attribute flags reported by the enumeration.</param>
/// <remarks>
/// <para>
/// Metadata is captured from the entries produced by the traversal enumeration, so requesting
/// entries does not add per-file system calls.
/// </para>
/// <para>
/// The nominal entry size is the size reported by the filesystem entry, not a fresh stat call.
/// On some platforms creation time may be unavailable (for example, on filesystems without
/// birth-time support).
/// </para>
/// </remarks>
public sealed record FileQueryEntry(
    string FullPath,
    string RelativePath,
    long Length,
    DateTime CreationTimeUtc,
    DateTime LastWriteTimeUtc,
    FileAttributes Attributes
) {
    /// <summary>
    /// Gets the query root that produced this entry.
    /// </summary>
    /// <remarks>
    /// This is set for every entry and is especially useful for multi-root batches, where the
    /// same relative path can occur below different roots.
    /// </remarks>
    public string RootPath { get; init; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the entry represents a directory.
    /// </summary>
    public bool IsDirectory => (Attributes & FileAttributes.Directory) is not 0;

    /// <summary>
    /// Creates a public entry from an internal enumeration entry and its relative path.
    /// </summary>
    /// <param name="entry">The internal filesystem entry.</param>
    /// <param name="relativePath">The root-relative path; directories may carry a trailing slash.</param>
    /// <param name="rootPath">The query root that produced the entry.</param>
    /// <returns>The immutable public entry.</returns>
    internal static FileQueryEntry Create(FileSystemEntry entry, ReadOnlySpan<char> relativePath, string rootPath) {
        var normalized = entry.IsDirectory && relativePath.Length > 0 && relativePath[^1] == '/'
            ? relativePath[..^1]
            : relativePath;

        return new FileQueryEntry(
            entry.FullPath,
            normalized.ToString(),
            entry.Length,
            entry.CreationTimeUtc,
            entry.LastWriteTimeUtc,
            entry.Attributes) {
            RootPath = rootPath
        };
    }
}
