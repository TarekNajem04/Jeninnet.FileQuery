//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
using System.Globalization;
using Jeninnet.FileQuery.Tool.Git;
using Jeninnet.FileQuery.Traversal;

namespace Jeninnet.FileQuery.Tool;

/// <summary>
/// Executes a validated <see cref="QueryRequest"/> and writes its matches.
/// </summary>
internal static class QueryRunner {
    /// <summary>
    /// Runs the query and writes matching paths (or the match count) to <paramref name="output"/>.
    /// </summary>
    /// <param name="request">The validated query request.</param>
    /// <param name="output">The writer that receives the results.</param>
    /// <returns>The process exit code.</returns>
    public static int Run(QueryRequest request, TextWriter output) {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(output);

        var query = CreateQuery(
            request,
            ResolveIncludedPaths(request),
            request.Explain
                ? new ImmediateProgress<FileQueryDiagnostic>(diagnostic => WriteExplanation(diagnostic, output))
                : null);
        var engine = FileQueryRuntime.Create();
        var matches = 0L;

        foreach(var path in engine.Execute(query)) {
            if(request.Explain) {
                continue;
            }

            matches++;

            if(request.CountOnly) {
                continue;
            }

            var value = request.RelativePaths
                ? Path.GetRelativePath(request.Root, path)
                : path;

            output.Write(value);
            output.Write(request.NullSeparated ? '\0' : '\n');
        }

        if(request.CountOnly) {
            output.WriteLine(matches.ToString(CultureInfo.InvariantCulture));
        }

        output.Flush();

        return CliApplication.EXIT_SUCCESS;
    }

    /// <summary>
    /// Creates the immutable engine query from the validated request.
    /// </summary>
    /// <param name="request">The validated query request.</param>
    /// <param name="includedPaths">The optional allow-list of root-relative paths.</param>
    /// <param name="diagnostics">The optional diagnostic sink used by explain mode.</param>
    /// <returns>The query descriptor passed to the engine.</returns>
    private static FileQuery CreateQuery(
        QueryRequest request,
        IReadOnlyCollection<string>? includedPaths,
        IProgress<FileQueryDiagnostic>? diagnostics
    ) {
        var patternInput = new PatternInput(
            TypedPatterns: request.TypedPatterns,
            InterpretationMode: request.InterpretationMode);

        var options = new FileQueryOptions(new FileQueryOptionsConfig(
            PatternInput: patternInput,
            RecurseSubdirectories: request.RecurseSubdirectories,
            MaxRecursionDepth: request.MaxRecursionDepth,
            IgnoreInaccessible: true,
            PatternMatchingMode: request.MatchingMode,
            CaseSensitivity: request.CaseSensitivity,
            Traversal: new TraversalOptions(SymlinkPolicy: request.SymlinkPolicy),
            AuditMatches: diagnostics is not null,
            Diagnostics: diagnostics) {
            IncludedPaths = includedPaths
        });

        return new FileQuery(request.Root, options);
    }

    /// <summary>
    /// Writes one tab-separated explanation line for a file.
    /// </summary>
    /// <param name="diagnostic">The diagnostic reported by the engine.</param>
    /// <param name="output">The writer that receives the line.</param>
    private static void WriteExplanation(FileQueryDiagnostic diagnostic, TextWriter output) {
        if(!string.Equals(diagnostic.EntryKind, "File", StringComparison.Ordinal)) {
            return;
        }

        var outcome = string.Equals(diagnostic.Outcome, "Include", StringComparison.Ordinal)
            ? "include"
            : "exclude";
        var reason = diagnostic.Reason
            .Replace('\t', ' ')
            .Replace('\r', ' ')
            .Replace('\n', ' ');

        output.Write(outcome);
        output.Write('\t');
        output.Write(diagnostic.RelativePath);
        output.Write('\t');
        output.WriteLine(reason);
    }

    /// <summary>
    /// Resolves the allow-list for a diff-aware query.
    /// </summary>
    /// <param name="request">The validated query request.</param>
    /// <returns>The allowed root-relative paths, or <see langword="null"/> when the query is not diff-aware.</returns>
    private static IReadOnlyCollection<string>? ResolveIncludedPaths(QueryRequest request) {
        if(request.ChangedSince is null && request.ChangedSinceDate is null) {
            return null;
        }

        var changeSet = GitChangeReader.Read(
            request.Root,
            request.ChangedSince,
            request.ChangedSinceDate,
            request.IncludeUntracked);

        return RebaseToRoot(request.Root, changeSet);
    }

    /// <summary>
    /// Rebases repository-relative change paths onto the query root.
    /// </summary>
    /// <param name="root">The query root.</param>
    /// <param name="changeSet">The change set reported by Git.</param>
    /// <returns>The paths relative to <paramref name="root"/>.</returns>
    /// <exception cref="GitException">Thrown when the query root is outside the repository.</exception>
    private static IReadOnlyList<string> RebaseToRoot(string root, GitChangeSet changeSet) {
        var relativeRoot = Path.GetRelativePath(changeSet.RepositoryRoot, root);

        if(relativeRoot is ".") {
            return changeSet.ExistingPaths;
        }

        if(Path.IsPathRooted(relativeRoot) ||
            relativeRoot == ".." ||
            relativeRoot.StartsWith("../", StringComparison.Ordinal) ||
            relativeRoot.StartsWith(@"..\", StringComparison.Ordinal)) {
            throw new GitException(
                $"The query root '{root}' is outside the Git repository '{changeSet.RepositoryRoot}'.");
        }

        // Windows path comparison is case-insensitive; Git reports the on-disk casing.
        var comparison = OperatingSystem.IsLinux()
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;
        var prefix = relativeRoot.Replace('\\', '/') + "/";

        return [.. changeSet.ExistingPaths
            .Where(path => path.StartsWith(prefix, comparison))
            .Select(path => path[prefix.Length..])];
    }
}
