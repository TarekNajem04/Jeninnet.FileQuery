//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
using Jeninnet.FileQuery.Internal;
using Jeninnet.FileQuery.Patterns;

namespace Jeninnet.FileQuery;

/// <summary>
/// An immutable, compiled pattern set that can match individual paths without traversing a filesystem.
/// </summary>
/// <remarks>
/// <para>
/// Use this type for linters, IDE quick actions, path classifiers, and tests where only a
/// root-relative path is available. Compilation happens once; <see cref="IsMatch(ReadOnlySpan{char})"/>
/// performs no allocation.
/// </para>
/// <para>
/// Paths are root-relative and use forward slashes. A trailing slash marks the path as a
/// directory, which is required for directory-only patterns such as <c>bin/</c>.
/// </para>
/// </remarks>
public sealed class FileQueryPatternSet {
    private readonly ICompiledPatternSet _compiledPatterns;
    private readonly IPathMatcher _matcher;
    private readonly CaseSensitivity _caseSensitivity;

    /// <summary>
    /// Initializes a compiled pattern set.
    /// </summary>
    /// <param name="compiledPatterns">The compiled patterns.</param>
    /// <param name="matcher">The matcher selected for the pattern dialects.</param>
    /// <param name="caseSensitivity">The resolved case-sensitivity mode.</param>
    private FileQueryPatternSet(
        ICompiledPatternSet compiledPatterns,
        IPathMatcher matcher,
        CaseSensitivity caseSensitivity
    ) {
        _compiledPatterns = compiledPatterns;
        _matcher = matcher;
        _caseSensitivity = caseSensitivity;
    }

    /// <summary>
    /// Compiles a pattern input for in-memory matching.
    /// </summary>
    /// <param name="patternInput">The patterns to compile.</param>
    /// <param name="matchingMode">
    /// The dialect used when <paramref name="patternInput"/> selects
    /// <see cref="PatternInterpretationMode.Specific"/>.
    /// </param>
    /// <param name="caseSensitivity">The case-sensitivity behavior. Platform defaults are resolved once.</param>
    /// <returns>The compiled pattern set.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="patternInput"/> is <see langword="null"/>.</exception>
    public static FileQueryPatternSet Compile(
        PatternInput patternInput,
        PatternMatchingMode matchingMode = PatternMatchingMode.GitIgnore,
        CaseSensitivity caseSensitivity = CaseSensitivity.PlatformDefault
    ) {
        ArgumentNullException.ThrowIfNull(patternInput);

        var resolvedCase = caseSensitivity.Resolve();
        var typedPatterns = PatternsMerger.Merge(patternInput);
        var configuration = new MatchingConfiguration(typedPatterns, matchingMode, resolvedCase);
        var compiled = CompiledPatternSetFactory.Create(configuration);

        return new FileQueryPatternSet(
            compiled,
            PathMatcherFactory.Create(patternInput, matchingMode),
            resolvedCase);
    }

    /// <summary>
    /// Compiles untyped patterns using automatic hybrid classification.
    /// </summary>
    /// <param name="patterns">The pattern strings.</param>
    /// <param name="caseSensitivity">The case-sensitivity behavior. Platform defaults are resolved once.</param>
    /// <returns>The compiled pattern set.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="patterns"/> is <see langword="null"/>.</exception>
    public static FileQueryPatternSet Compile(
        IEnumerable<string> patterns,
        CaseSensitivity caseSensitivity = CaseSensitivity.PlatformDefault
    ) {
        ArgumentNullException.ThrowIfNull(patterns);
        return Compile(new PatternInput(Patterns: patterns), PatternMatchingMode.GitIgnore, caseSensitivity);
    }

    /// <summary>
    /// Determines whether a root-relative path is included by the compiled patterns.
    /// </summary>
    /// <param name="relativePath">
    /// The root-relative path. Backslashes are normalized to forward slashes; a trailing slash marks a directory.
    /// </param>
    /// <returns><see langword="true"/> when the path is included; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="relativePath"/> is <see langword="null"/>.</exception>
    public bool IsMatch(string relativePath) {
        ArgumentNullException.ThrowIfNull(relativePath);
        return relativePath.Contains('\\', StringComparison.Ordinal)
            ? IsMatch(relativePath.Replace('\\', '/').AsSpan())
            : IsMatch(relativePath.AsSpan());
    }

    /// <summary>
    /// Determines whether a normalized root-relative path is included by the compiled patterns.
    /// </summary>
    /// <param name="relativePath">
    /// The normalized root-relative path, using forward slashes. A trailing slash marks a directory.
    /// </param>
    /// <returns><see langword="true"/> when the path is included; otherwise <see langword="false"/>.</returns>
    /// <remarks>This overload is allocation-free on the matching path.</remarks>
    public bool IsMatch(ReadOnlySpan<char> relativePath) =>
        _matcher.Match(_compiledPatterns, CreateContext(relativePath)) is MatchOutcome.Include;

    /// <summary>
    /// Explains the outcome for a root-relative path, including the deciding pattern.
    /// </summary>
    /// <param name="relativePath">
    /// The root-relative path. Backslashes are normalized to forward slashes; a trailing slash marks a directory.
    /// </param>
    /// <returns>The explanation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="relativePath"/> is <see langword="null"/>.</exception>
    public FileQueryPatternExplanation Explain(string relativePath) {
        ArgumentNullException.ThrowIfNull(relativePath);

        var normalized = relativePath.Contains('\\', StringComparison.Ordinal)
            ? relativePath.Replace('\\', '/')
            : relativePath;
        var context = CreateContext(normalized.AsSpan());
        var outcome = _matcher.Match(_compiledPatterns, context);
        var pattern = MatchExplanation.FindResponsiblePattern(_compiledPatterns, context, outcome);
        var reason = pattern is null
            ? MatchExplanation.GetDefaultReason(outcome)
            : $"{outcome} by {pattern.PatternKind} pattern.";

        return new FileQueryPatternExplanation(
            normalized,
            ToPublicOutcome(outcome),
            outcome is MatchOutcome.Include,
            reason,
            pattern?.SourceText,
            pattern?.PatternKind,
            pattern?.SourceIndex >= 0 ? pattern.SourceIndex : null);
    }

    /// <summary>
    /// Creates the match context, treating a trailing slash as a directory marker.
    /// </summary>
    /// <param name="relativePath">The normalized path.</param>
    private PathMatchContext CreateContext(ReadOnlySpan<char> relativePath) {
        var isDirectory = relativePath.Length > 0 && relativePath[^1] == '/';
        return new PathMatchContext(
            relativePath,
            isDirectory ? PathKind.Directory : PathKind.File,
            _caseSensitivity);
    }

    private static FileQueryMatchOutcome ToPublicOutcome(MatchOutcome outcome) =>
        outcome switch {
            MatchOutcome.Include => FileQueryMatchOutcome.Include,
            MatchOutcome.Exclude => FileQueryMatchOutcome.Exclude,
            _ => FileQueryMatchOutcome.NoMatch
        };
}
