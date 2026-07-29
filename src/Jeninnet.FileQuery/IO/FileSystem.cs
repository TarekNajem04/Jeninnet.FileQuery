namespace Jeninnet.FileQuery.IO;

/// <summary>
/// The default implementation of the file system abstraction.
/// </summary>
/// <remarks>
/// Wraps standard <see cref="System.IO"/> APIs. This class is implemented as a singleton
/// to avoid overhead and provide a consistent access point for the engine.
/// </remarks>
internal sealed class FileSystem : IFileSystem {
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
    ) {
        var attempts = GetMaxAttempts(errorRecovery);
        var attempt = 0;
        IEnumerator<string>? enumerator = null;

        while(attempt < attempts) {
            var res = TryMoveNext(directory, ref enumerator, ignoreInaccessible, errorRecovery, ref attempt, attempts, out var path);
            if(res == EnumerationResult.Break) {
                yield break;
            }

            if(res == EnumerationResult.Continue) {
                continue;
            }

            if(TryCreateEntry(path!, ignoreInaccessible, errorRecovery, out var entry)) {
                yield return entry;
            }
        }
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<FileSystemEntry> EnumerateAsync(
        string directory,
        bool ignoreInaccessible,
        FileQueryErrorRecoveryOptions errorRecovery,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    ) {
        if(!TryEnsureAccessible(directory, ignoreInaccessible, errorRecovery)) {
            yield break;
        }

        var attempts = GetMaxAttempts(errorRecovery);
        var attempt = 0;
        IEnumerator<string>? enumerator = null;

        while(attempt < attempts) {
            cancellationToken.ThrowIfCancellationRequested();

            var res = TryMoveNext(directory, ref enumerator, ignoreInaccessible, errorRecovery, ref attempt, attempts, out var path);
            if(res == EnumerationResult.Break) {
                yield break;
            }

            if(res == EnumerationResult.Continue) {
                continue;
            }

            if(TryCreateEntry(path!, ignoreInaccessible, errorRecovery, out var entry)) {
                yield return entry;
                await Task.Yield();
            }
        }
    }

    /// <inheritdoc/>
    public bool DirectoryExists(string path) => Directory.Exists(path);

    /// <summary>
    /// Retrieves attributes for the given path directly from the OS.
    /// </summary>
    /// <inheritdoc/>
    public FileAttributes GetAttributes(string path) => File.GetAttributes(path);

    /// <inheritdoc/>
    public string ResolveRealPath(string path) {
        try {
            var attributes = File.GetAttributes(path);

            if(attributes.HasFlag(FileAttributes.ReparsePoint)) {
                var target = attributes.HasFlag(FileAttributes.Directory)
                    ? Directory.ResolveLinkTarget(path, returnFinalTarget: true)
                    : File.ResolveLinkTarget(path, returnFinalTarget: true);

                return target?.FullName ?? path;
            }
        }
        catch {
            // If we cannot resolve (e.g. permission error), return the original path.
            // The engine will handle the access error during enumeration.
        }

        return path;
    }

    /// <inheritdoc/>
    public char DirectorySeparator => Path.DirectorySeparatorChar;

    /// <inheritdoc/>
    public string GetFullPath(string path) => Path.GetFullPath(path);

    /// <inheritdoc/>
    public string GetFullPath(string path, string basePath) => Path.GetFullPath(path, basePath);

    private static int GetMaxAttempts(FileQueryErrorRecoveryOptions errorRecovery) => errorRecovery.Action is FileQueryErrorAction.Retry ? errorRecovery.MaxRetryAttempts + 1 : 1;

    private enum EnumerationResult { Success = 0, Break = 1, Continue = 2 }

    private static EnumerationResult TryMoveNext(
        string directory,
        ref IEnumerator<string>? enumerator,
        bool ignoreInaccessible,
        FileQueryErrorRecoveryOptions errorRecovery,
        ref int attempt,
        int attempts,
        out string? path
    ) {
        path = null;
        try {
            enumerator ??= Directory.EnumerateFileSystemEntries(directory).GetEnumerator();
            if(enumerator.MoveNext()) {
                path = enumerator.Current;
                return EnumerationResult.Success;
            }

            enumerator.Dispose();
            return EnumerationResult.Break;
        }
        catch(Exception ex) when(FileSystemGuards.IsRecoverable(ex)) {
            enumerator?.Dispose();
            enumerator = null;

            if(FileSystemGuards.ShouldSkip(ignoreInaccessible, errorRecovery, attempt, attempts)) {
                return EnumerationResult.Break;
            }

            if(errorRecovery.Action is FileQueryErrorAction.Retry && attempt < attempts - 1) {
                attempt++;
                return EnumerationResult.Continue;
            }

            throw;
        }
    }

    private static bool TryCreateEntry(string path, bool ignoreInaccessible, FileQueryErrorRecoveryOptions errorRecovery, out FileSystemEntry entry) {
        entry = default;
        if(!TryGetAttributes(path, ignoreInaccessible, errorRecovery, out var attributes)) {
            return false;
        }

        if(attributes.HasFlag(FileAttributes.Directory) && !TryEnsureAccessible(path, ignoreInaccessible, errorRecovery)) {
            return false;
        }

        entry = new FileSystemEntry(path, attributes);
        return true;
    }

    private static bool TryGetAttributes(
        string path,
        bool ignoreInaccessible,
        FileQueryErrorRecoveryOptions errorRecovery,
        out FileAttributes attributes
    ) {
        attributes = default;
        var attempts = GetMaxAttempts(errorRecovery);

        for(var attempt = 0; attempt < attempts; attempt++) {
            try {
                attributes = File.GetAttributes(path);
                return true;
            }
            catch(Exception ex) when(FileSystemGuards.IsRecoverable(ex)) {
                if(FileSystemGuards.ShouldSkip(ignoreInaccessible, errorRecovery, attempt, attempts)) {
                    return false;
                }

                if(errorRecovery.Action is FileQueryErrorAction.Retry && attempt < attempts - 1) {
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
    ) {
        var attempts = GetMaxAttempts(errorRecovery);

        for(var attempt = 0; attempt < attempts; attempt++) {
            try {
                FileSystemGuards.EnsureAccessible(directory, ignoreInaccessible);
                return true;
            }
            catch(Exception ex) when(FileSystemGuards.IsRecoverable(ex)) {
                if(FileSystemGuards.ShouldSkip(ignoreInaccessible, errorRecovery, attempt, attempts)) {
                    return false;
                }

                if(errorRecovery.Action is FileQueryErrorAction.Retry && attempt < attempts - 1) {
                    continue;
                }

                throw;
            }
        }

        return false;
    }
}
