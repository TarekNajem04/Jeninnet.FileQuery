//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
using System.Text;
using Jeninnet.FileQuery.Enums;
using Jeninnet.FileQuery.Extensions;

namespace Jeninnet.FileQuery.Traversal;

/// <summary>
/// Restricts traversal to an explicit allow-list of root-relative file paths.
/// </summary>
/// <remarks>
/// <para>
/// The filter stores the allowed file paths and every ancestor directory. File lookups compare
/// the traversal's pooled relative-path span directly, so no per-entry string is allocated.
/// </para>
/// <para>
/// Directory lookups let the executor prune subtrees that cannot contain an allowed file.
/// </para>
/// </remarks>
internal sealed class RelativePathFilter {
    private readonly HashSet<string>.AlternateLookup<ReadOnlySpan<char>> _fileLookup;
    private readonly HashSet<string>.AlternateLookup<ReadOnlySpan<char>> _directoryLookup;

    /// <summary>
    /// Initializes a filter over the supplied lookup sets.
    /// </summary>
    /// <param name="files">The normalized allowed file paths.</param>
    /// <param name="directories">The normalized ancestor directories, including the empty root.</param>
    private RelativePathFilter(HashSet<string> files, HashSet<string> directories) {
        _fileLookup = files.GetAlternateLookup<ReadOnlySpan<char>>();
        _directoryLookup = directories.GetAlternateLookup<ReadOnlySpan<char>>();
    }

    /// <summary>
    /// Creates a filter from root-relative file paths.
    /// </summary>
    /// <param name="includedPaths">The allowed file paths.</param>
    /// <param name="caseSensitivity">The resolved case sensitivity used for lookups.</param>
    /// <returns>The initialized filter.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when a path is empty, absolute, or contains a parent segment.
    /// </exception>
    public static RelativePathFilter Create(IEnumerable<string> includedPaths, CaseSensitivity caseSensitivity) {
        ArgumentNullException.ThrowIfNull(includedPaths);

        var comparer = caseSensitivity.Resolve() is CaseSensitivity.Insensitive
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;

        var files = new HashSet<string>(comparer);
        var directories = new HashSet<string>(comparer) { string.Empty };

        foreach(var path in includedPaths) {
            var normalized = Normalize(path);
            files.Add(normalized);
            AddAncestorDirectories(directories, normalized);
        }

        return new RelativePathFilter(files, directories);
    }

    /// <summary>
    /// Determines whether a file at <paramref name="relativePath"/> is in the allow-list.
    /// </summary>
    /// <param name="relativePath">The normalized root-relative file path.</param>
    /// <returns><see langword="true"/> when the file is listed.</returns>
    public bool ContainsFile(ReadOnlySpan<char> relativePath) => _fileLookup.Contains(relativePath);

    /// <summary>
    /// Determines whether a directory can contain an allowed file.
    /// </summary>
    /// <param name="relativePath">The normalized root-relative directory path (with or without a trailing slash).</param>
    /// <returns><see langword="true"/> when the directory is the root or an ancestor of a listed file.</returns>
    public bool CanContainFiles(ReadOnlySpan<char> relativePath) =>
        _directoryLookup.Contains(relativePath.TrimEnd('/'));

    /// <summary>
    /// Normalizes a path to forward slashes and validates that it is root-relative.
    /// </summary>
    /// <param name="path">The path to normalize.</param>
    /// <returns>The normalized path.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the path is empty, absolute, or contains a parent segment.
    /// </exception>
    private static string Normalize(string path) {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if(path[^1] is '/' or '\\') {
            throw new ArgumentException(
                $"Included paths must identify files, not directories: '{path}'.",
                nameof(path));
        }

        if(Path.IsPathRooted(path)) {
            throw new ArgumentException(
                $"Included paths must be relative to the query root: '{path}'.",
                nameof(path));
        }

        var builder = new StringBuilder(path.Length);

        foreach(var segment in path.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries)) {
            if(segment is ".") {
                continue;
            }

            if(segment is "..") {
                throw new ArgumentException(
                    $"Included paths must not contain parent segments: '{path}'.",
                    nameof(path));
            }

            if(builder.Length > 0) {
                builder.Append('/');
            }

            builder.Append(segment);
        }

        if(builder.Length == 0) {
            throw new ArgumentException("Included paths must not be empty.", nameof(path));
        }

        return builder.ToString();
    }

    /// <summary>
    /// Adds every ancestor directory of <paramref name="filePath"/> to the lookup set.
    /// </summary>
    /// <param name="directories">The directory lookup set.</param>
    /// <param name="filePath">The normalized file path.</param>
    private static void AddAncestorDirectories(HashSet<string> directories, string filePath) {
        var index = filePath.LastIndexOf('/');

        while(index > 0) {
            var directory = filePath[..index];
            directories.Add(directory);
            index = directory.LastIndexOf('/');
        }
    }
}
