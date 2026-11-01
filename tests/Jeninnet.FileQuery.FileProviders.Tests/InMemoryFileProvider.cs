//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
using System.Collections;
using Microsoft.Extensions.Primitives;

namespace Jeninnet.FileQuery.FileProviders.Tests;

/// <summary>
/// A minimal in-memory <see cref="IFileProvider"/> used to prove provider portability.
/// </summary>
internal sealed class InMemoryFileProvider : IFileProvider {
    private readonly Dictionary<string, InMemoryFileInfo> _files = new(StringComparer.Ordinal);
    private readonly bool _throwOnEnumeration;

    /// <summary>
    /// Initializes an empty provider.
    /// </summary>
    /// <param name="throwOnEnumeration">
    /// When <see langword="true"/>, directory enumeration throws <see cref="UnauthorizedAccessException"/>.
    /// </param>
    public InMemoryFileProvider(bool throwOnEnumeration = false) =>
        _throwOnEnumeration = throwOnEnumeration;

    /// <summary>
    /// Adds a file with the supplied contents.
    /// </summary>
    /// <param name="path">The root-relative path.</param>
    /// <param name="contents">The file contents.</param>
    public void AddFile(string path, string contents) {
        var normalized = Normalize(path);
        _files[normalized] = new InMemoryFileInfo(
            Path.GetFileName(normalized),
            normalized,
            isDirectory: false,
            Encoding.UTF8.GetBytes(contents),
            DateTimeOffset.UtcNow);
    }

    /// <inheritdoc/>
    public IDirectoryContents GetDirectoryContents(string subpath) {
        if(_throwOnEnumeration) {
            throw new UnauthorizedAccessException("The provider denied directory enumeration.");
        }

        var normalized = Normalize(subpath);
        var prefix = normalized.Length == 0 ? "" : normalized + "/";
        var entries = new List<IFileInfo>();
        var directories = new HashSet<string>(StringComparer.Ordinal);

        foreach(var (path, info) in _files) {
            if(!path.StartsWith(prefix, StringComparison.Ordinal)) {
                continue;
            }

            var remaining = path[prefix.Length..];
            var separator = remaining.IndexOf('/', StringComparison.Ordinal);

            if(separator < 0) {
                entries.Add(info);
                continue;
            }

            var directoryName = remaining[..separator];

            if(directories.Add(directoryName)) {
                entries.Add(new InMemoryFileInfo(
                    directoryName,
                    prefix + directoryName,
                    isDirectory: true,
                    [],
                    info.LastModified));
            }
        }

        return new InMemoryDirectoryContents(entries);
    }

    /// <inheritdoc/>
    public IFileInfo GetFileInfo(string subpath) {
        var normalized = Normalize(subpath);

        if(normalized.Length == 0) {
            return new InMemoryFileInfo("", "", isDirectory: true, [], DateTimeOffset.UtcNow);
        }

        if(_files.TryGetValue(normalized, out var file)) {
            return file;
        }

        var directoryPrefix = normalized + "/";
        var directory = _files
            .Where(pair => pair.Key.StartsWith(directoryPrefix, StringComparison.Ordinal))
            .Select(pair => pair.Value)
            .FirstOrDefault();

        return directory is not null
            ? new InMemoryFileInfo(normalized, normalized, isDirectory: true, [], directory.LastModified)
            : new InMemoryFileInfo(Path.GetFileName(normalized), normalized, isDirectory: false, [], DateTimeOffset.MinValue);
    }

    /// <inheritdoc/>
    public IChangeToken Watch(string filter) => NullChangeToken.Singleton;

    private static string Normalize(string path) => path.Replace('\\', '/').Trim('/');
}

internal sealed class InMemoryDirectoryContents(IReadOnlyList<IFileInfo> entries) : IDirectoryContents {
    public bool Exists => true;

    public IEnumerator<IFileInfo> GetEnumerator() => entries.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

internal sealed class InMemoryFileInfo(
    string name,
    string path,
    bool isDirectory,
    byte[] contents,
    DateTimeOffset lastModified
) : IFileInfo {
    public bool Exists => path.Length > 0;

    public long Length => contents.Length;

    public string PhysicalPath => string.Empty;

    public string Name => name;

    public DateTimeOffset LastModified => lastModified;

    public bool IsDirectory => isDirectory;

    public Stream CreateReadStream() => new MemoryStream(contents, writable: false);
}
