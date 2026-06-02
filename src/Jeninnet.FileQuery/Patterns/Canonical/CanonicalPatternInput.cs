namespace Jeninnet.FileQuery.Patterns.Canonical;

/// <summary>
/// Represents the canonical, normalized input to the pattern engine.
/// </summary>
internal sealed record CanonicalPatternInput {
    private static readonly ImmutableDictionary<PatternKind, ImmutableArray<string>>
        _emptyTypedPatterns = ImmutableDictionary<PatternKind, ImmutableArray<string>>.Empty;

    /// <summary>
    /// Gets the normalized immutable list of patterns.
    /// </summary>
    public ImmutableArray<string> Patterns { get; }

    /// <summary>
    /// Gets explicitly typed patterns.
    /// </summary>
    public ImmutableDictionary<PatternKind, ImmutableArray<string>> TypedPatterns { get; }

    /// <summary>
    /// Gets the interpretation mode.
    /// </summary>
    public PatternInterpretationMode InterpretationMode { get; }

    public CanonicalPatternInput(
        IEnumerable<string>? patterns = null,
        IReadOnlyDictionary<PatternKind, IEnumerable<string>>? typedPatterns = null,
        PatternInterpretationMode interpretationMode = PatternInterpretationMode.Hybrid) {
        Patterns = patterns is null
            ? ImmutableArray<string>.Empty
            : patterns.ToImmutableArray();

        if(typedPatterns is null || typedPatterns.Count == 0) {
            TypedPatterns = _emptyTypedPatterns;
        } else {
            var builder = ImmutableDictionary.CreateBuilder<PatternKind, ImmutableArray<string>>();

            foreach(var (type, list) in typedPatterns) {
                builder[type] = list is null
                    ? ImmutableArray<string>.Empty
                    : list.ToImmutableArray();
            }

            TypedPatterns = builder.ToImmutable();
        }

        InterpretationMode = interpretationMode;
    }
}
