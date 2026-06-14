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
    /// Enumerates filesystem entries while applying the configured exception policy.
    /// </summary>
    /// <param name="directory">The directory to enumerate.</param>
    /// <param name="ignoreInaccessible">
    /// If <see langword="true"/>, IO exceptions during initial enumeration are suppressed.
    /// </param>
    /// <param name="errorRecovery">The configured IO error recovery policy.</param>
    /// <returns>
    /// A sequence of entry paths, or an empty sequence if the directory is inaccessible and
    /// <paramref name="ignoreInaccessible"/> is <see langword="true"/>.
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">Thrown if the directory is inaccessible and <paramref name="ignoreInaccessible"/> is <see langword="false"/>.</exception>
    /// <exception cref="IOException">Thrown if an IO error occurs and <paramref name="ignoreInaccessible"/> is <see langword="false"/>.</exception>
    public static IEnumerable<string> EnumerateEntries(
        string directory,
        bool ignoreInaccessible,
        FileQueryErrorRecoveryOptions errorRecovery
    )
    {
        ArgumentNullException.ThrowIfNull(directory);
        ArgumentNullException.ThrowIfNull(errorRecovery);

        return EnumerateEntriesInternal(directory, ignoreInaccessible, errorRecovery);
    }

    private static IEnumerable<string> EnumerateEntriesInternal(
        string directory,
        bool ignoreInaccessible,
        FileQueryErrorRecoveryOptions errorRecovery
    )
    {
        var attempts = errorRecovery.Action is FileQueryErrorAction.Retry
            ? errorRecovery.MaxRetryAttempts + 1
            : 1;

        var attempt = 0;
        IEnumerator<string>? enumerator = null;

        while(attempt < attempts)
        {
            string? current = null;
            var hasCurrent = false;

            try
            {
                enumerator ??= Directory.EnumerateFileSystemEntries(directory).GetEnumerator();
                hasCurrent = enumerator.MoveNext();

                if(hasCurrent)
                {
                    current = enumerator.Current;
                }
            }
            catch(Exception ex) when(IsRecoverable(ex))
            {
                enumerator?.Dispose();
                enumerator = null;

                if(ShouldSkip(ignoreInaccessible, errorRecovery, attempt, attempts))
                {
                    yield break;
                }

                if(errorRecovery.Action is FileQueryErrorAction.Retry && attempt < attempts - 1)
                {
                    attempt++;
                    continue;
                }

                throw;
            }

            if(!hasCurrent)
            {
                enumerator?.Dispose();
                yield break;
            }

            yield return current!;
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
