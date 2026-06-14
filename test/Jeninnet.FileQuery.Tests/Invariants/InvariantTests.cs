namespace Jeninnet.FileQuery.Tests.Invariants;

/// <summary>
/// Regression tests for the four invariant gaps identified during the v1.0 freeze audit.
/// </summary>
[TestClass]
public sealed class InvariantTests
{

    // ======================================================================
    // 1 — RecursiveWildcardIsolationInvariant was text-based, missing **a
    //
    // The old invariant scanned context.Pattern.Text for "***". A pattern
    // like "**a" has only two stars so the text check passed, yet the token
    // stream [RecursiveWildcardToken, LiteralToken("a")] is structurally invalid.
    // RecursiveWildcardInSegmentInvariant (new) catches this for all dialects.
    // ======================================================================

    [TestMethod]
    public void GitIgnore_StarStarLiteral_SegmentMixed_IsRejected() =>
        // "**a" produces [RecursiveWildcardToken, LiteralToken("a")] in one segment.
        Assert.ThrowsExactly<PatternException>(
            () => CompiledPatternFactory.Compile(PatternKind.GitIgnore, "**a"),
            "A mixed segment '**a' must be rejected for GitIgnore patterns.");

    [TestMethod]
    public void Glob_DoubleStarMixedSegment_IsRejected() => Assert.ThrowsExactly<PatternException>(
            () => CompiledPatternFactory.Compile(PatternKind.Glob, "**a"),
            "In Glob patterns, '**' must appear as a standalone segment.");

    [TestMethod]
    public void GitIgnore_LiteralStarStar_SegmentMixed_IsRejected() => Assert.ThrowsExactly<PatternException>(
            () => CompiledPatternFactory.Compile(PatternKind.GitIgnore, "a**"),
            "A mixed segment 'a**' must be rejected for GitIgnore patterns.");

    [TestMethod]
    public void GitIgnore_LiteralStarStarLiteral_IsRejected() => Assert.ThrowsExactly<PatternException>(
            () => CompiledPatternFactory.Compile(PatternKind.GitIgnore, "a**b"),
            "A mixed segment 'a**b' must be rejected.");

    [TestMethod]
    public void GitIgnore_StandaloneDoubleStar_IsAccepted()
    {
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

    [TestMethod]
    public void RegexSyntaxInvariant_InvalidRegex_RejectsCorrectly() =>
        // "[invalid" is not valid regex — unclosed bracket.
        Assert.ThrowsExactly<PatternException>(
            () => CompiledPatternFactory.Compile(PatternKind.Regex, "r:[invalid"),
            "An invalid regex pattern must be rejected.");

    [TestMethod]
    public void RegexSyntaxInvariant_ValidRegex_IsAccepted()
    {
        // "r:^src/.*\.cs$" is a valid .NET regex expression.
        var compiled = CompiledPatternFactory.Compile(PatternKind.Regex, @"r:^src/.*\.cs$");
        Assert.IsNotEmpty(compiled, "A valid regex pattern must compile successfully.");
    }

    [TestMethod]
    public void RegexSyntaxInvariant_ErrorMessageShowsExpressionNotPrefix()
    {
        // The error message must reference the expression ("[invalid"),
        // not the full raw string ("r:[invalid").
        var ex = Assert.ThrowsExactly<PatternException>(() =>
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

    [TestMethod]
    public void GitIgnore_BareSlash_IsRejected() => Assert.ThrowsExactly<PatternException>(
            () => CompiledPatternFactory.Compile(PatternKind.GitIgnore, "/"),
            "A bare '/' must be rejected as a GitIgnore pattern.");

    [TestMethod]
    public void GitIgnore_MultipleSlashes_IsRejected() => Assert.ThrowsExactly<PatternException>(
            () => CompiledPatternFactory.Compile(PatternKind.GitIgnore, "///"),
            "A pattern of only '/' separators must be rejected.");

    [TestMethod]
    public void GitIgnore_RootAnchoredWithBody_IsAccepted()
    {
        // "/src" is a valid root-anchored GitIgnore pattern.
        var compiled = CompiledPatternFactory.Compile(PatternKind.GitIgnore, "/src");
        Assert.IsNotEmpty(compiled, "'/src' must compile successfully.");
    }

    // ======================================================================
    // 4 — **a is unguarded in GitIgnore mode (covered above in Gap 1)
    // These tests verify the unified GitIgnoreImplicitRecursiveInvariant
    // correctly inserts ** for unanchored non-negated patterns too.
    // ======================================================================

    [TestMethod]
    public void GitIgnoreImplicitRecursive_UnanchoredNonNegated_ReceivesImplicitDoubleStar()
    {
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

    [TestMethod]
    public void GitIgnoreImplicitRecursive_RootAnchored_DoesNotReceiveImplicitDoubleStar()
    {
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

    [TestMethod]
    public void GitIgnoreImplicitRecursive_AlreadyStartsWithDoubleStar_NotDuplicated()
    {
        // "**/*.cs" already begins with ** — must not have another ** inserted.
        var compiled = CompiledPatternFactory
            .Compile(PatternKind.GitIgnore, "**/*.cs")
            .Patterns
            .Single();

        var doubleStarCount = compiled.Segments
            .Count(seg => seg.Count == 1 && seg[0] is RecursiveWildcardToken);

        Assert.AreEqual(1, doubleStarCount,
            "'**/*.cs' must contain exactly one '**' segment — not two.");
    }
}
