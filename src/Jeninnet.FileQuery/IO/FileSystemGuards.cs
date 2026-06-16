namespace Jeninnet.FileQuery.IO;

/// <summary>
/// Provides guard methods for performing safe file system operations.
/// </summary>
/// <remarks>
/// This class centralizes the exception handling logic for directory enumeration,
/// ensuring that <see cref="UnauthorizedAccessException"/> and <see cref="IOException"/>
/// are handled consistently across the engine based on the <see langword="ignoreInaccessible"/> policy.
/// </remarks>
internal static class FileSystemGuards
{
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
    public static void EnsureAccessible(string directory, bool ignoreInaccessible)
    {
        if(ignoreInaccessible)
        {
            return;
        }

        try
        {
            // Force enumeration to trigger access checks.
            using var entries = Directory.EnumerateFileSystemEntries(directory).GetEnumerator();
            _ = entries.MoveNext();
        }
        catch(Exception ex) when(
            ex is UnauthorizedAccessException
            or IOException
            or DirectoryNotFoundException
        )
        {
            throw;
        }
    }

    internal static bool IsRecoverable(Exception ex) =>
        ex is UnauthorizedAccessException
        or IOException
        or DirectoryNotFoundException;

    internal static bool ShouldSkip(
        bool ignoreInaccessible,
        FileQueryErrorRecoveryOptions errorRecovery,
        int attempt,
        int attempts
    ) =>
        errorRecovery.Action is FileQueryErrorAction.Skip ||
        (ignoreInaccessible && errorRecovery.Action is not FileQueryErrorAction.Abort) ||
        (errorRecovery.Action is FileQueryErrorAction.Retry && attempt >= attempts - 1 && ignoreInaccessible);
}
