//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Internal;

/// <summary>
/// Merges untyped and explicitly typed patterns into a single dictionary
/// keyed by <see cref="PatternKind"/>.
/// </summary>
/// <remarks>
/// <para>
/// This class is the sole component responsible for resolving the union of
/// <see cref="PatternInput.Patterns"/> (auto-classified) and
/// <see cref="PatternInput.TypedPatterns"/> (explicitly typed) into the
/// format consumed by the pattern compilation pipeline.
/// </para>
/// <para>
/// <strong>Ordering contract:</strong> Explicitly typed patterns (from
/// <see cref="PatternInput.TypedPatterns"/>) are always added first for each
/// bucket. Auto-classified patterns from <see cref="PatternInput.Patterns"/>
/// are appended afterward. This ensures that explicitly declared intent takes
/// precedence over auto-classification within the same pattern-kind bucket.
/// </para>
/// <para>
/// <strong>De-duplication:</strong> Patterns already present in a bucket are
/// never added again, preserving the first occurrence.
/// </para>
/// </remarks>
internal static class PatternsMerger {
    /// <summary>
    /// Merges the pattern configuration from a <see cref="PatternInput"/> into a
    /// dictionary keyed by <see cref="PatternKind"/>, classifying any untyped patterns.
    /// </summary>
    /// <param name="patternInput">The public pattern configuration to merge.</param>
    /// <returns>
    /// A dictionary mapping each <see cref="PatternKind"/> to an immutable array
    /// of pattern strings in the order they should be evaluated.
    /// </returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static Dictionary<PatternKind, ImmutableArray<string>> Merge(PatternInput patternInput) =>
        Merge(
            patternInput.Patterns,
            patternInput.TypedPatterns.ToDictionary(
                static kvp => kvp.Key,
                static kvp => (IEnumerable<string>)kvp.Value
            )
        );

    /// <summary>
    /// Validates that a <see cref="PatternInput"/> is consistent with
    /// <see cref="PatternInterpretationMode.Specific"/> before compilation.
    /// </summary>
    /// <param name="patternInput">The pattern configuration.</param>
    /// <param name="matchingMode">
    /// The dialect selected for <see cref="PatternInterpretationMode.Specific"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="patternInput"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="PatternException">
    /// Thrown when untyped patterns are present in Specific mode, or when a typed
    /// pattern uses a dialect that does not match <paramref name="matchingMode"/>.
    /// </exception>
    public static void ValidateSpecificMode(
        PatternInput patternInput,
        PatternMatchingMode matchingMode
    ) {
        ArgumentNullException.ThrowIfNull(patternInput);

        if(patternInput.InterpretationMode is not PatternInterpretationMode.Specific) {
            return;
        }

        if(patternInput.Patterns.Count > 0) {
            throw new PatternException(
                $"Pattern '{patternInput.Patterns[0]}' requires an explicit PatternKind " +
                "when interpretation mode is set to 'Specific'.");
        }

        var expectedKind = matchingMode switch {
            PatternMatchingMode.GitIgnore => PatternKind.GitIgnore,
            PatternMatchingMode.Glob => PatternKind.Glob,
            PatternMatchingMode.Regex => PatternKind.Regex,
            _ => PatternKind.Unknown
        };

        var mismatch = patternInput.TypedPatterns.Keys
            .Where(kind => kind != expectedKind)
            .Select(static kind => (PatternKind?)kind)
            .FirstOrDefault();

        if(mismatch is { } mismatchedKind) {
            throw new PatternException(
                $"Pattern kind '{mismatchedKind}' cannot be used when interpretation mode is " +
                $"'Specific' and the matching mode is '{matchingMode}'.");
        }
    }

    /// <summary>
    /// Merges a raw pattern sequence and a typed-pattern dictionary into a single dictionary keyed by <see cref="PatternKind"/>.
    /// </summary>
    /// <param name="patterns">
    /// Untyped pattern strings. Each string is classified using
    /// <see cref="PatternClassifier"/> to determine its dialect.
    /// </param>
    /// <param name="typedPatterns">
    /// Explicitly typed patterns. These are seeded into their respective buckets
    /// before untyped patterns are classified and appended.
    /// </param>
    /// <returns>
    /// A dictionary mapping each <see cref="PatternKind"/> to an immutable array of merged pattern strings.
    /// </returns>
    public static Dictionary<PatternKind, ImmutableArray<string>> Merge(
        IEnumerable<string>? patterns,
        IReadOnlyDictionary<PatternKind, IEnumerable<string>>? typedPatterns
    ) {
        // Lists preserve declaration order, which is semantically significant for
        // last-match-wins dialects. A HashSet only provides de-duplication here.
        var buckets = new Dictionary<PatternKind, List<string>>();
        var seen = new Dictionary<PatternKind, HashSet<string>>();

        void Add(PatternKind kind, string value) {
            if(!buckets.TryGetValue(kind, out var list)) {
                list = [];
                buckets[kind] = list;
                seen[kind] = new HashSet<string>(StringComparer.Ordinal);
            }

            if(seen[kind].Add(value)) {
                list.Add(value);
            }
        }

        // Seed the dictionary with explicitly typed patterns first.
        if(typedPatterns is not null) {
            foreach(var (kind, values) in typedPatterns) {
                foreach(var val in (values ?? []).Where(v => v is not null)) {
                    Add(kind, val);
                }
            }
        }

        // Classify and append each untyped pattern.
        foreach(var rawPattern in (patterns ?? []).Where(p => p is not null)) {
            Add(PatternClassifier.Classify(rawPattern), rawPattern);
        }

        return buckets.ToDictionary(
            static kvp => kvp.Key,
            static kvp => kvp.Value.ToImmutableArray()
        );
    }
}
