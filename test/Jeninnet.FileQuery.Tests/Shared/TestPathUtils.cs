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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Join(params string[] segments)
        => Path.Combine(segments);

    /// <summary>
    /// Normalizes slashes in absolute paths returned by FileQueryEngine.
    /// </summary>
    public static string Normalize(string p)
        => p.Replace('\\', Path.DirectorySeparatorChar)
            .Replace('/', Path.DirectorySeparatorChar);

    /// <summary>
    /// Asserts that two paths match regardless of slash direction.
    /// Helpful when FileQueryEngine returns normalized paths.
    /// </summary>
    public static bool Equivalent(string a, string b) {
        a = Normalize(a).TrimEnd(Path.DirectorySeparatorChar);
        b = Normalize(b).TrimEnd(Path.DirectorySeparatorChar);

        return a.Equals(b, StringComparison.OrdinalIgnoreCase);
    }
}
