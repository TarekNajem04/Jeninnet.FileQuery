//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.Testing.Assertions.Extensions;

/// <summary>Provides extension methods for path-related string comparisons.</summary>
public static class PathExtensions {
    /// <summary>
    /// Determines whether the path ends with the specified suffix, normalizing both
    /// paths to use the platform's directory separator character before comparing.
    /// </summary>
    /// <param name="path">The full path to inspect.</param>
    /// <param name="ending">The suffix to look for at the end of <paramref name="path"/>.</param>
    /// <returns><see langword="true"/> if <paramref name="path"/> ends with <paramref name="ending"/>; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="path"/> or <paramref name="ending"/> is <see langword="null"/>.</exception>
    public static bool EndWithNormalized(this string path, string ending) {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(ending);

        ending = ending.Replace('/', Path.DirectorySeparatorChar);
        path = path.Replace('/', Path.DirectorySeparatorChar);
        return path.EndsWith(ending, StringComparison.Ordinal);
    }

    /// <summary>
    /// Determines whether the path ends with the specified relative path segment,
    /// normalizing both paths to use the platform's directory separator character.
    /// </summary>
    /// <param name="path">The full path to inspect.</param>
    /// <param name="relative">The relative path suffix to look for.</param>
    /// <returns><see langword="true"/> if <paramref name="path"/> ends with <paramref name="relative"/>; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="path"/> or <paramref name="relative"/> is <see langword="null"/>.</exception>
    public static bool EndWithPath(this string path, string relative) {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(relative);

        relative = relative.Replace('/', Path.DirectorySeparatorChar);
        path = path.Replace('/', Path.DirectorySeparatorChar);
        return path.EndsWith(relative, StringComparison.Ordinal);
    }
}
