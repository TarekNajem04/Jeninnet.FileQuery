//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
using System.Buffers;
using System.Globalization;
using System.Text;
using System.Text.Json;
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
                ? new ImmediateProgress<FileQueryDiagnostic>(diagnostic => WriteDiagnostic(diagnostic, request.JsonOutput, output))
                : null);
        var engine = FileQueryRuntime.Create();

        if(request.Explain) {
            // The diagnostic sink writes each result; enumeration drives the traversal.
            _ = engine.Execute(query).Count();
            output.Flush();
            return CliApplication.EXIT_SUCCESS;
        }

        if(request.JsonOutput) {
            foreach(var entry in engine.ExecuteEntries(query)) {
                WriteEntryJson(entry, output);
            }

            output.Flush();
            return CliApplication.EXIT_SUCCESS;
        }

        var matches = 0L;

        foreach(var path in engine.Execute(query)) {
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
            Diagnostics: diagnostics,
            GitIgnoreFiles: request.GitIgnoreFiles) {
            IncludedPaths = includedPaths
        });

        return new FileQuery(request.Root, options);
    }

    /// <summary>
    /// Writes an explanation in text or JSON form.
    /// </summary>
    /// <param name="diagnostic">The diagnostic reported by the engine.</param>
    /// <param name="json">Whether to write JSON.</param>
    /// <param name="output">The writer that receives the result.</param>
    private static void WriteDiagnostic(FileQueryDiagnostic diagnostic, bool json, TextWriter output) {
        if(!string.Equals(diagnostic.EntryKind, "File", StringComparison.Ordinal)) {
            return;
        }

        if(json) {
            WriteDiagnosticJson(diagnostic, output);
            return;
        }

        WriteExplanation(diagnostic, output);
    }

    /// <summary>
    /// Writes one tab-separated explanation line for a file.
    /// </summary>
    /// <param name="diagnostic">The diagnostic reported by the engine.</param>
    /// <param name="output">The writer that receives the line.</param>
    private static void WriteExplanation(FileQueryDiagnostic diagnostic, TextWriter output) {
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
    /// Writes one newline-delimited JSON object for a matched entry.
    /// </summary>
    /// <param name="entry">The matched entry.</param>
    /// <param name="output">The writer that receives the line.</param>
    private static void WriteEntryJson(FileQueryEntry entry, TextWriter output) {
        var line = SerializeJson(writer => {
            writer.WriteString("path", entry.FullPath);
            writer.WriteString("relativePath", entry.RelativePath);
            writer.WriteNumber("length", entry.Length);
            writer.WriteString("createdUtc", entry.CreationTimeUtc);
            writer.WriteString("modifiedUtc", entry.LastWriteTimeUtc);
            WriteAttributes(writer, entry.Attributes);
            writer.WriteBoolean("isDirectory", entry.IsDirectory);
        });

        output.WriteLine(line);
    }

    /// <summary>
    /// Writes one newline-delimited JSON object for an explanation.
    /// </summary>
    /// <param name="diagnostic">The diagnostic reported by the engine.</param>
    /// <param name="output">The writer that receives the line.</param>
    private static void WriteDiagnosticJson(FileQueryDiagnostic diagnostic, TextWriter output) {
        var line = SerializeJson(writer => {
            writer.WriteString("path", diagnostic.Path);
            writer.WriteString("relativePath", diagnostic.RelativePath);
            writer.WriteString("entryKind", diagnostic.EntryKind);
            writer.WriteString("outcome", ToJsonOutcome(diagnostic.Outcome));
            writer.WriteString("reason", diagnostic.Reason);

            if(diagnostic.Length is { } length) {
                writer.WriteNumber("length", length);
            }

            if(diagnostic.CreationTimeUtc is { } createdUtc) {
                writer.WriteString("createdUtc", createdUtc);
            }

            if(diagnostic.LastWriteTimeUtc is { } lastWriteUtc) {
                writer.WriteString("modifiedUtc", lastWriteUtc);
            }

            if(diagnostic.Attributes is { } attributes) {
                WriteAttributes(writer, attributes);
            }

            if(diagnostic.IsDirectory is { } isDirectory) {
                writer.WriteBoolean("isDirectory", isDirectory);
            }

            writer.WriteString("patternKind", diagnostic.PatternKind?.ToString());
            writer.WriteString("pattern", diagnostic.Pattern);

            if(diagnostic.PatternIndex is { } patternIndex) {
                writer.WriteNumber("patternIndex", patternIndex);
            } else {
                writer.WriteNull("patternIndex");
            }
        });

        output.WriteLine(line);
    }

    /// <summary>
    /// Writes an attribute array with stable flag names.
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="attributes">The attribute flags.</param>
    private static void WriteAttributes(Utf8JsonWriter writer, FileAttributes attributes) {
        writer.WriteStartArray("attributes");

        foreach(var value in Enum.GetValues<FileAttributes>()) {
            if(value is not FileAttributes.None && (attributes & value) == value) {
                writer.WriteStringValue(value.ToString());
            }
        }

        writer.WriteEndArray();
    }

    /// <summary>
    /// Serializes a JSON object into a single line.
    /// </summary>
    /// <param name="writeProperties">The callback that writes the object properties.</param>
    /// <returns>The JSON text without a trailing newline.</returns>
    private static string SerializeJson(Action<Utf8JsonWriter> writeProperties) {
        var buffer = new ArrayBufferWriter<byte>();
        using(var writer = new Utf8JsonWriter(buffer)) {
            writer.WriteStartObject();
            writeProperties(writer);
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(buffer.WrittenSpan);
    }

    /// <summary>
    /// Maps the engine outcome to the stable lowercase JSON value.
    /// </summary>
    /// <param name="outcome">The engine outcome.</param>
    private static string ToJsonOutcome(string outcome) =>
        outcome switch {
            "Include" => "include",
            "Exclude" => "exclude",
            _ => "nomatch"
        };

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
