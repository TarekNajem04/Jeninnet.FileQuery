//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
using Jeninnet.FileQuery.IO;
using Microsoft.Extensions.FileProviders;

namespace Jeninnet.FileQuery.FileProviders;

/// <summary>
/// Adapts an <see cref="IFileProvider"/> to the engine's file-system contract.
/// </summary>
/// <remarks>
/// All paths are virtual, root-relative, and use forward slashes. Provider entries do not expose
/// raw attribute flags, so files are reported as <see cref="FileAttributes.Normal"/> and
/// directories as <see cref="FileAttributes.Directory"/>. The last-modified timestamp is mapped to
/// both timestamp fields because providers expose only one timestamp.
/// </remarks>
/// <param name="provider">The provider to adapt.</param>
internal sealed class FileProviderFileSystem(IFileProvider provider) : IFileSystem {
    private readonly IFileProvider _provider = provider;

    /// <inheritdoc/>
    public IEnumerable<FileSystemEntry> Enumerate(
        string directory,
        bool ignoreInaccessible,
        FileQueryErrorRecoveryOptions errorRecovery
    ) {
        var attempts = GetMaxAttempts(errorRecovery);
        var yielded = 0;
        var state = new EnumerationState();

        try {
            while(state.Attempt < attempts) {
                var result = TryMoveNext(
                    directory,
                    state,
                    ignoreInaccessible,
                    errorRecovery,
                    attempts,
                    out var info);

                if(result == EnumerationResult.Break) {
                    yield break;
                }

                if(result == EnumerationResult.Continue) {
                    state.Skip = yielded;
                    continue;
                }

                if(!info.Exists) {
                    continue;
                }

                yielded++;
                yield return ToEntry(directory, info);
            }
        }
        finally {
            state.Enumerator?.Dispose();
        }
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<FileSystemEntry> EnumerateAsync(
        string directory,
        bool ignoreInaccessible,
        FileQueryErrorRecoveryOptions errorRecovery,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default
    ) {
        // Yield once so the first MoveNextAsync does not complete entirely on the
        // caller's thread. Per-entry yielding forced a thread pool hop per file.
        await Task.Yield();

        foreach(var entry in Enumerate(directory, ignoreInaccessible, errorRecovery)) {
            cancellationToken.ThrowIfCancellationRequested();
            yield return entry;
        }
    }

    /// <inheritdoc/>
    public FileAttributes GetAttributes(string path) =>
        DirectoryExists(path) ? FileAttributes.Directory : FileAttributes.Normal;

    /// <inheritdoc/>
    public bool DirectoryExists(string path) {
        var normalized = GetFullPath(path);

        if(normalized is "/") {
            return true;
        }

        return _provider.GetDirectoryContents(ToProviderPath(normalized)).Exists;
    }

    /// <inheritdoc/>
    public bool FileExists(string path) {
        var info = _provider.GetFileInfo(ToProviderPath(GetFullPath(path)));
        return info.Exists && !info.IsDirectory;
    }

    /// <inheritdoc/>
    public string[] ReadAllLines(string path) {
        var info = _provider.GetFileInfo(ToProviderPath(GetFullPath(path)));

        if(!info.Exists || info.IsDirectory) {
            throw new FileNotFoundException($"The provider does not contain file '{path}'.", path);
        }

        using var stream = info.CreateReadStream();
        using var reader = new StreamReader(stream);
        var lines = new List<string>();
        string? line;

        while((line = reader.ReadLine()) is not null) {
            lines.Add(line);
        }

        return [.. lines];
    }

    /// <inheritdoc/>
    public string ResolveRealPath(string path) => GetFullPath(path);

    /// <inheritdoc/>
    public char DirectorySeparator => '/';

    /// <inheritdoc/>
    public string GetFullPath(string path) {
        if(string.IsNullOrWhiteSpace(path)) {
            return "/";
        }

        var normalized = path.Replace('\\', '/').Trim();

        if(!normalized.StartsWith('/')) {
            normalized = "/" + normalized;
        }

        normalized = normalized.TrimEnd('/');
        return normalized.Length == 0 ? "/" : normalized;
    }

    /// <inheritdoc/>
    public string GetFullPath(string path, string basePath) =>
        GetFullPath($"{GetFullPath(basePath).TrimEnd('/')}/{path}");

    private IEnumerator<IFileInfo> CreateEnumerator(string directory) =>
        _provider.GetDirectoryContents(ToProviderPath(GetFullPath(directory))).GetEnumerator();

    private static int GetMaxAttempts(FileQueryErrorRecoveryOptions errorRecovery) {
        if(errorRecovery.Action is not FileQueryErrorAction.Retry) {
            return 1;
        }

        return errorRecovery.MaxRetryAttempts == int.MaxValue
            ? int.MaxValue
            : errorRecovery.MaxRetryAttempts + 1;
    }

    private sealed class EnumerationState {
        public IEnumerator<IFileInfo>? Enumerator;
        public int Attempt;
        public int Skip;
    }

    private EnumerationResult TryMoveNext(
        string directory,
        EnumerationState state,
        bool ignoreInaccessible,
        FileQueryErrorRecoveryOptions errorRecovery,
        int attempts,
        out IFileInfo info
    ) {
        info = null!;

        try {
            if(state.Enumerator is null) {
                state.Enumerator = CreateEnumerator(directory);

                while(state.Skip > 0) {
                    if(!state.Enumerator.MoveNext()) {
                        state.Enumerator.Dispose();
                        state.Enumerator = null;
                        return EnumerationResult.Break;
                    }

                    state.Skip--;
                }
            }

            if(state.Enumerator.MoveNext()) {
                info = state.Enumerator.Current;
                return EnumerationResult.Success;
            }

            state.Enumerator.Dispose();
            return EnumerationResult.Break;
        }
        catch(Exception ex) when(FileSystemGuards.IsRecoverable(ex)) {
            state.Enumerator?.Dispose();
            state.Enumerator = null;

            if(FileSystemGuards.ShouldSkip(ignoreInaccessible, errorRecovery, state.Attempt, attempts)) {
                return EnumerationResult.Break;
            }

            if(errorRecovery.Action is FileQueryErrorAction.Retry && state.Attempt < attempts - 1) {
                state.Attempt++;
                return EnumerationResult.Continue;
            }

            throw;
        }
    }

    private enum EnumerationResult {
        Success = 0,
        Break = 1,
        Continue = 2
    }

    private FileSystemEntry ToEntry(string directory, IFileInfo info) {
        var fullPath = GetFullPath(info.Name, directory);
        var attributes = info.IsDirectory ? FileAttributes.Directory : FileAttributes.Normal;
        var modified = info.LastModified.UtcDateTime;

        return new FileSystemEntry(
            fullPath,
            attributes,
            info.IsDirectory ? 0 : info.Length,
            modified,
            modified);
    }

    private static string ToProviderPath(string path) => path.TrimStart('/');
}
