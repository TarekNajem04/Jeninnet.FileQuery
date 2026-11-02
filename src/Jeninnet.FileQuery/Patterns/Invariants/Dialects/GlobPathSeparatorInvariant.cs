//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Invariants.Dialects;

/// <summary>
/// Rejects Windows-style path separators in Glob patterns.
/// </summary>
/// <remarks>
/// <para>
/// Glob patterns use <c>/</c> as the path separator and reserve <c>\</c> for escaping
/// glob metacharacters. Without this guard, a Windows-style pattern such as
/// <c>src\*.cs</c> compiles to a literal backslash and silently never matches a
/// normalized path.
/// </para>
/// <para>
/// A backslash followed by a glob metacharacter (<c>*</c>, <c>?</c>, <c>!</c>,
/// <c>#</c>, <c>[</c>, <c>]</c>, <c>\</c>, <c>{</c>, or <c>}</c>) remains a valid
/// escape and is left untouched.
/// </para>
/// </remarks>
internal sealed class GlobPathSeparatorInvariant : IPatternInvariant {
    /// <inheritdoc/>
    public PatternInvariantPhase Phase => PatternInvariantPhase.Lexical;

    /// <inheritdoc/>
    public PatternKind? AppliesTo => PatternKind.Glob;

    /// <inheritdoc/>
    public PatternInvariantResult Validate(PatternCompilationContext context) {
        var text = context.Pattern.Text.AsSpan();
        var i = 0;

        while(i < text.Length) {
            if(text[i] != '\\') {
                i++;
                continue;
            }

            if(i + 1 < text.Length && IsEscapable(text[i + 1])) {
                i += 2;
                continue;
            }

            return PatternInvariantResult.Fail(
                "Glob patterns use '/' as the path separator. Replace Windows-style " +
                "'\\' with '/' (a backslash is only valid before a glob metacharacter).",
                i,
                PatternTextPosition.SegmentAt(text, i));
        }

        return PatternInvariantResult.Success;
    }

    /// <summary>
    /// Determines whether <paramref name="value"/> may follow a backslash escape.
    /// </summary>
    /// <param name="value">The character following the backslash.</param>
    private static bool IsEscapable(char value) =>
        value is '*' or '?' or '!' or '#' or '[' or ']' or '\\' or '{' or '}';
}
