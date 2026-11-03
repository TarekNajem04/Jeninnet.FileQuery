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
                var failure = ValidateCharacterClass(context, token.Value);

                if(failure is not null) {
                    return failure.Value;
                }
            }
#pragma warning restore S3267
        }

        return PatternInvariantResult.Success;
    }

    /// <summary>
    /// Validates one parsed character class.
    /// </summary>
    /// <param name="context">The compilation context.</param>
    /// <param name="characterClass">The parsed character class.</param>
    /// <returns>The failure result, or <see langword="null"/> when the class is valid.</returns>
    private static PatternInvariantResult? ValidateCharacterClass(
        PatternCompilationContext context,
        CharacterClass characterClass
    ) {
        if(characterClass.Elements.Count == 0) {
            return CreateFailure(context, "Empty character class is not allowed.");
        }

        var parseError = characterClass.Elements
            .OfType<CharacterClassParseError>()
            .FirstOrDefault();

        return parseError is null
            ? null
            : CreateFailure(context, parseError.Message);
    }

    /// <summary>
    /// Creates a failure result located at the first opening bracket.
    /// </summary>
    /// <param name="context">The compilation context.</param>
    /// <param name="message">The failure message.</param>
    /// <returns>The failure result.</returns>
    private static PatternInvariantResult CreateFailure(PatternCompilationContext context, string message) {
        var span = context.Pattern.Text.AsSpan();
        int? position = PatternTextPosition.Find(span, "[");
        int? segment = position is { } value ? PatternTextPosition.SegmentAt(span, value) : null;

        return PatternInvariantResult.Fail(message, position, segment);
    }
}
