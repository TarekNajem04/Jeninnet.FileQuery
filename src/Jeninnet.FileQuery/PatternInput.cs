//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery;

/// <summary>
/// Represents the pattern configuration for a single file query.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="PatternInput"/> is the public boundary between the caller and the pattern compilation pipeline.
/// It expresses intent using standard BCL types and is deliberately free of implementation details such as
/// immutable collections or internal classification structures.
/// </para>
/// <para>
/// <strong>Separation of concerns:</strong> The internal pipeline converts
/// <see cref="PatternInput"/> into canonicalized and classified structures before
/// pattern compilation begins. Callers never interact with those intermediate forms.
/// </para>
/// <para>
/// <strong>Evaluation model:</strong> Patterns in <see cref="Patterns"/> are classified
/// automatically when <see cref="InterpretationMode"/> is
/// <see cref="PatternInterpretationMode.Hybrid"/>. Patterns in
/// <see cref="TypedPatterns"/> bypass classification — their dialect is declared explicitly.
/// </para>
/// <para>
/// <strong>Nullability:</strong> The constructor accepts <see langword="null"/>
/// for <c>typedPatterns</c> to represent no typed patterns, but
/// <see cref="TypedPatterns"/> itself never returns <see langword="null"/>.
/// Consumers can enumerate it directly and should treat an empty dictionary as
/// the absence of explicitly typed patterns.
/// </para>
/// <para>
/// <strong>Rule ordering:</strong> Within each dialect, patterns are evaluated
/// sequentially in the order they were provided and the last matching rule wins.
/// Across dialects, the compiled subsets are evaluated in the fixed order
/// GitIgnore → Glob → Regex; the first subset that returns <c>Include</c> wins.
/// A later dialect can override an earlier Exclude, but not an earlier Include.
/// </para>
/// </remarks>
/// <example>
/// Untyped hybrid input — patterns are auto-classified:
/// <code>
/// var input = new PatternInput(
///     patterns: ["**", "!*.log", "important.log"]
/// );
/// </code>
/// </example>
/// <example>
/// Mixed typed input — GitIgnore and Regex patterns declared explicitly:
/// <code>
/// var input = new PatternInput(
///     typedPatterns: new Dictionary&lt;PatternKind, IEnumerable&lt;string&gt;&gt;
///     {
///         [PatternKind.GitIgnore] = ["**", "!*.cs"],
///         [PatternKind.Regex]     = ["r:^data_.*\\.log$"]
///     }
/// );
/// </code>
/// </example>
public sealed record PatternInput {
    // ------------------------------------------------------------------
    // Shared empty instances — avoid repeated allocation of empty
    // collections when the default constructor is used.
    // ------------------------------------------------------------------

    private static readonly IReadOnlyList<string> _emptyPatterns
        = [];

    private static readonly IReadOnlyDictionary<PatternKind, IReadOnlyList<string>> _emptyTypedPatterns
        = new Dictionary<PatternKind, IReadOnlyList<string>>();

    /// <summary>
    /// Gets the ordered list of untyped pattern strings.
    /// </summary>
    /// <remarks>
    /// These patterns are classified automatically by the engine using
    /// <see cref="PatternInterpretationMode"/>. An empty list is valid
    /// and means no untyped patterns are configured.
    /// </remarks>
    public IReadOnlyList<string> Patterns { get; }

    /// <summary>
    /// Gets the explicitly typed patterns, grouped by dialect.
    /// </summary>
    /// <remarks>
    /// Patterns in this dictionary bypass automatic classification. The engine
    /// routes each group to the matcher for its declared <see cref="PatternKind"/>.
    /// This property never returns <see langword="null"/>; an empty dictionary
    /// means no explicitly typed patterns are configured.
    /// </remarks>
    public IReadOnlyDictionary<PatternKind, IReadOnlyList<string>> TypedPatterns { get; }

    /// <summary>
    /// Gets the interpretation mode that governs how untyped <see cref="Patterns"/> are classified.
    /// </summary>
    /// <remarks>
    /// When <see cref="PatternInterpretationMode.Specific"/> is used, all untyped patterns
    /// must have been provided through <see cref="TypedPatterns"/> instead; otherwise the
    /// engine throws a <see cref="PatternException"/> during compilation.
    /// </remarks>
    public PatternInterpretationMode InterpretationMode { get; }

    /// <summary>
    /// Initializes a new <see cref="PatternInput"/> with the specified pattern configuration.
    /// </summary>
    /// <param name="Patterns">An optional sequence of untyped pattern strings. When <see langword="null"/>, an empty list is used.</param>
    /// <param name="TypedPatterns">An optional dictionary of explicitly typed patterns. When <see langword="null"/>, an empty dictionary is used.</param>
    /// <param name="InterpretationMode">The classification mode for untyped patterns. Defaults to <see cref="PatternInterpretationMode.Hybrid"/>.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="Patterns"/> or <paramref name="TypedPatterns"/> contains an
    /// empty or whitespace-only pattern.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="TypedPatterns"/> uses an undefined <see cref="PatternKind"/>,
    /// or when <paramref name="InterpretationMode"/> is not a defined
    /// <see cref="PatternInterpretationMode"/> value.
    /// </exception>
    public PatternInput(
        IEnumerable<string>? Patterns = null,
        IReadOnlyDictionary<PatternKind, IEnumerable<string>>? TypedPatterns = null,
        PatternInterpretationMode InterpretationMode = PatternInterpretationMode.Hybrid
    ) {
        if(!Enum.IsDefined(InterpretationMode)) {
            throw new ArgumentOutOfRangeException(
                nameof(InterpretationMode),
                InterpretationMode,
                "Unknown pattern interpretation mode.");
        }

        this.Patterns = Patterns is null
            ? _emptyPatterns
            : MaterializePatterns(Patterns);

        this.TypedPatterns = TypedPatterns is null || TypedPatterns.Count == 0
            ? _emptyTypedPatterns
            : BuildTypedPatterns(TypedPatterns);

        this.InterpretationMode = InterpretationMode;
    }

    /// <summary>
    /// Materializes untyped patterns and rejects empty or whitespace-only entries.
    /// </summary>
    /// <param name="Patterns">The source patterns.</param>
    /// <returns>A read-only list containing the patterns.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="Patterns"/> contains an empty or whitespace-only pattern.
    /// </exception>
    private static List<string> MaterializePatterns(IEnumerable<string> Patterns) {
        var result = new List<string>();

        foreach(var pattern in Patterns) {
            if(pattern is null) {
                continue;
            }

            if(string.IsNullOrWhiteSpace(pattern)) {
                throw new ArgumentException(
                    "Patterns cannot contain empty or whitespace-only entries.",
                    nameof(Patterns));
            }

            result.Add(pattern);
        }

        return result;
    }

    /// <summary>
    /// Materializes <paramref name="TypedPatterns"/> into an immutable dictionary of
    /// read-only lists, filtering out null entries and rejecting empty or
    /// whitespace-only patterns.
    /// </summary>
    /// <param name="TypedPatterns">The source dictionary of typed patterns.</param>
    /// <returns>A dictionary containing processed, read-only lists of patterns.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when a typed-pattern list contains an empty or whitespace-only pattern.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="TypedPatterns"/> contains an undefined PatternKind.
    /// </exception>
    private static Dictionary<PatternKind, IReadOnlyList<string>> BuildTypedPatterns(IReadOnlyDictionary<PatternKind, IEnumerable<string>> TypedPatterns) {
        var result = new Dictionary<PatternKind, IReadOnlyList<string>>(TypedPatterns.Count);

        foreach(var (kind, values) in TypedPatterns) {
            if(!Enum.IsDefined(kind)) {
                throw new ArgumentOutOfRangeException(
                    nameof(TypedPatterns),
                    kind,
                    "Typed patterns cannot use an undefined PatternKind.");
            }

            if(values is null) {
                continue;
            }

            var list = new List<string>();

            foreach(var pattern in values) {
                if(pattern is null) {
                    continue;
                }

                if(string.IsNullOrWhiteSpace(pattern)) {
                    throw new ArgumentException(
                        "Typed patterns cannot contain empty or whitespace-only entries.",
                        nameof(TypedPatterns));
                }

                list.Add(pattern);
            }

            if(list.Count > 0) {
                result[kind] = list.AsReadOnly();
            }
        }

        return result;
    }
}
