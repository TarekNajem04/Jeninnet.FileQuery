namespace Jeninnet.FileQuery.IO;

/// <summary>
/// The default implementation of the file system abstraction.
/// </summary>
/// <remarks>
/// Wraps standard <see cref="System.IO"/> APIs. This class is implemented as a singleton
/// to avoid overhead and provide a consistent access point for the engine.
/// </remarks>
internal sealed class FileSystem : IFileSystem
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="FileSystem"/>.
    /// </summary>
    public static FileSystem Instance { get; } = new FileSystem();

    /// <summary>
    /// Enumerates entries in the specified directory using a guarded approach to handle accessibility.
    /// </summary>
    /// <inheritdoc/>
    public IEnumerable<FileSystemEntry> Enumerate(
        string directory,
        bool ignoreInaccessible,
        FileQueryErrorRecoveryOptions errorRecovery
    )
    {
        foreach(var path in FileSystemGuards.EnumerateEntries(directory, ignoreInaccessible, errorRecovery))
        {
            if(!TryGetAttributes(path, ignoreInaccessible, errorRecovery, out var attributes))
            {
                continue;
            }

            if(attributes.HasFlag(FileAttributes.Directory))
            {
                if(!TryEnsureAccessible(path, ignoreInaccessible, errorRecovery))
                {
                    continue;
                }
            }

            yield return new FileSystemEntry(path, attributes);
        }
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<FileSystemEntry> EnumerateAsync(
        string directory,
        bool ignoreInaccessible,
        FileQueryErrorRecoveryOptions errorRecovery,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        if(!TryEnsureAccessible(directory, ignoreInaccessible, errorRecovery))
        {
            yield break;
        }

        foreach(var path in FileSystemGuards.EnumerateEntries(directory, ignoreInaccessible, errorRecovery))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if(!TryGetAttributes(path, ignoreInaccessible, errorRecovery, out var attributes))
            {
                continue;
            }

            if(attributes.HasFlag(FileAttributes.Directory))
            {
                if(!TryEnsureAccessible(path, ignoreInaccessible, errorRecovery))
                {
                    continue;
                }
            }

            yield return new FileSystemEntry(path, attributes);

            // Directory enumeration is backed by synchronous OS APIs; yield between
            // entries so async consumers can observe cancellation and interleave work.
            await Task.Yield();
        }
    }

    /// <inheritdoc/>
    public bool DirectoryExists(string path)
        => Directory.Exists(path);

    /// <summary>
    /// Retrieves attributes for the given path directly from the OS.
    /// </summary>
    /// <inheritdoc/>
    public FileAttributes GetAttributes(string path)
        => File.GetAttributes(path);

    /// <inheritdoc/>
    public string ResolveRealPath(string path)
    {
        try
        {
            var attributes = File.GetAttributes(path);

            if(attributes.HasFlag(FileAttributes.ReparsePoint))
            {
                var target = attributes.HasFlag(FileAttributes.Directory)
                    ? Directory.ResolveLinkTarget(path, returnFinalTarget: true)
                    : File.ResolveLinkTarget(path, returnFinalTarget: true);

                return target?.FullName ?? path;
            }
        }
        catch
        {
            // If we cannot resolve (e.g. permission error), return the original path.
            // The engine will handle the access error during enumeration.
        }

        return path;
    }

    /// <inheritdoc/>
    public string GetFullPath(string path) => Path.GetFullPath(path);
    /// <inheritdoc/>
    public string GetFullPath(string path, string basePath) => Path.GetFullPath(path, basePath);

    private static bool TryGetAttributes(
        string path,
        bool ignoreInaccessible,
        FileQueryErrorRecoveryOptions errorRecovery,
        out FileAttributes attributes
    )
    {
        attributes = default;
        var attempts = errorRecovery.Action is FileQueryErrorAction.Retry
            ? errorRecovery.MaxRetryAttempts + 1
            : 1;

        for(var attempt = 0; attempt < attempts; attempt++)
        {
            try
            {
                attributes = File.GetAttributes(path);
                return true;
            }
            catch(Exception ex) when(FileSystemGuards.IsRecoverable(ex))
            {
                if(FileSystemGuards.ShouldSkip(ignoreInaccessible, errorRecovery, attempt, attempts))
                {
                    return false;
                }

                if(errorRecovery.Action is FileQueryErrorAction.Retry && attempt < attempts - 1)
                {
                    continue;
                }

                throw;
            }
        }

        return false;
    }

    private static bool TryEnsureAccessible(
        string directory,
        bool ignoreInaccessible,
        FileQueryErrorRecoveryOptions errorRecovery
    )
    {
        var attempts = errorRecovery.Action is FileQueryErrorAction.Retry
            ? errorRecovery.MaxRetryAttempts + 1
            : 1;

        for(var attempt = 0; attempt < attempts; attempt++)
        {
            try
            {
                FileSystemGuards.EnsureAccessible(directory, ignoreInaccessible);
                return true;
            }
            catch(Exception ex) when(FileSystemGuards.IsRecoverable(ex))
            {
                if(FileSystemGuards.ShouldSkip(ignoreInaccessible, errorRecovery, attempt, attempts))
                {
                    return false;
                }

                if(errorRecovery.Action is FileQueryErrorAction.Retry && attempt < attempts - 1)
                {
                    continue;
                }

                throw;
            }
        }

        return false;
    }
}
