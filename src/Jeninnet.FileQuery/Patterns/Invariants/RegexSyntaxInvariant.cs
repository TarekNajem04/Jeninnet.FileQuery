namespace Jeninnet.FileQuery.Patterns.Invariants;

/// <summary>
/// Ensures that the regular expression portion of a <see cref="PatternKind.Regex"/>
/// pattern is compilable by the .NET <see cref="Regex"/> engine.
/// </summary>
/// <remarks>
/// <para>
/// <strong>What changed (v1.0 fix):</strong> The previous implementation passed
/// <c>context.Pattern.Text</c> directly to <c>new Regex(...)</c>. For a pattern
/// such as <c>"r:^src/.*\.cs$"</c>, this compiled the string <c>"r:^src/.*\.cs$"</c>
/// (with the prefix), not <c>"^src/.*\.cs$"</c> (the actual expression). The
/// .NET regex engine accepted the prefix as two literal characters, so structurally
/// invalid expressions that began with <c>r:</c> could produce misleading error
/// messages. The prefix is now stripped before compilation.
/// </para>
/// </remarks>
internal sealed class RegexSyntaxInvariant : IPatternInvariant
{
    private const string REGEX_PREFIX = "r:";

    /// <inheritdoc/>
    public PatternInvariantPhase Phase => PatternInvariantPhase.Lexical;

    /// <inheritdoc/>
    public PatternKind? AppliesTo => PatternKind.Regex;

    /// <inheritdoc/>
    public PatternInvariantResult Validate(PatternCompilationContext context)
    {
        var text = context.Pattern.Text;

        // Strip the "r:" prefix before validating so the Regex engine sees only
        // the actual expression the matcher will use at runtime.
        var expression = text.StartsWith(REGEX_PREFIX, StringComparison.Ordinal)
            ? text[REGEX_PREFIX.Length..]
            : text;

        try
        {
            _ = new Regex(
                expression,
                RegexOptions.Compiled | RegexOptions.CultureInvariant,
                TimeSpan.FromSeconds(1) // Pass a timeout to limit the execution time.
            );
            return PatternInvariantResult.Success;
        }
        catch(ArgumentException ex)
        {
            return PatternInvariantResult.Fail(
                $"Invalid regex syntax in '{expression}': {ex.Message}");
        }
    }
}
