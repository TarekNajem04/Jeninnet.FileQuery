//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.Testing.Assertions.Utilities;

/// <summary>Provides utility methods for path manipulation and comparison in test code.</summary>
public static class PathHelper {
    /// <summary>Joins path segments using the platform's directory separator character.</summary>
    /// <param name="segments">The path segments to join.</param>
    /// <returns>A single path string with segments separated by <see cref="Path.DirectorySeparatorChar"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="segments"/> is <see langword="null"/>.</exception>
    public static string Join(params string[] segments) {
        ArgumentNullException.ThrowIfNull(segments);
        return string.Join(Path.DirectorySeparatorChar.ToString(), segments);
    }

    /// <summary>Normalizes a path by replacing forward slashes with the platform's directory separator character.</summary>
    /// <param name="p">The path to normalize.</param>
    /// <returns>The normalized path string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="p"/> is <see langword="null"/>.</exception>
    public static string Normalize(string p) {
        ArgumentNullException.ThrowIfNull(p);
        return p.Replace('/', Path.DirectorySeparatorChar);
    }

    /// <summary>Determines whether two paths are equivalent after normalization and case-insensitive comparison.</summary>
    /// <param name="a">The first path to compare.</param>
    /// <param name="b">The second path to compare.</param>
    /// <returns><see langword="true"/> if the normalized paths are equivalent; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="a"/> or <paramref name="b"/> is <see langword="null"/>.</exception>
    public static bool Equivalent(string a, string b) {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);

        var normalizedA = Normalize(a).TrimEnd(Path.DirectorySeparatorChar);
        var normalizedB = Normalize(b).TrimEnd(Path.DirectorySeparatorChar);
        return string.Equals(normalizedA, normalizedB, StringComparison.OrdinalIgnoreCase);
    }
}
