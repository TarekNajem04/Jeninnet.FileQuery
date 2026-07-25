namespace Jeninnet.FileQuery.Tests.Shared;

/// <summary>
/// Helpers for normalizing paths and building expected results
/// in tests that must work cross-platform.
/// </summary>
public static class TestPathUtils {
    /// <summary>
    /// Platform-normalized join, e.g. returns:
    /// Windows:   "a\b\c.txt"
    /// Linux/mac: "a/b/c.txt"
    /// </summary>
    /// <param name="segments">The path segments to join.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Join(params string[] segments) => Path.Combine(segments);

    /// <summary>
    /// Normalizes slashes in absolute paths returned by FileQueryEngine.
    /// </summary>
    /// <param name="p">The path to normalize.</param>
    public static string Normalize(string p) {
        ArgumentNullException.ThrowIfNull(p);
        return p.Replace('\\', Path.DirectorySeparatorChar)
            .Replace('/', Path.DirectorySeparatorChar);
    }

    /// <summary>
    /// Asserts that two paths match regardless of slash direction.
    /// Helpful when FileQueryEngine returns normalized paths.
    /// </summary>
    /// <param name="a">First path to compare.</param>
    /// <param name="b">Second path to compare.</param>
    public static bool Equivalent(string a, string b) {
        a = Normalize(a).TrimEnd(Path.DirectorySeparatorChar);
        b = Normalize(b).TrimEnd(Path.DirectorySeparatorChar);

        return a.Equals(b, StringComparison.OrdinalIgnoreCase);
    }
}
