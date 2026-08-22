//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.IO;

/// <summary>
/// Provides cross-platform, high-performance filesystem path normalization.
/// All matchers and pattern compilers rely on <see cref="PathUtilities"/> to produce a
/// canonical, stable representation of paths.
/// </summary>
/// <remarks>
/// Normalization guarantees:
/// <list type="bullet">
///   <item><description>All separators become forward slashes (<c>'/'</c>).</description></item>
///   <item><description>Consecutive duplicate slashes are collapsed to a single slash,
///         <em>except</em> for the leading <c>//</c> of a UNC path.</description></item>
///   <item><description>Trailing slashes are removed from non-root paths by default.
///         Pass <c>trimTrailingSlash: false</c> to suppress this behavior.</description></item>
///   <item><description>Drive roots (<c>"C:/"</c>) and UNC roots (<c>"//server/share"</c>,
///         <c>"//server/share/"</c>) always preserve their trailing slash regardless of
///         the <c>trimTrailingSlash</c> argument.</description></item>
///   <item><description>Drive-letter prefixes are uppercased on Windows.</description></item>
/// </list>
/// </remarks>
internal static class PathUtilities {
    /// <summary>
    /// Normalizes a filesystem path into a canonical forward-slash form.
    /// </summary>
    /// <param name="path">The raw path string from the filesystem or caller.</param>
    public static string Normalize(string? path) => Normalize(path, trimTrailingSlash: true);

    /// <summary>
    /// Normalizes a filesystem path into a canonical forward-slash form.
    /// </summary>
    /// <param name="path">The raw path string from the filesystem or caller.</param>
    /// <param name="trimTrailingSlash">
    /// When <see langword="true"/> (the default), trailing slashes are removed from
    /// non-root paths. Pass <see langword="false"/> to preserve a trailing slash that
    /// signals a directory context to matchers (for example, when building relative
    /// paths for directory-only pattern evaluation).
    /// Drive roots and UNC roots are never trimmed regardless of this flag.
    /// </param>
    /// <returns>A normalized path with forward slashes and no redundant segments.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="path"/> is <see langword="null"/> or empty.
    /// </exception>
    public static string Normalize(string? path, bool trimTrailingSlash) {
        ArgumentException.ThrowIfNullOrEmpty(path);

        // ALWAYS normalize to forward slashes.
        var normalized = path.Replace('\\', '/');

        // Normalize drive letters (Windows only).
        if(OperatingSystem.IsWindows() && normalized.Length >= 2 && normalized[1] == ':') {
            normalized = char.ToUpperInvariant(normalized[0]) + normalized[1..];
        }

        // Check for double slashes. UNC roots start with "//".
        // NormalizeSlow logic needs to be run to collapse duplicate slashes
        // while preserving leading UNC slashes.
        return NormalizeSlow(normalized, trimTrailingSlash);
    }

    /// <summary>
    /// Converts backslashes to forward slashes without applying full normalization.
    /// Use only where normalization has already been applied upstream.
    /// </summary>
    /// <param name="path">The path to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToForward(string path) => path.Replace('\\', '/');

    /// <summary>
    /// Full normalization: collapses consecutive slashes, preserves the leading
    /// <c>//</c> of UNC paths, and optionally trims trailing slashes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>UNC leading-slash preservation:</strong> before the main loop runs,
    /// a leading <c>//</c> is detected and both slashes are written to the builder
    /// immediately (<c>lastWasSlash = true</c>, <c>startIndex = 2</c>).
    /// </para>
    /// </remarks>
    /// <param name="input">The normalized string to perform slow normalization on.</param>
    /// <param name="trimTrailingSlash">Whether to trim trailing slashes.</param>
    private static string NormalizeSlow(string input, bool trimTrailingSlash) {
        // Force evaluation of the input string as a sequence of forward slashes.
        // On Unix, a leading "//" is technically permitted as a root/server indicator (implementation defined).
        // On Windows, "//" or "\\" is a UNC root indicator.

        var sb = new StringBuilder(input.Length);

        // UNC detection: Check if the normalized string starts with "//".
        // On Unix, a path starting with "//" might be special, but our engine contract
        // mandates that all paths are normalized to forward slashes.
        var isUnc = input.StartsWith("//", StringComparison.Ordinal);

        var startIndex = 0;
        var lastWasSlash = false;

        if(isUnc) {
            sb.Append("//");
            startIndex = 2;
            lastWasSlash = true;
        }

        for(var i = startIndex; i < input.Length; i++) {
            var c = input[i];

            if(c == '/') {
                if(!lastWasSlash) {
                    sb.Append('/');
                    lastWasSlash = true;
                }

                continue;
            }

            lastWasSlash = false;
            sb.Append(c);
        }

        var result = sb.ToString();

        return trimTrailingSlash
            ? TrimTrailingSlash(result)
            : result;
    }

    /// <summary>
    /// Computes a stable, root-relative path using forward slashes.
    /// When <paramref name="entry"/> is a directory the returned path ends with <c>'/'</c>,
    /// which directory-only patterns depend on for precise matching.
    /// </summary>
    /// <param name="rootDir">The root directory of the traversal.</param>
    /// <param name="entry">The filesystem entry to build a path for.</param>
    public static string BuildRelativePath(string rootDir, FileSystemEntry entry) {
        var full = entry.FullPath.AsSpan();
        var root = rootDir.AsSpan();
        var span = full[root.Length..];

        if(span.Length > 0 && span[0] == Path.DirectorySeparatorChar) {
            span = span[1..];
        }

        var bufferLength = span.Length + (entry.IsDirectory ? 1 : 0);
        var buffer = bufferLength <= 256
            ? stackalloc char[bufferLength]
            : new char[bufferLength];

        var written = 0;

        foreach(var ch in span) {
            buffer[written++] = ch == Path.DirectorySeparatorChar ? '/' : ch;
        }

        if(entry.IsDirectory) {
            buffer[written++] = '/';
        }

        return new string(buffer[..written]);
    }

    /// <summary>Returns a <see cref="PathSegmentEnumerator"/> over the normalized segments.</summary>
    /// <param name="normalized">The normalized path to enumerate.</param>
    /// <param name="isDirectory">Whether the path represents a directory.</param>
    public static PathSegmentEnumerator EnumerateSegments(
        ReadOnlySpan<char> normalized,
        bool isDirectory
    ) => new(normalized, isDirectory);

    /// <summary>Counts path segments in a normalized path.</summary>
    /// <param name="path">The normalized path.</param>
    /// <param name="isDirectory">Whether the path represents a directory.</param>
    public static int CountSegments(ReadOnlySpan<char> path, bool isDirectory) {
        if(path.IsEmpty) {
            return 0;
        }

        if(isDirectory && path[^1] == '/') {
            path = path[..^1];
        }

        var count = 1;
        for(var i = 0; i < path.Length; i++) {
            if(path[i] == '/') {
                count++;
            }
        }

        return count;
    }

    /// <summary>Splits a normalized, relative path into its constituent segments.</summary>
    /// <param name="normalized">The normalized path to split.</param>
    /// <param name="isDirectory">Whether the path represents a directory.</param>
    public static string[] SplitNormalizedPath(ReadOnlySpan<char> normalized, bool isDirectory) {
        if(normalized.IsEmpty || (normalized.Length == 1 && normalized[0] == '/')) {
            return [];
        }

        if(isDirectory && normalized.Length > 0 && normalized[^1] == '/') {
            normalized = normalized[..^1];
        }

        var slashIndex = normalized.IndexOf('/');

        if(slashIndex < 0) {
            return [normalized.ToString()];
        }

        List<string> segments = [];
        var start = 0;

        while(true) {
            var idx = normalized[start..].IndexOf('/');

            if(idx < 0) {
                var seg = normalized[start..];
                if(!seg.IsEmpty) {
                    segments.Add(seg.ToString());
                }

                break;
            }

            var part = normalized.Slice(start, idx);
            if(!part.IsEmpty) {
                segments.Add(part.ToString());
            }

            start += idx + 1;
        }

        return [.. segments];
    }

    /// <summary>
    /// Removes a trailing slash from <paramref name="path"/> unless the path
    /// is a root whose trailing slash is semantically significant.
    /// </summary>
    /// <param name="path">The path to trim.</param>
    private static string TrimTrailingSlash(string path) {
        if(path.Length <= 1) {
            return path;
        }

        if(!path.EndsWith('/')) {
            return path;
        }

        // Roots always keep their trailing slash.
        if(IsDriveRoot(path) || IsUncRoot(path)) {
            return path;
        }

        return path.TrimEnd('/');
    }

    /// <summary>Checks if a path ends with a slash.</summary>
    /// <param name="path">The path to check.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool EndsWithSlash(string path) => path.Length > 0 && path[^1] is '/' or '\\';

    /// <summary>
    /// Returns <see langword="true"/> for Windows drive roots such as <c>"C:/"</c>.
    /// </summary>
    /// <param name="path">The path to check.</param>
    private static bool IsDriveRoot(string path) =>
        path.Length == 3 &&
        char.IsLetter(path[0]) &&
        path[1] == ':' &&
        path[2] == '/';

    /// <summary>
    /// Returns <see langword="true"/> for UNC roots, with or without a trailing slash.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <remarks>
    /// <para>
    /// A UNC root has the form <c>//server/share</c> or <c>//server/share/</c>.
    /// </para>
    /// <para>
    /// <strong>Previous implementation (buggy):</strong> counted total forward slashes
    /// and returned <see langword="true"/> only when the count was exactly 3.
    /// This failed for <c>"//server/share/"</c> (4 slashes), causing the trailing slash
    /// to be incorrectly trimmed and the path to become <c>"//server/share"</c> even
    /// when the caller passed a path explicitly representing the share root.
    /// </para>
    /// <para>
    /// <strong>Current implementation:</strong> uses structural parsing.
    /// After confirming the <c>//</c> prefix, it locates the server/share separator
    /// and checks whether anything follows the share name other than an optional
    /// single trailing slash. This correctly distinguishes:
    /// <list type="bullet">
    ///   <item><description><c>"//server/share"</c> → UNC root, no trailing slash</description></item>
    ///   <item><description><c>"//server/share/"</c> → UNC root, trailing slash preserved</description></item>
    ///   <item><description><c>"//server/share/folder"</c> → not a root, trailing slash eligible for trimming</description></item>
    ///   <item><description><c>"//server/share/folder/"</c> → not a root, trailing slash trimmed</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    private static bool IsUncRoot(string path) {
        if(!path.StartsWith("//", StringComparison.Ordinal)) {
            return false;
        }

        // Find the slash between the server name and the share name.
        // The search starts at index 2 to skip the leading "//".
        var serverSlash = path.IndexOf('/', 2);
        if(serverSlash < 0) {
            // "//server" — no share separator, not a valid UNC root.
            return false;
        }

        // Find the slash that follows the share name (if any).
        var afterShare = path.IndexOf('/', serverSlash + 1);

        return afterShare switch {
            // No slash after the share name: "//server/share" — canonical UNC root.
            -1 => true,

            // A slash exists after the share name.
            // It is still a UNC root only when that slash is the very last character
            // ("//server/share/") and nothing comes after it.
            _ => afterShare == path.Length - 1
        };
    }
}
