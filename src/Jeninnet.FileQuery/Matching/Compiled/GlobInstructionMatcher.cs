//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Matching.Compiled;

/// <summary>
/// Implements glob-style matching for file-system paths.
/// </summary>
/// <remarks>
/// <para>
/// This matcher implements classic glob semantics:
/// </para>
/// <list type="bullet">
///   <item><description>Patterns are evaluated in order.</description></item>
///   <item><description>The <em>first</em> matching pattern includes the path.</description></item>
///   <item><description><c>**</c> may match zero or more complete path segments.</description></item>
///   <item><description>Directory-only patterns (<c>foo/</c>) never match files.</description></item>
/// </list>
/// <para>
/// Unlike GitIgnore matching, this matcher does <strong>not</strong> support
/// anchoring, negation, or unanchored recursive scanning.
/// </para>
/// </remarks>
internal sealed class GlobInstructionMatcher : SegmentMatchEngine {
    /// <summary>
    /// Initializes a new instance of the <see cref="GlobInstructionMatcher"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This constructor is <c>internal</c> by design and exists solely to support
    /// controlled creation of matcher instances within the assembly.
    /// </para>
    /// <para>
    /// <strong>Architectural rule:</strong>
    /// Matchers must only be instantiated through internal factories that
    /// preserve pattern and matcher invariants.
    /// </para>
    /// </remarks>
    internal GlobInstructionMatcher() { }

    /// <inheritdoc/>
    public override bool Supports(PatternKind patternKind) => patternKind is PatternKind.Glob;

    /// <inheritdoc/>
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

        var isDirectory = context.PathKind is PathKind.Directory;
        var comparison = context.CaseSensitivity.GetStringComparison();
        var pathView = new PathView(context.Path, isDirectory);

        for(var patternIndex = 0; patternIndex < patterns.Count; patternIndex++) {
            if(patterns[patternIndex].DirectoryOnly && !isDirectory) {
                continue;
            }

            var enumerator = pathView.EnumerateSegments();
            if(MatchExact(patterns[patternIndex].Segments, patternIndex: 0, comparison, ref enumerator)) {
                return MatchResult.Success();
            }
        }

        return MatchResult.Fail();
    }
}
