namespace Jeninnet.FileQuery.Patterns.Invariants;

/// <summary>
/// Validates that all <see cref="CharRange"/> elements inside compiled
/// character class tokens represent valid, non-inverted ranges.
/// </summary>
/// <remarks>
/// <para>
/// The parser (<see cref="CharacterClassParser"/>) accepts ranges
/// such as <c>z-a</c> syntactically because it cannot distinguish intent from
/// error without context. This invariant performs the semantic check:
/// <see cref="CharRange.Start"/> must be ≤ <see cref="CharRange.End"/>.
/// </para>
/// <para>
/// Applies only to <see cref="PatternKind.Glob"/> patterns because GitIgnore
/// delegates character class matching to the same underlying segment engine.
/// </para>
/// </remarks>
internal sealed class CharacterClassRangeInvariant : IPatternInvariant
{
    /// <inheritdoc/>
    public PatternInvariantPhase Phase => PatternInvariantPhase.Structural;

    /// <inheritdoc/>
    public PatternKind? AppliesTo => PatternKind.Glob;

    /// <inheritdoc/>
    public PatternInvariantResult Validate(PatternCompilationContext context)
    {
        foreach(var segment in context.Tokens!)
        {
            foreach(var token in segment.OfType<CharacterClassToken>())
            {
                foreach(var range in token.Value.Elements.OfType<CharRange>())
                {
                    if(range.Start > range.End)
                    {
                        return PatternInvariantResult.Fail(
                            $"Invalid character range '{range.Start}-{range.End}': " +
                            $"start (U+{(int)range.Start:X4}) must not exceed end (U+{(int)range.End:X4}).");
                    }
                }
            }
        }

        return PatternInvariantResult.Success;
    }
}
