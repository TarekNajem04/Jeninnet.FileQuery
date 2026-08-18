//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Compiled;

/// <summary>
/// Configuration for creating a <see cref="CompiledPattern"/>.
/// </summary>
/// <param name="IsNegated">Indicates if the pattern is negated.</param>
/// <param name="DirectoryOnly">Indicates if the pattern applies only to directories.</param>
/// <param name="AnchoredToRoot">Indicates if the pattern is anchored to the root.</param>
/// <param name="Segments">The segments of the pattern.</param>
/// <param name="PatternKind">The kind of the pattern.</param>
/// <param name="Intent">The intent of the compiled match.</param>
/// <param name="ConcretePathAnchor">The concrete path anchor.</param>
/// <param name="SourceText">The source text.</param>
/// <param name="SourceIndex">The source index.</param>
/// <param name="RegexText">The raw regex string.</param>
/// <param name="LiteralSuffix">The trailing literal suffix of the last segment, or an empty string when none applies.</param>
internal record CompiledPatternConfig(
    bool IsNegated,
    bool DirectoryOnly,
    bool AnchoredToRoot,
    IReadOnlyList<IReadOnlyList<IPatternToken>> Segments,
    PatternKind PatternKind,
    CompiledMatchIntent Intent,
    string ConcretePathAnchor,
    string SourceText = "",
    int SourceIndex = -1,
    string? RegexText = null,
    string LiteralSuffix = ""
);

/// <summary>
/// Represents a fully compiled, immutable pattern expression produced by a
/// <see cref="PatternScanner"/> implementation (GitIgnore, Glob, etc.).
/// </summary>
/// <remarks>
/// ARCHITECTURAL CONTRACT:
/// <list type="bullet">
/// <item>This type represents a fully validated, immutable pattern.</item>
/// <item>Instances must only be created by PatternCompilerBase.</item>
/// <item>Consumers must treat this as a read-only data structure.</item>
/// </list>
/// </remarks>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
internal sealed record CompiledPattern : ICompiledPattern {
    /// <summary>
    /// True if the pattern begins with <c>!</c>, negating matches.
    /// </summary>
    public bool IsNegated { get; init; }

    /// <summary>
    /// True if the pattern applies only to directories (ends with '/').
    /// </summary>
    public bool DirectoryOnly { get; init; }

    /// <inheritdoc/>
    public bool AnchoredToRoot { get; init; }

    /// <inheritdoc/>
    public IReadOnlyList<IReadOnlyList<IPatternToken>> Segments { get; init; }

    /// <inheritdoc/>
    public PatternKind PatternKind { get; init; }

    public CompiledMatchIntent Intent { get; init; }

    /// <inheritdoc/>
    public string SourceText { get; init; }

    /// <inheritdoc/>
    public int SourceIndex { get; init; }

    /// <inheritdoc/>
    public string ConcretePathAnchor { get; init; }

    /// <summary>
    /// Gets the raw regex string if the pattern is a Regex kind.
    /// </summary>
    public string? RegexText { get; init; }

    /// <inheritdoc/>
    public string LiteralSuffix { get; init; }

    internal CompiledPattern(CompiledPatternConfig config) {
        ArgumentNullException.ThrowIfNull(config);
        IsNegated = config.IsNegated;
        DirectoryOnly = config.DirectoryOnly;
        AnchoredToRoot = config.AnchoredToRoot;
        Segments = config.Segments ?? throw new ArgumentNullException(nameof(config));
        PatternKind = config.PatternKind;
        Intent = config.Intent;
        ConcretePathAnchor = config.ConcretePathAnchor ?? throw new ArgumentNullException(nameof(config));
        SourceText = config.SourceText;
        SourceIndex = config.SourceIndex;
        RegexText = config.RegexText;
        LiteralSuffix = config.LiteralSuffix;
    }

    /// <summary>
    /// Returns a debug string representation of this compiled pattern.
    /// </summary>
    public override string ToString() => string.Join("/", Segments.Select(static seg => string.Concat(seg.Select(static t => t.ToString() ?? "*"))));

    private string GetDebuggerDisplay() => ToString();
}
