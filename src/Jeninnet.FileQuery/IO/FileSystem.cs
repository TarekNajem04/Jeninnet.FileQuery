//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
using System.IO.Enumeration;
using SystemFileSystemEntry = System.IO.Enumeration.FileSystemEntry;

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
        IEnumerator<FileSystemEntry>? enumerator = null;

        while(attempt < attempts) {
            var res = TryMoveNext(directory, ref enumerator, ignoreInaccessible, errorRecovery, ref attempt, attempts, out var entry);
            if(res == EnumerationResult.Break) {
                yield break;
            }

            if(res == EnumerationResult.Continue) {
                continue;
            }

            if(entry.IsDirectory && !TryEnsureAccessible(entry.FullPath, ignoreInaccessible, errorRecovery)) {
                continue;
            }

            yield return entry;
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
        IEnumerator<FileSystemEntry>? enumerator = null;

        while(attempt < attempts) {
            cancellationToken.ThrowIfCancellationRequested();

            var res = TryMoveNext(directory, ref enumerator, ignoreInaccessible, errorRecovery, ref attempt, attempts, out var entry);
            if(res == EnumerationResult.Break) {
                yield break;
            }

            if(res == EnumerationResult.Continue) {
                continue;
            }

            if(entry.IsDirectory && !TryEnsureAccessible(entry.FullPath, ignoreInaccessible, errorRecovery)) {
                continue;
            }

            yield return entry;
            await Task.Yield();
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
        ref IEnumerator<FileSystemEntry>? enumerator,
        bool ignoreInaccessible,
        FileQueryErrorRecoveryOptions errorRecovery,
        ref int attempt,
        int attempts,
        out FileSystemEntry entry
    ) {
        entry = default;
        try {
            enumerator ??= CreateEnumerator(directory);
            if(enumerator.MoveNext()) {
                entry = enumerator.Current;
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

    /// <summary>
    /// Creates an enumerator over the specified directory whose entries carry the
    /// attributes provided directly by the OS enumeration.
    /// </summary>
    /// <param name="directory">The directory to enumerate.</param>
    /// <remarks>
    /// <para>
    /// The enumeration is configured exactly like <see cref="Directory.EnumerateFileSystemEntries(string)"/>:
    /// every entry is returned regardless of its attributes (<see cref="EnumerationOptions.AttributesToSkip"/>
    /// is zero) and access errors surface as exceptions so the caller's skip/retry
    /// policy can apply (<see cref="EnumerationOptions.IgnoreInaccessible"/> is disabled).
    /// </para>
    /// <para>
    /// This replaces the previous per-entry <see cref="File.GetAttributes(string)"/>
    /// call: on Windows, <see cref="SystemFileSystemEntry.Attributes"/> is populated
    /// from the <c>WIN32_FIND_DATA</c> returned by the enumeration itself, eliminating
    /// one attribute lookup (and its internal full-path normalization) per entry.
    /// Reparse points are reported without following the link, matching the
    /// <see cref="File.GetAttributes(string)"/> semantics.
    /// </para>
    /// </remarks>
    private static IEnumerator<FileSystemEntry> CreateEnumerator(string directory) {
        var options = new EnumerationOptions {
            AttributesToSkip = 0,
            IgnoreInaccessible = false
        };

        return new FileSystemEnumerable<FileSystemEntry>(directory, TransformEntry, options).GetEnumerator();
    }

    /// <summary>
    /// Builds a <see cref="FileSystemEntry"/> from the raw OS enumeration data.
    /// </summary>
    /// <param name="entry">The raw enumeration entry.</param>
    /// <returns>The file system entry used by the engine.</returns>
    private static FileSystemEntry TransformEntry(ref SystemFileSystemEntry entry) {
        var path = entry.ToFullPath();
        return new FileSystemEntry(path, entry.Attributes);
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
