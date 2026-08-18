//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Matching.Compiled;

/// <summary>
/// Implements pattern matching using full .NET regular expressions.
/// </summary>
/// <remarks>
/// <para>
/// Evaluates <see cref="PatternKind.Regex"/> patterns compiled into
/// <see cref="Regex"/> instances cached by
/// <c>(pattern text, case sensitivity)</c> key.
/// </para>
/// <para>
/// <strong>Performance contract — zero hot-path allocations:</strong>
/// Pattern iteration uses an index-based <c>for</c> loop. A <c>foreach</c>
/// over the <see cref="ICompiledPatternSet"/> interface would box a
/// heap-allocated <see cref="IEnumerator{T}"/> (~40 B per call), which was
/// confirmed by the benchmark showing 40 B allocated.
/// </para>
/// <para>
/// <strong>Cache key design:</strong> <see cref="Regex"/> instances are cached
/// by <see cref="RegexCacheKey"/>, a composite of pattern text and case
/// sensitivity. Two independently compiled patterns with identical text and
/// the same case sensitivity share one <see cref="Regex"/> instance.
/// Keying by <see cref="ICompiledPattern"/> object identity (the previous
/// design) caused two bugs: structural equality on the record produced
/// separate <see cref="Regex"/> objects for each distinct instance, and a
/// change in case sensitivity between calls could return the wrong cached
/// <see cref="Regex"/>.
/// </para>
/// </remarks>
internal sealed class RegexInstructionMatcher : PathMatcher {
    /// <summary>
    /// Composite key used to cache compiled <see cref="Regex"/> instances.
    /// </summary>
    /// <param name="Pattern">The regex pattern text.</param>
    /// <param name="CaseSensitivity">The case sensitivity setting.</param>
    private readonly record struct RegexCacheKey(
        string Pattern,
        CaseSensitivity CaseSensitivity
    );

    /// <summary>
    /// Thread-safe cache of compiled <see cref="Regex"/> instances.
    /// </summary>
    private readonly ConcurrentDictionary<RegexCacheKey, Regex> _regexCache = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="RegexInstructionMatcher"/>.
    /// </summary>
    /// <remarks>
    /// <c>internal</c> by design. Matchers must only be instantiated through
    /// <see cref="PathMatcherFactory"/>.
    /// </remarks>
    internal RegexInstructionMatcher() { }

    /// <inheritdoc/>
    public override bool Supports(PatternKind patternKind) => patternKind is PatternKind.Regex;

    /// <inheritdoc/>
    /// <param name="patterns">The set of compiled patterns to match.</param>
    /// <param name="context">The path context containing path information and options.</param>
    protected override MatchResult MatchCore(
        ICompiledPatternSet patterns,
        PathMatchContext context
    ) {
        if(context.Path.IsEmpty) {
            return MatchResult.Fail();
        }

        if(patterns.Count == 0) {
            return MatchResult.Success();
        }

        // INDEX-BASED LOOP — avoids boxing a heap-allocated IEnumerator<ICompiledPattern>
        // that a foreach over the ICompiledPatternSet interface would create (~40 B per call).
        // Benchmarks confirmed 40 B allocated before this fix; target is 0 B.
        for(var i = 0; i < patterns.Count; i++) {
            var pattern = patterns[i];
            var regexText = pattern.RegexText;

            if(regexText is null) {
                // Pattern did not contain a valid RegularExpressionToken — skip.
                return MatchResult.Fail();
            }

            var regex = GetOrCreateRegex(regexText, context.CaseSensitivity);

            // Full-path match: the entire normalized path must satisfy the expression.
            if(regex.IsMatch(context.Path)) {
                return MatchResult.Success();
            }
        }

        return MatchResult.Fail();
    }

    /// <summary>
    /// Retrieves a compiled <see cref="Regex"/> for the given expression text
    /// and case-sensitivity mode, creating and caching it on first access.
    /// </summary>
    /// <param name="patternText">
    /// The raw regular expression string (without the <c>r:</c> prefix).
    /// </param>
    /// <param name="caseSensitivity">
    /// Determines whether <see cref="RegexOptions.IgnoreCase"/> is applied.
    /// </param>
    private Regex GetOrCreateRegex(string patternText, CaseSensitivity caseSensitivity) {
        var key = new RegexCacheKey(patternText, caseSensitivity);

        return _regexCache.GetOrAdd(key, static k => {
            var options = RegexOptions.Compiled | RegexOptions.CultureInvariant;

            if(k.CaseSensitivity is CaseSensitivity.Insensitive) {
                options |= RegexOptions.IgnoreCase;
            }

            // Pass a timeout to limit the execution time.
            // Using a default timeout of 1 second as per common recommendations for regex protection.
            return new Regex(k.Pattern, options, TimeSpan.FromSeconds(1));
        });
    }
}
