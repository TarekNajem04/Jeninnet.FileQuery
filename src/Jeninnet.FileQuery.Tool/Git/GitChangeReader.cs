//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
using System.Globalization;

namespace Jeninnet.FileQuery.Tool.Git;

/// <summary>
/// Reads the files that changed between a Git baseline and the working tree.
/// </summary>
internal static class GitChangeReader {
    private const string REV_PARSE = "rev-parse";

    /// <summary>
    /// Reads the change set for a repository.
    /// </summary>
    /// <param name="directory">A directory inside the repository (or its root).</param>
    /// <param name="sinceCommit">The baseline commit, tag, or branch; when <see langword="null"/>, the timestamp or HEAD is used.</param>
    /// <param name="sinceTimestamp">The baseline time; the last commit before it is used.</param>
    /// <param name="includeUntracked">Whether untracked files are reported as added.</param>
    /// <returns>The change set relative to the repository root.</returns>
    /// <exception cref="GitException">Thrown when Git fails or the baseline is invalid.</exception>
    public static GitChangeSet Read(
        string directory,
        string? sinceCommit,
        DateTimeOffset? sinceTimestamp,
        bool includeUntracked
    ) {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);

        if(sinceCommit is not null && sinceTimestamp is not null) {
            throw new GitException("Specify either a commit or a timestamp baseline, not both.");
        }

        var repositoryRoot = ResolveRepositoryRoot(directory);
        var rootPrefix = ResolveRootPrefix(directory);
        var baseline = ResolveBaseline(repositoryRoot, sinceCommit, sinceTimestamp);
        var changes = ParseNameStatus(RunOrThrow(
            repositoryRoot,
            "diff",
            "--name-status",
            "-z",
            "-M",
            "--diff-filter=ACDMRT",
            baseline,
            "--").StandardOutput);

        if(includeUntracked) {
            AppendUntracked(repositoryRoot, changes);
        }

        return new GitChangeSet(repositoryRoot, rootPrefix, changes);
    }

    /// <summary>
    /// Resolves the absolute repository root for a directory.
    /// </summary>
    /// <param name="directory">A directory inside the repository.</param>
    /// <returns>The absolute repository root.</returns>
    /// <exception cref="GitException">Thrown when the directory is not inside a Git repository.</exception>
    private static string ResolveRepositoryRoot(string directory) {
        var result = GitProcess.Run(directory, REV_PARSE, "--show-toplevel");

        if(result.ExitCode != 0) {
            throw new GitException(
                $"Not a Git repository: '{directory}'. {result.StandardError.Trim()}".TrimEnd());
        }

        return Path.GetFullPath(result.StandardOutput.Trim());
    }

    /// <summary>
    /// Resolves the path from the repository root to the queried directory.
    /// </summary>
    /// <param name="directory">A directory inside the repository.</param>
    /// <returns>The forward-slash-terminated prefix, or an empty string at the repository root.</returns>
    /// <exception cref="GitException">Thrown when Git cannot resolve the prefix.</exception>
    private static string ResolveRootPrefix(string directory) {
        var prefix = RunOrThrow(directory, REV_PARSE, "--show-prefix")
            .StandardOutput
            .Trim()
            .Replace('\\', '/');

        if(prefix.Length == 0) {
            return string.Empty;
        }

        return prefix.EndsWith('/') ? prefix : prefix + "/";
    }

    /// <summary>
    /// Resolves the commit or tree that the working tree is compared against.
    /// </summary>
    /// <param name="repositoryRoot">The repository root.</param>
    /// <param name="sinceCommit">The explicit commit baseline.</param>
    /// <param name="sinceTimestamp">The explicit timestamp baseline.</param>
    /// <returns>The object identifier to diff against.</returns>
    /// <exception cref="GitException">Thrown when the revision is unknown.</exception>
    private static string ResolveBaseline(
        string repositoryRoot,
        string? sinceCommit,
        DateTimeOffset? sinceTimestamp
    ) {
        if(sinceCommit is not null) {
            var verify = GitProcess.Run(repositoryRoot, REV_PARSE, "--verify", $"{sinceCommit}^{{commit}}");

            if(verify.ExitCode != 0) {
                throw new GitException($"Unknown Git revision: '{sinceCommit}'.");
            }

            return verify.StandardOutput.Trim();
        }

        if(sinceTimestamp is not null) {
            var timestamp = sinceTimestamp.Value.ToUniversalTime()
                .ToString("yyyy-MM-ddTHH:mm:ss'Z'", CultureInfo.InvariantCulture);

            var revision = GitProcess.Run(repositoryRoot, "rev-list", "-1", $"--before={timestamp}", "HEAD");

            if(revision.ExitCode == 0 && revision.StandardOutput.Trim().Length > 0) {
                return revision.StandardOutput.Trim();
            }

            // No commit is older than the timestamp: every tracked file counts as added.
            return EmptyTree(repositoryRoot);
        }

        var head = GitProcess.Run(repositoryRoot, REV_PARSE, "--verify", "HEAD");

        return head.ExitCode == 0 && head.StandardOutput.Trim().Length > 0
            ? head.StandardOutput.Trim()
            : EmptyTree(repositoryRoot);
    }

    /// <summary>
    /// Returns the object identifier of an empty tree in the repository's hash format.
    /// </summary>
    /// <param name="repositoryRoot">The repository root.</param>
    /// <returns>The empty tree identifier.</returns>
    private static string EmptyTree(string repositoryRoot) =>
        RunOrThrow(repositoryRoot, "hash-object", "-t", "tree", "--stdin").StandardOutput.Trim();

    /// <summary>
    /// Parses NUL-separated <c>git diff --name-status -z</c> output.
    /// </summary>
    /// <param name="output">The raw Git output.</param>
    /// <returns>The parsed changes.</returns>
    private static List<GitChange> ParseNameStatus(string output) {
        var tokens = output.Split('\0');
        var changes = new List<GitChange>();
        var index = 0;

        while(index < tokens.Length) {
            var status = tokens[index++];

            if(status.Length == 0) {
                continue;
            }

            var kind = MapKind(status[0]);

            if(kind is GitChangeKind.Renamed or GitChangeKind.Copied) {
                if(tokens.Length - index < 2) {
                    break;
                }

                var previousPath = Normalize(tokens[index++]);
                var path = Normalize(tokens[index++]);
                changes.Add(new GitChange(kind, path, previousPath));
                continue;
            }

            if(index >= tokens.Length) {
                break;
            }

            changes.Add(new GitChange(kind, Normalize(tokens[index++])));
        }

        return changes;
    }

    /// <summary>
    /// Adds untracked files to the change set as added files.
    /// </summary>
    /// <param name="repositoryRoot">The repository root.</param>
    /// <param name="changes">The change list to extend.</param>
    private static void AppendUntracked(string repositoryRoot, List<GitChange> changes) {
        var existing = new HashSet<string>(
            changes.Select(static change => change.Path),
            StringComparer.Ordinal);

        var untracked = RunOrThrow(
            repositoryRoot,
            "ls-files",
            "--others",
            "--exclude-standard",
            "-z").StandardOutput;

        foreach(var path in untracked.Split('\0', StringSplitOptions.RemoveEmptyEntries)) {
            var normalized = Normalize(path);

            if(existing.Add(normalized)) {
                changes.Add(new GitChange(GitChangeKind.Added, normalized));
            }
        }
    }

    /// <summary>
    /// Maps a Git status letter to a change kind.
    /// </summary>
    /// <param name="status">The Git status letter.</param>
    /// <returns>The corresponding change kind.</returns>
    private static GitChangeKind MapKind(char status) => status switch {
        'A' => GitChangeKind.Added,
        'C' => GitChangeKind.Copied,
        'D' => GitChangeKind.Deleted,
        'M' => GitChangeKind.Modified,
        'R' => GitChangeKind.Renamed,
        'T' => GitChangeKind.TypeChanged,
        _ => GitChangeKind.Unknown
    };

    /// <summary>
    /// Normalizes a Git path to forward slashes.
    /// </summary>
    /// <param name="path">The raw Git path.</param>
    /// <returns>The normalized path.</returns>
    private static string Normalize(string path) => path.Replace('\\', '/');

    /// <summary>
    /// Runs Git and throws when the command fails.
    /// </summary>
    /// <param name="workingDirectory">The directory in which Git runs.</param>
    /// <param name="arguments">The Git arguments.</param>
    /// <returns>The Git result.</returns>
    /// <exception cref="GitException">Thrown when the command exits with a non-zero code.</exception>
    private static GitResult RunOrThrow(string workingDirectory, params string[] arguments) {
        var result = GitProcess.Run(workingDirectory, arguments);

        if(result.ExitCode != 0) {
            throw new GitException(
                $"git {string.Join(' ', arguments)} failed: {result.StandardError.Trim()}");
        }

        return result;
    }
}
