//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Invariants;

/// <summary>
/// Validates the structural integrity of compiled character class tokens.
/// </summary>
/// <remarks>
/// <para>
/// Because <see cref="CharacterClassParser"/> never throws,
/// structural problems are recorded as
/// <see cref="CharacterClassParseError"/> sentinel elements inside
/// <see cref="CharacterClass.Elements"/>. This invariant finds those
/// sentinels and converts them into
/// <see cref="PatternInvariantResult"/> failures.
/// </para>
/// <para>
/// Conditions detected:
/// <list type="bullet">
///   <item>
///     Any <see cref="CharacterClassParseError"/> sentinel present in the
///     elements list (unterminated bracket, invalid POSIX syntax, incomplete
///     escape sequence, etc.).
///   </item>
///   <item>
///     A character class whose <see cref="CharacterClass.Elements"/> list is
///     empty after parsing — which should not occur with the current parser
///     but is guarded defensively.
///   </item>
/// </list>
/// </para>
/// </remarks>
internal sealed class CharacterClassStructureInvariant : IPatternInvariant {
    /// <inheritdoc/>
    public PatternInvariantPhase Phase => PatternInvariantPhase.Structural;

    /// <inheritdoc/>
    public PatternKind? AppliesTo => PatternKind.Glob;

    /// <inheritdoc/>
    public PatternInvariantResult Validate(PatternCompilationContext context) {
        foreach(var segment in context.Tokens!) {
#pragma warning disable S3267
            foreach(var token in segment.OfType<CharacterClassToken>()) {
                // Guard against an unexpectedly empty element list.
                if(token.Value.Elements.Count == 0) {
                    return PatternInvariantResult.Fail("Empty character class is not allowed.");
                }

                // Surface any parse error recorded by CharacterClassParser.
                var parseError = token.Value
                                      .Elements
                                      .OfType<CharacterClassParseError>()
                                      .FirstOrDefault();

                if(parseError is not null) {
                    return PatternInvariantResult.Fail(parseError.Message);
                }
            }
#pragma warning restore S3267
        }

        return PatternInvariantResult.Success;
    }
}
