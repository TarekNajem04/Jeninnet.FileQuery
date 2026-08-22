//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Unit.Patterns;

/// <summary>
/// Regression tests for the four invariant gaps identified during the v1.0 freeze audit.
/// </summary>
[TestClass]
public sealed class InvariantRegressionTests {

    // ======================================================================
    // 1 — RecursiveWildcardIsolationInvariant was text-based, missing **a
    //
    // The old invariant scanned context.Pattern.Text for "***". A pattern
    // like "**a" has only two stars so the text check passed, yet the token
    // stream [RecursiveWildcardToken, LiteralToken("a")] is structurally invalid.
    // RecursiveWildcardInSegmentInvariant (new) catches this for all dialects.
    // ======================================================================

    /// <summary>
    /// Verifies that Should BeRejected When GitIgnoreStarStarLiteralSegmentMixed.
    /// </summary>
    [TestMethod]
    public void Should_BeRejected_When_GitIgnoreStarStarLiteralSegmentMixed() =>
        // "**a" produces [RecursiveWildcardToken, LiteralToken("a")] in one segment.
        Assert.ThrowsExactly<PatternException>(
            static () => CompiledPatternFactory.Compile(PatternKind.GitIgnore, "**a"),
            "A mixed segment '**a' must be rejected for GitIgnore patterns.");

    /// <summary>
    /// Verifies that Should BeRejected When GlobDoubleStarMixedSegment.
    /// </summary>
    [TestMethod]
    public void Should_BeRejected_When_GlobDoubleStarMixedSegment() => Assert.ThrowsExactly<PatternException>(
            static () => CompiledPatternFactory.Compile(PatternKind.Glob, "**a"),
            "In Glob patterns, '**' must appear as a standalone segment.");

    /// <summary>
    /// Verifies that Should BeRejected When GitIgnoreLiteralStarStarSegmentMixed.
    /// </summary>
    [TestMethod]
    public void Should_BeRejected_When_GitIgnoreLiteralStarStarSegmentMixed() => Assert.ThrowsExactly<PatternException>(
            static () => CompiledPatternFactory.Compile(PatternKind.GitIgnore, "a**"),
            "A mixed segment 'a**' must be rejected for GitIgnore patterns.");

    /// <summary>
    /// Verifies that Should BeRejected When GitIgnoreLiteralStarStarLiteral.
    /// </summary>
    [TestMethod]
    public void Should_BeRejected_When_GitIgnoreLiteralStarStarLiteral() => Assert.ThrowsExactly<PatternException>(
            static () => CompiledPatternFactory.Compile(PatternKind.GitIgnore, "a**b"),
            "A mixed segment 'a**b' must be rejected.");

    /// <summary>
    /// Verifies that Should BeAccepted When GitIgnoreStandaloneDoubleStar.
    /// </summary>
    [TestMethod]
    public void Should_BeAccepted_When_GitIgnoreStandaloneDoubleStar() {
        // A standalone ** in its own segment must still be valid.
        var compiled = CompiledPatternFactory.Compile(PatternKind.GitIgnore, "**/src");
        Assert.IsNotEmpty(compiled, "'**/src' must compile successfully.");
    }

    // ======================================================================
    // 2 — RegexSyntaxInvariant was validating "r:..." including the prefix
    //
    // The .NET Regex engine accepts "r:" as two literal characters, so the
    // validator was checking a different string than what the matcher uses.
    // After the fix, "r:" is stripped before compilation.
    // ======================================================================

    /// <summary>
    /// Verifies that Should RejectCorrectly When InvalidRegexUsed.
    /// </summary>
    [TestMethod]
    public void Should_RejectCorrectly_When_InvalidRegexUsed() =>
        // "[invalid" is not valid regex — unclosed bracket.
        Assert.ThrowsExactly<PatternException>(
            static () => CompiledPatternFactory.Compile(PatternKind.Regex, "r:[invalid"),
            "An invalid regex pattern must be rejected.");

    /// <summary>
    /// Verifies that Should BeAccepted When ValidRegexUsed.
    /// </summary>
    [TestMethod]
    public void Should_BeAccepted_When_ValidRegexUsed() {
        // "r:^src/.*\.cs$" is a valid .NET regex expression.
        var compiled = CompiledPatternFactory.Compile(PatternKind.Regex, @"r:^src/.*\.cs$");
        Assert.IsNotEmpty(compiled, "A valid regex pattern must compile successfully.");
    }

    /// <summary>
    /// Verifies that Should ShowExpressionNotPrefix When ErrorMessageGenerated.
    /// </summary>
    [TestMethod]
    public void Should_ShowExpressionNotPrefix_When_ErrorMessageGenerated() {
        // The error message must reference the expression ("[invalid"),
        // not the full raw string ("r:[invalid").
        var ex = Assert.ThrowsExactly<PatternException>(static () =>
            CompiledPatternFactory.Compile(PatternKind.Regex, "r:[invalid")
        );

        Assert.Contains(
            "[invalid", ex.Message,
            $"Error message must reference the expression. Actual: {ex.Message}"
        );

        Assert.DoesNotStartWith(
            "Invalid regex syntax in 'r:", ex.Message,
            $"Error message must not include the 'r:' prefix. Actual: {ex.Message}"
        );
    }

    // ======================================================================
    // 3 — No invariant for zero-segment result (bare "/")
    //
    // PatternScanner.AnalyzeStructure sets IsRootAnchored=true for "/".
    // SplitSegments produces a single (0,0) sentinel entry.
    // TokenizeSegment on a zero-length span produces an empty token list.
    // GitIgnorePatternInvariant now detects this case.
    // ======================================================================

    /// <summary>
    /// Verifies that Should BeRejected When GitIgnoreBareSlash.
    /// </summary>
    [TestMethod]
    public void Should_BeRejected_When_GitIgnoreBareSlash() => Assert.ThrowsExactly<PatternException>(
            static () => CompiledPatternFactory.Compile(PatternKind.GitIgnore, "/"),
            "A bare '/' must be rejected as a GitIgnore pattern.");

    /// <summary>
    /// Verifies that Should BeRejected When GitIgnoreMultipleSlashes.
    /// </summary>
    [TestMethod]
    public void Should_BeRejected_When_GitIgnoreMultipleSlashes() => Assert.ThrowsExactly<PatternException>(
            static () => CompiledPatternFactory.Compile(PatternKind.GitIgnore, "///"),
            "A pattern of only '/' separators must be rejected.");

    /// <summary>
    /// Verifies that Should BeAccepted When GitIgnoreRootAnchoredWithBody.
    /// </summary>
    [TestMethod]
    public void Should_BeAccepted_When_GitIgnoreRootAnchoredWithBody() {
        // "/src" is a valid root-anchored GitIgnore pattern.
        var compiled = CompiledPatternFactory.Compile(PatternKind.GitIgnore, "/src");
        Assert.IsNotEmpty(compiled, "'/src' must compile successfully.");
    }

    // ======================================================================
    // 4 — **a is unguarded in GitIgnore mode (covered above in Gap 1)
    // These tests verify the unified GitIgnoreImplicitRecursiveInvariant
    // correctly inserts ** for unanchored non-negated patterns too.
    // ======================================================================

    /// <summary>
    /// Verifies that Should ReceiveImplicitDoubleStar When UnanchoredNonNegated.
    /// </summary>
    [TestMethod]
    public void Should_ReceiveImplicitDoubleStar_When_UnanchoredNonNegated() {
        // "*.txt" should compile to [**, *.txt] — the matcher must slide across depths.
        var compiled = CompiledPatternFactory
            .Compile(PatternKind.GitIgnore, "*.txt")
            .Patterns
            .Single();

        // First segment must be a standalone recursive wildcard.
        Assert.IsGreaterThanOrEqualTo(
            2, compiled.Segments.Count,
            "'*.txt' must have at least two segments after implicit ** is prepended.");

        Assert.IsTrue(
            compiled.Segments[0].Count == 1 &&
            compiled.Segments[0][0] is RecursiveWildcardToken,
            "First segment of '*.txt' must be the implicit '**'.");
    }

    /// <summary>
    /// Verifies that Should NotReceiveImplicitDoubleStar When RootAnchored.
    /// </summary>
    [TestMethod]
    public void Should_NotReceiveImplicitDoubleStar_When_RootAnchored() {
        // "/src/*.cs" is root-anchored — no implicit ** must be prepended.
        var compiled = CompiledPatternFactory
            .Compile(PatternKind.GitIgnore, "/src/*.cs")
            .Patterns
            .Single();

        Assert.IsTrue(compiled.AnchoredToRoot,
            "'/src/*.cs' must be marked as root-anchored.");

        Assert.IsFalse(
            compiled.Segments[0].Count == 1 &&
            compiled.Segments[0][0] is RecursiveWildcardToken,
            "Root-anchored pattern must not have an implicit '**' prepended.");
    }

    /// <summary>
    /// Verifies that Should NotDuplicateDoubleStar When AlreadyStartsWithDoubleStar.
    /// </summary>
    [TestMethod]
    public void Should_NotDuplicateDoubleStar_When_AlreadyStartsWithDoubleStar() {
        // "**/*.cs" already begins with ** — must not have another ** inserted.
        var compiled = CompiledPatternFactory
            .Compile(PatternKind.GitIgnore, "**/*.cs")
            .Patterns
            .Single();

        var doubleStarCount = compiled.Segments
            .Count(static seg => seg.Count == 1 && seg[0] is RecursiveWildcardToken);

        Assert.AreEqual(1, doubleStarCount,
            "'**/*.cs' must contain exactly one '**' segment — not two.");
    }
}
