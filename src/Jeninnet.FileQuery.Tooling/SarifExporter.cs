//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
using System.Buffers;
using System.Text;
using System.Text.Json;

namespace Jeninnet.FileQuery.Tooling;

/// <summary>
/// Projects pattern validation diagnostics into a SARIF 2.1.0 document.
/// </summary>
/// <remarks>
/// The output is a single-run SARIF log suitable for GitHub code scanning and other SARIF
/// consumers. Results carry a source location when <see cref="PatternDiagnostic.FilePath"/> is
/// supplied, plus the pattern text as a result property for traceability.
/// </remarks>
public static class SarifExporter {
    /// <summary>The rule identifier used for malformed file query patterns.</summary>
    public const string RuleId = "JNQ2001";

#pragma warning disable S1075 // Stable schema and documentation URLs are part of the SARIF contract.
    private const string SCHEMA_URI = "https://json.schemastore.org/sarif-2.1.0.json";
    private const string TOOL_NAME = "Jeninnet.FileQuery";
    private const string HELP_URI = "https://github.com/TarekNajem04/Jeninnet.FileQuery/blob/main/docs/guides/msbuild-validation.md";
#pragma warning restore S1075

    /// <summary>
    /// Serializes diagnostics as a SARIF 2.1.0 JSON string.
    /// </summary>
    /// <param name="diagnostics">The diagnostics to export.</param>
    /// <returns>The SARIF document.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="diagnostics"/> is <see langword="null"/>.</exception>
    public static string ToJson(IEnumerable<PatternDiagnostic> diagnostics) {
        ArgumentNullException.ThrowIfNull(diagnostics);

        var buffer = new ArrayBufferWriter<byte>();

        using(var writer = new Utf8JsonWriter(buffer)) {
            writer.WriteStartObject();
            writer.WriteString("$schema", SCHEMA_URI);
            writer.WriteString("version", "2.1.0");
            writer.WriteStartArray("runs");
            writer.WriteStartObject();

            WriteTool(writer);
            WriteResults(writer, diagnostics);

            writer.WriteEndObject();
            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(buffer.WrittenSpan);
    }

    /// <summary>
    /// Writes the SARIF tool descriptor.
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    private static void WriteTool(Utf8JsonWriter writer) {
        writer.WriteStartObject("tool");
        writer.WriteStartObject("driver");
        writer.WriteString("name", TOOL_NAME);
        writer.WriteString("informationUri", "https://github.com/TarekNajem04/Jeninnet.FileQuery");
        writer.WriteStartArray("rules");
        writer.WriteStartObject();
        writer.WriteString("id", RuleId);
        writer.WriteStartObject("shortDescription");
        writer.WriteString("text", "Malformed file query pattern");
        writer.WriteEndObject();
        writer.WriteString("helpUri", HELP_URI);
        writer.WriteEndObject();
        writer.WriteEndArray();
        writer.WriteEndObject();
        writer.WriteEndObject();
    }

    /// <summary>
    /// Writes all SARIF results.
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="diagnostics">The diagnostics to export.</param>
    private static void WriteResults(Utf8JsonWriter writer, IEnumerable<PatternDiagnostic> diagnostics) {
        writer.WriteStartArray("results");

        foreach(var diagnostic in diagnostics) {
            writer.WriteStartObject();
            writer.WriteString("ruleId", RuleId);
            writer.WriteString("level", "error");

            writer.WriteStartObject("message");
            writer.WriteString("text", $"'{diagnostic.Pattern}': {diagnostic.Message}");
            writer.WriteEndObject();

            if(!string.IsNullOrWhiteSpace(diagnostic.FilePath)) {
                writer.WriteStartArray("locations");
                writer.WriteStartObject();
                writer.WriteStartObject("physicalLocation");
                writer.WriteStartObject("artifactLocation");
                writer.WriteString("uri", ToUri(diagnostic.FilePath));
                writer.WriteEndObject();

                if(diagnostic.Line is not null || diagnostic.Position is not null) {
                    writer.WriteStartObject("region");
                    writer.WriteNumber("startLine", diagnostic.Line ?? 1);
                    writer.WriteNumber("startColumn", diagnostic.Column ?? (diagnostic.Position + 1) ?? 1);
                    writer.WriteEndObject();
                }

                writer.WriteEndObject();
                writer.WriteEndObject();
                writer.WriteEndArray();
            }

            writer.WriteStartObject("properties");
            writer.WriteString("pattern", diagnostic.Pattern);

            if(diagnostic.Segment is { } segment) {
                writer.WriteNumber("segment", segment);
            }

            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }

    /// <summary>
    /// Converts a filesystem path into a SARIF artifact URI.
    /// </summary>
    /// <param name="path">The source path.</param>
    /// <returns>A forward-slash, percent-escaped relative URI.</returns>
    private static string ToUri(string path) {
        var normalized = path.Replace('\\', '/');

        if(normalized.Length > 1 && normalized[1] == ':') {
            normalized = normalized[2..].TrimStart('/');
        }

        normalized = normalized.TrimStart('/');
        var segments = normalized.Split('/');
        var escaped = new string[segments.Length];

        for(var i = 0; i < segments.Length; i++) {
            escaped[i] = Uri.EscapeDataString(segments[i]);
        }

        return string.Join("/", escaped);
    }
}
