//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.IO;

/// <summary>
/// Enumerates normalized path segments without allocation.
/// </summary>
/// <remarks>
/// <para>
/// <strong>State Contract:</strong>
/// </para>
/// <list type="number">
///   <item>
///     <description>
///     The enumerator is initially positioned <em>before</em> the first segment.
///     </description>
///   </item>
///   <item>
///     <description>
///     <see cref="MoveNext"/> must be called exactly once to advance to each segment.
///     </description>
///   </item>
///   <item>
///     <description>
///     <see cref="Current"/> is only valid after a successful call to
///     <see cref="MoveNext"/>.
///     </description>
///   </item>
///   <item>
///     <description>
///     The enumerator is a <c>struct</c> and is safe to copy by value for
///     speculative matching and backtracking.
///     </description>
///   </item>
///   <item>
///     <description>
///     There is no reset operation. A new enumerator must be created to restart enumeration.
///     </description>
///   </item>
/// </list>
/// <para>
/// Violating this contract results in undefined matching behavior.
/// </para>
/// </remarks>
internal ref struct PathSegmentEnumerator {
    private ReadOnlySpan<char> _remaining;

    public PathSegmentEnumerator(ReadOnlySpan<char> path, bool isDirectory) {
        // Trim trailing slash for directories
        if(isDirectory && path.Length > 0 && path[^1] == '/') {
            path = path[..^1];
        }

        _remaining = path;
        Current = default;
    }

    public ReadOnlySpan<char> Current { get; private set; }

    public bool MoveNext() {
        if(_remaining.IsEmpty) {
            return false;
        }

        var index = _remaining.IndexOf('/');
        if(index < 0) {
            Current = _remaining;
            _remaining = [];
            return true;
        }

        Current = _remaining[..index];
        _remaining = _remaining[(index + 1)..];
        return true;
    }
}
