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
internal static class PatternsMerger
{
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
        IEnumerable<string> patterns,
        IReadOnlyDictionary<PatternKind, IEnumerable<string>> typedPatterns
    )
    {
        var mergedPatterns = new Dictionary<PatternKind, (List<string> List, HashSet<string> Set)>();

        // Seed the dictionary with explicitly typed patterns first.
        if(typedPatterns is not null)
        {
            foreach(var (kind, values) in typedPatterns)
            {
                if(!mergedPatterns.TryGetValue(kind, out var bucket))
                {
                    bucket = (new List<string>(), new HashSet<string>(StringComparer.Ordinal));
                    mergedPatterns[kind] = bucket;
                }

                foreach(var val in values)
                {
                    if(bucket.Set.Add(val))
                    {
                        bucket.List.Add(val);
                    }
                }
            }
        }

        if(patterns is not null)
        {
            // Classify and append each untyped pattern.
            foreach(var rawPattern in patterns)
            {
                var kind = PatternClassifier.Classify(rawPattern);

                if(!mergedPatterns.TryGetValue(kind, out var bucket))
                {
                    bucket = (new List<string>(), new HashSet<string>(StringComparer.Ordinal));
                    mergedPatterns[kind] = bucket;
                }

                if(bucket.Set.Add(rawPattern))
                {
                    bucket.List.Add(rawPattern);
                }
            }
        }

        return mergedPatterns.ToDictionary(
            static kvp => kvp.Key,
            static kvp => kvp.Value.List.ToImmutableArray()
        );
    }
}
