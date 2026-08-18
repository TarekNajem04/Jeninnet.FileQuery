//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
using System.Buffers;

namespace Jeninnet.FileQuery.Traversal;

/// <summary>
/// Reusable, rented character buffer used to compose root-relative paths during
/// traversal without materializing a managed string for every filesystem entry.
/// </summary>
/// <remarks>
/// <para>
/// The buffer is rented once from <see cref="ArrayPool{T}.Shared"/> and returned on
/// dispose. It grows by renting a larger array, copying the existing content, and
/// returning the old array to the pool. A path built for entry N is guaranteed to
/// stay valid while entry N is processed; the next call to
/// <see cref="BuildRelativePath"/> overwrites it. Spans returned by this type must
/// never outlive either the next build or the buffer instance itself.
/// </para>
/// </remarks>
internal sealed class RelativePathBuffer : IDisposable {
    private const int INITIAL_CAPACITY = 256;

    private char[] _buffer;
    private int _written;

    /// <summary>
    /// Initializes the buffer and rents the backing storage from the shared
    /// <see cref="ArrayPool{T}.Shared"/>.
    /// </summary>
    public RelativePathBuffer() => _buffer = ArrayPool<char>.Shared.Rent(INITIAL_CAPACITY);

    /// <summary>Gets the number of characters the rented backing storage can hold.</summary>
    public int Capacity => _buffer.Length;

    /// <summary>
    /// Gets the root-relative path currently composed in the buffer.
    /// </summary>
    public ReadOnlySpan<char> RelativePath => _buffer.AsSpan(0, _written);

    /// <summary>
    /// Composes the root-relative, normalized representation of <paramref name="entry"/>
    /// in the reusable buffer.
    /// </summary>
    /// <remarks>
    /// Semantics are identical to <see cref="PathUtilities.BuildRelativePath"/>:
    /// the root prefix is removed, path separators become forward slashes, and
    /// directories receive a trailing <c>'/'</c>. The returned span is valid only until
    /// the next call to this method.
    /// </remarks>
    /// <param name="rootDir">The root directory of the traversal.</param>
    /// <param name="entry">The filesystem entry to build a path for.</param>
    public ReadOnlySpan<char> BuildRelativePath(string rootDir, FileSystemEntry entry) {
        var full = entry.FullPath.AsSpan();
        var root = rootDir.AsSpan();
        var span = full[root.Length..];

        if(span.Length > 0 && span[0] == Path.DirectorySeparatorChar) {
            span = span[1..];
        }

        EnsureCapacity(span.Length + (entry.IsDirectory ? 1 : 0));

        _written = 0;

        foreach(var ch in span) {
            _buffer[_written++] = ch == Path.DirectorySeparatorChar ? '/' : ch;
        }

        if(entry.IsDirectory) {
            _buffer[_written++] = '/';
        }

        return RelativePath;
    }

    /// <summary>
    /// Returns the rented backing storage to the shared <see cref="ArrayPool{T}.Shared"/>.
    /// </summary>
    public void Dispose() {
        var buffer = _buffer;
        _buffer = null!;

        if(buffer is not null) {
            ArrayPool<char>.Shared.Return(buffer);
        }
    }

    /// <summary>
    /// Grows the backing storage until it can hold at least <paramref name="required"/>
    /// characters, copying the composed content into the replacement.
    /// </summary>
    /// <param name="required">The minimum number of characters the buffer must hold.</param>
    private void EnsureCapacity(int required) {
        if(required <= _buffer.Length) {
            return;
        }

        var newSize = _buffer.Length;

        while(newSize < required) {
            newSize *= 2;
        }

        var replacement = ArrayPool<char>.Shared.Rent(newSize);
        Array.Copy(_buffer, replacement, _written);
        ArrayPool<char>.Shared.Return(_buffer);
        _buffer = replacement;
    }
}
