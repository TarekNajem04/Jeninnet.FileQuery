namespace Jeninnet.FileQuery.Matching;

/// <summary>
/// Internal immutable projection of pattern and matching-related settings.
/// </summary>
/// <remarks>
/// <para>
/// <strong>TypedPatterns key type change (v1.0):</strong>
/// <see cref="TypedPatterns"/> was previously typed as
/// <see cref="ImmutableDictionary{TKey,TValue}"/>.
/// <see cref="TraversalPlanBuilder"/> was calling
/// <c>PatternsMerger.Merge(...).ToImmutableDictionary()</c>, which copied
/// the already-materialized <see cref="Dictionary{TKey,TValue}"/> into a
/// second immutable structure. The copy was unnecessary: this configuration
/// object lives only for the duration of a single query build and is never
/// mutated after construction. Changing to
/// <see cref="IReadOnlyDictionary{TKey,TValue}"/> eliminates that copy.
/// </para>
/// </remarks>
internal sealed record MatchingConfiguration(
    IReadOnlyDictionary<PatternKind, ImmutableArray<string>> TypedPatterns,
    PatternMatchingMode MatchingMode,
    CaseSensitivity CaseSensitivity
);
