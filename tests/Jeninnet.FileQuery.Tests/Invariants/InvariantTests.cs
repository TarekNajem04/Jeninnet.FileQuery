//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Invariants;

/// <summary>
/// Regression tests for the four invariant gaps identified during the v1.0 freeze audit.
/// </summary>
[TestClass]
public sealed class InvariantTests {

    // ======================================================================
    // 1 — RecursiveWildcardIsolationInvariant was text-based, missing **a
    //
    // The old invariant scanned context.Pattern.Text for "***". A pattern
    // like "**a" has only two stars so the text check passed, yet the token
    // stream [RecursiveWildcardToken, LiteralToken("a")] is structurally invalid.
    // RecursiveWildcardInSegmentInvariant (new) catches this for all dialects.
    // ======================================================================

    /// <summary>Tests GitIgnore_StarStarLiteral_SegmentMixed_IsRejected.</summary>
    [TestMethod]
    public void GitIgnore_StarStarLiteral_SegmentMixed_IsRejected() =>
        // "**a" produces [RecursiveWildcardToken, LiteralToken("a")] in one segment.
        FluentActions.Invoking(static () => CompiledPatternFactory.Compile(PatternKind.GitIgnore, "**a")).Should().ThrowExactly<PatternException>(because: "A mixed segment '**a' must be rejected for GitIgnore patterns.");

    /// <summary>Tests Glob_DoubleStarMixedSegment_IsRejected.</summary>
    [TestMethod]
    public void Glob_DoubleStarMixedSegment_IsRejected() => FluentActions.Invoking(static () => CompiledPatternFactory.Compile(PatternKind.Glob, "**a")).Should().ThrowExactly<PatternException>(because: "In Glob patterns, '**' must appear as a standalone segment.");

    /// <summary>Tests GitIgnore_LiteralStarStar_SegmentMixed_IsRejected.</summary>
    [TestMethod]
    public void GitIgnore_LiteralStarStar_SegmentMixed_IsRejected() => FluentActions.Invoking(static () => CompiledPatternFactory.Compile(PatternKind.GitIgnore, "a**")).Should().ThrowExactly<PatternException>(because: "A mixed segment 'a**' must be rejected for GitIgnore patterns.");

    /// <summary>Tests GitIgnore_LiteralStarStarLiteral_IsRejected.</summary>
    [TestMethod]
    public void GitIgnore_LiteralStarStarLiteral_IsRejected() => FluentActions.Invoking(static () => CompiledPatternFactory.Compile(PatternKind.GitIgnore, "a**b")).Should().ThrowExactly<PatternException>(because: "A mixed segment 'a**b' must be rejected.");

    /// <summary>Tests GitIgnore_StandaloneDoubleStar_IsAccepted.</summary>
    [TestMethod]
    public void GitIgnore_StandaloneDoubleStar_IsAccepted() {
        // A standalone ** in its own segment must still be valid.
        var compiled = CompiledPatternFactory.Compile(PatternKind.GitIgnore, "**/src");
        compiled.Should().NotBeEmpty(because: "'**/src' must compile successfully.");
    }

    // ======================================================================
    // 2 — RegexSyntaxInvariant was validating "r:..." including the prefix
    //
    // The .NET Regex engine accepts "r:" as two literal characters, so the
    // validator was checking a different string than what the matcher uses.
    // After the fix, "r:" is stripped before compilation.
    // ======================================================================

    /// <summary>Tests RegexSyntaxInvariant_InvalidRegex_RejectsCorrectly.</summary>
    [TestMethod]
    public void RegexSyntaxInvariant_InvalidRegex_RejectsCorrectly() =>
        // "[invalid" is not valid regex — unclosed bracket.
        FluentActions.Invoking(static () => CompiledPatternFactory.Compile(PatternKind.Regex, "r:[invalid")).Should().ThrowExactly<PatternException>(because: "An invalid regex pattern must be rejected.");

    /// <summary>Tests RegexSyntaxInvariant_ValidRegex_IsAccepted.</summary>
    [TestMethod]
    public void RegexSyntaxInvariant_ValidRegex_IsAccepted() {
        // "r:^src/.*\.cs$" is a valid .NET regex expression.
        var compiled = CompiledPatternFactory.Compile(PatternKind.Regex, @"r:^src/.*\.cs$");
        compiled.Should().NotBeEmpty(because: "A valid regex pattern must compile successfully.");
    }

    /// <summary>Tests RegexSyntaxInvariant_ErrorMessageShowsExpressionNotPrefix.</summary>
    [TestMethod]
    public void RegexSyntaxInvariant_ErrorMessageShowsExpressionNotPrefix() {
        // The error message must reference the expression ("[invalid"),
        // not the full raw string ("r:[invalid").
        var ex = FluentActions.Invoking(static () =>
            CompiledPatternFactory.Compile(PatternKind.Regex, "r:[invalid")).Should().ThrowExactly<PatternException>().Which;

        ex.Message.Should().Contain("[invalid", because: $"Error message must reference the expression. Actual: {ex.Message}");

        ex.Message.Should().NotStartWith("Invalid regex syntax in 'r:", because: $"Error message must not include the 'r:' prefix. Actual: {ex.Message}");
    }

    // ======================================================================
    // 3 — No invariant for zero-segment result (bare "/")
    //
    // PatternScanner.AnalyzeStructure sets IsRootAnchored=true for "/".
    // SplitSegments produces a single (0,0) sentinel entry.
    // TokenizeSegment on a zero-length span produces an empty token list.
    // GitIgnorePatternInvariant now detects this case.
    // ======================================================================

    /// <summary>Tests GitIgnore_BareSlash_IsRejected.</summary>
    [TestMethod]
    public void GitIgnore_BareSlash_IsRejected() => FluentActions.Invoking(static () => CompiledPatternFactory.Compile(PatternKind.GitIgnore, "/")).Should().ThrowExactly<PatternException>(because: "A bare '/' must be rejected as a GitIgnore pattern.");

    /// <summary>Tests GitIgnore_MultipleSlashes_IsRejected.</summary>
    [TestMethod]
    public void GitIgnore_MultipleSlashes_IsRejected() => FluentActions.Invoking(static () => CompiledPatternFactory.Compile(PatternKind.GitIgnore, "///")).Should().ThrowExactly<PatternException>(because: "A pattern of only '/' separators must be rejected.");

    /// <summary>Tests GitIgnore_RootAnchoredWithBody_IsAccepted.</summary>
    [TestMethod]
    public void GitIgnore_RootAnchoredWithBody_IsAccepted() {
        // "/src" is a valid root-anchored GitIgnore pattern.
        var compiled = CompiledPatternFactory.Compile(PatternKind.GitIgnore, "/src");
        compiled.Should().NotBeEmpty(because: "'/src' must compile successfully.");
    }

    // ======================================================================
    // 4 — **a is unguarded in GitIgnore mode (covered above in Gap 1)
    // These tests verify the unified GitIgnoreImplicitRecursiveInvariant
    // correctly inserts ** for unanchored non-negated patterns too.
    // ======================================================================

    /// <summary>Tests GitIgnoreImplicitRecursive_UnanchoredNonNegated_ReceivesImplicitDoubleStar.</summary>
    [TestMethod]
    public void GitIgnoreImplicitRecursive_UnanchoredNonNegated_ReceivesImplicitDoubleStar() {
        // "*.txt" should compile to [**, *.txt] — the matcher must slide across depths.
        var compiled = CompiledPatternFactory
            .Compile(PatternKind.GitIgnore, "*.txt")
            .Patterns
            .Single();

        // First segment must be a standalone recursive wildcard.
        compiled.Segments.Count.Should().BeGreaterThanOrEqualTo(2, because: "'*.txt' must have at least two segments after implicit ** is prepended.");

        (compiled.Segments[0].Count == 1 &&
            compiled.Segments[0][0] is RecursiveWildcardToken).Should().BeTrue(because: "First segment of '*.txt' must be the implicit '**'.");
    }

    /// <summary>Tests GitIgnoreImplicitRecursive_RootAnchored_DoesNotReceiveImplicitDoubleStar.</summary>
    [TestMethod]
    public void GitIgnoreImplicitRecursive_RootAnchored_DoesNotReceiveImplicitDoubleStar() {
        // "/src/*.cs" is root-anchored — no implicit ** must be prepended.
        var compiled = CompiledPatternFactory
            .Compile(PatternKind.GitIgnore, "/src/*.cs")
            .Patterns
            .Single();

        compiled.AnchoredToRoot.Should().BeTrue(because: "'/src/*.cs' must be marked as root-anchored.");

        (compiled.Segments[0].Count == 1 &&
            compiled.Segments[0][0] is RecursiveWildcardToken).Should().BeFalse(because: "Root-anchored pattern must not have an implicit '**' prepended.");
    }

    /// <summary>Tests GitIgnoreImplicitRecursive_AlreadyStartsWithDoubleStar_NotDuplicated.</summary>
    [TestMethod]
    public void GitIgnoreImplicitRecursive_AlreadyStartsWithDoubleStar_NotDuplicated() {
        // "**/*.cs" already begins with ** — must not have another ** inserted.
        var compiled = CompiledPatternFactory
            .Compile(PatternKind.GitIgnore, "**/*.cs")
            .Patterns
            .Single();

        var doubleStarCount = compiled.Segments
            .Count(static seg => seg.Count == 1 && seg[0] is RecursiveWildcardToken);

        doubleStarCount.Should().Be(1, because: "'**/*.cs' must contain exactly one '**' segment — not two.");
    }
}
