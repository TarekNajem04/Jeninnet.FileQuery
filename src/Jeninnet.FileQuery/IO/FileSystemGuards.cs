namespace Jeninnet.FileQuery.IO;

/// <summary>
/// Provides guard methods for performing safe file system operations.
/// </summary>
/// <remarks>
/// This class centralizes the exception handling logic for directory enumeration,
/// ensuring that <see cref="UnauthorizedAccessException"/> and <see cref="IOException"/>
/// are handled consistently across the engine based on the <see langword="ignoreInaccessible"/> policy.
/// </remarks>
internal static class FileSystemGuards {
    /// <summary>
    /// Enumerates filesystem entries while applying the configured exception policy.
    /// </summary>
    /// <param name="directory">The directory to enumerate.</param>
    /// <param name="ignoreInaccessible">
    /// If <see langword="true"/>, IO exceptions during initial enumeration are suppressed.
    /// </param>
    /// <returns>
    /// A sequence of entry paths, or an empty sequence if the directory is inaccessible and
    /// <paramref name="ignoreInaccessible"/> is <see langword="true"/>.
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">Thrown if the directory is inaccessible and <paramref name="ignoreInaccessible"/> is <see langword="false"/>.</exception>
    /// <exception cref="IOException">Thrown if an IO error occurs and <paramref name="ignoreInaccessible"/> is <see langword="false"/>.</exception>
    public static IEnumerable<string> EnumerateEntries(
        string directory,
        bool ignoreInaccessible
    ) {
        try {
            return Directory.EnumerateFileSystemEntries(directory);
        }
        catch(Exception ex) when(
            ex is UnauthorizedAccessException
            or IOException
            or DirectoryNotFoundException
        ) {
            if(ignoreInaccessible) {
                return [];
            }

            throw;
        }
    }

    /// <summary>
    /// Validates that a directory is accessible according to the configured policy.
    /// </summary>
    /// <param name="directory">The directory path to verify.</param>
    /// <param name="ignoreInaccessible">
    /// If <see langword="true"/>, access checks are skipped.
    /// </param>
    /// <remarks>
    /// This method forces an enumeration attempt to trigger any potential
    /// <see cref="UnauthorizedAccessException"/> before the engine continues traversal.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EnsureAccessible(string directory, bool ignoreInaccessible) {
        if(ignoreInaccessible) {
            return;
        }

        try {
            // Force enumeration to trigger access checks.
            using var entries = Directory.EnumerateFileSystemEntries(directory).GetEnumerator();
            _ = entries.MoveNext();
        }
        catch(Exception ex) when(
            ex is UnauthorizedAccessException
            or IOException
            or DirectoryNotFoundException
        ) {
            throw;
        }
    }
}
