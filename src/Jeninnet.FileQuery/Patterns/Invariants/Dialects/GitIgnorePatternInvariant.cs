//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Invariants.Dialects;

/// <summary>
/// GitIgnore-specific structural invariants.
/// </summary>
/// <remarks>
/// Guards:
/// <list type="bullet">
///   <item>
///     A directory-only pattern must contain at least one segment (e.g., <c>"/"</c> alone is invalid).
///   </item>
///   <item>
///     A root-anchored pattern with no body (the bare <c>"/"</c> pattern)
///     is invalid. Root anchoring without a segment to match is meaningless.
///   </item>
/// </list>
/// </remarks>
internal sealed class GitIgnorePatternInvariant : IPatternInvariant {
    /// <inheritdoc/>
    public PatternInvariantPhase Phase => PatternInvariantPhase.Semantic;

    /// <inheritdoc/>
    public PatternKind? AppliesTo => PatternKind.GitIgnore;

    /// <inheritdoc/>
    public PatternInvariantResult Validate(PatternCompilationContext context) {
        if(context.Tokens is null) {
            return PatternInvariantResult.Fail("Tokens not initialized.");
        }

        // A directory-only pattern (ending with '/') with no segments is invalid.
        if(context.State.IsDirectoryOnly && context.Tokens.Count == 0) {
            return PatternInvariantResult.Fail(
                "Directory-only GitIgnore patterns must contain at least one segment.");
        }

        // A root-anchored pattern with no body (bare "/") is invalid.
        // The zero-length sentinel segment (0, 0) inserted by SplitSegments for
        // this case produces an empty token list — which is meaningless to match.
        if(context.State.IsRootAnchored &&
            context.Tokens.Count == 1 &&
            context.Tokens[0].Count == 0) {
            return PatternInvariantResult.Fail(
                "A root-anchored GitIgnore pattern must contain at least one segment " +
                "after the leading '/'. A bare '/' is not a valid pattern.");
        }

        return PatternInvariantResult.Success;
    }
}
