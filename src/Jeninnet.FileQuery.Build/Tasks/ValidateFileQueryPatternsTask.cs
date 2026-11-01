//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
using Jeninnet.FileQuery.Tooling;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using MSBuildTask = Microsoft.Build.Utilities.Task;

namespace Jeninnet.FileQuery.Build.Tasks;

/// <summary>
/// Validates file query patterns during an MSBuild evaluation without executing a query.
/// </summary>
/// <remarks>
/// Patterns are supplied as items; the optional <c>SourceFile</c>, <c>SourceLine</c>, and
/// <c>SourceColumn</c> metadata are used for build diagnostics and SARIF locations.
/// </remarks>
public sealed class ValidateFileQueryPatternsTask : MSBuildTask {
    /// <summary>The diagnostic code reported for malformed patterns.</summary>
    public const string DiagnosticCode = "JNQ2001";

    /// <summary>
    /// Gets or sets the pattern items to validate.
    /// </summary>
    [Required]
    public ITaskItem[] Patterns { get; set; } = [];

    /// <summary>
    /// Gets or sets the optional path of a SARIF 2.1.0 file to write.
    /// </summary>
    public string? SarifOutput { get; set; }

    /// <summary>
    /// Gets or sets the project directory used to resolve a relative <see cref="SarifOutput"/> path.
    /// </summary>
    public string? ProjectDirectory { get; set; }

    /// <inheritdoc/>
    public override bool Execute() {
        var diagnostics = new List<PatternDiagnostic>();

        foreach(var item in Patterns) {
            var pattern = item.ItemSpec;
            var validation = PatternInspector.Validate(pattern);

            if(validation.IsValid) {
                continue;
            }

            var sourceFile = item.GetMetadata("SourceFile");
            var line = ReadPositiveInt(item, "SourceLine");
            var column = ReadPositiveInt(item, "SourceColumn");
            var message = validation.Error ?? "Malformed file query pattern.";

            diagnostics.Add(new PatternDiagnostic(
                pattern,
                message,
                validation.Position,
                validation.Segment,
                string.IsNullOrWhiteSpace(sourceFile) ? null : sourceFile,
                line,
                column));

            Log.LogError(
                subcategory: null,
                errorCode: DiagnosticCode,
                helpKeyword: null,
                file: string.IsNullOrWhiteSpace(sourceFile) ? null : sourceFile,
                lineNumber: line ?? 0,
                columnNumber: column ?? (validation.Position + 1) ?? 0,
                endLineNumber: 0,
                endColumnNumber: 0,
                message: "'{0}': {1}",
                messageArgs: [pattern, message]);
        }

        if(!string.IsNullOrWhiteSpace(SarifOutput)) {
            WriteSarif(diagnostics);
        }

        return !Log.HasLoggedErrors;
    }

    private void WriteSarif(List<PatternDiagnostic> diagnostics) {
        var sarifOutput = SarifOutput;

        if(string.IsNullOrWhiteSpace(sarifOutput)) {
            return;
        }

        var path = Path.IsPathRooted(sarifOutput)
            ? sarifOutput
            : Path.Combine(ProjectDirectory ?? Directory.GetCurrentDirectory(), sarifOutput);

        path = Path.GetFullPath(path);
        var directory = Path.GetDirectoryName(path);

        if(!string.IsNullOrEmpty(directory)) {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, SarifExporter.ToJson(diagnostics));
        Log.LogMessage(MessageImportance.Low, "Wrote SARIF diagnostics to '{0}'.", path);
    }

    private static int? ReadPositiveInt(ITaskItem item, string metadataName) {
        var value = item.GetMetadata(metadataName);
        return int.TryParse(value, out var parsed) && parsed > 0 ? parsed : null;
    }
}
