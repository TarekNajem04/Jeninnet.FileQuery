//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery;

/// <summary>
/// Describes an audited match decision for a filesystem entry.
/// </summary>
/// <param name="Path">The absolute path evaluated by the engine.</param>
/// <param name="RelativePath">The normalized path relative to the query root.</param>
/// <param name="EntryKind">The evaluated entry kind.</param>
/// <param name="Outcome">The final match outcome before traversal decisions are applied.</param>
/// <param name="Reason">A concise explanation for the outcome.</param>
/// <param name="PatternKind">The kind of pattern responsible for the outcome, when available.</param>
/// <param name="Pattern">The source pattern responsible for the outcome, when available.</param>
/// <param name="PatternIndex">The zero-based source pattern index responsible for the outcome, when available.</param>
public sealed record FileQueryDiagnostic(
    string Path,
    string RelativePath,
    string EntryKind,
    string Outcome,
    string Reason,
    PatternKind? PatternKind,
    string? Pattern,
    int? PatternIndex
) {
    /// <summary>
    /// Gets the entry length in bytes as reported by the enumeration, when available.
    /// </summary>
    public long? Length { get; init; }

    /// <summary>
    /// Gets the creation time in UTC as reported by the enumeration, when available.
    /// </summary>
    public DateTime? CreationTimeUtc { get; init; }

    /// <summary>
    /// Gets the last-write time in UTC as reported by the enumeration, when available.
    /// </summary>
    public DateTime? LastWriteTimeUtc { get; init; }

    /// <summary>
    /// Gets the raw attribute flags reported by the enumeration, when available.
    /// </summary>
    public FileAttributes? Attributes { get; init; }

    /// <summary>
    /// Gets a value indicating whether the audited entry is a directory, when available.
    /// </summary>
    public bool? IsDirectory { get; init; }
}
