//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Patterns.Syntax;

/// <summary>
/// Tests for the redesigned character class system.
/// Covers the parser, the AST discriminated union, the invariants, and end-to-end matching.
/// </summary>
[TestClass]
public sealed class CharacterClassParserTests {
    /// <summary>
    /// Verifies that CharacterClassParser can parse various valid character class patterns successfully.
    /// </summary>
    [TestMethod]
    public void CharacterClassParser_ShouldSuccess() {
        string[] _patterns =
        [
            "[a-z]",
            "[a-z0-9_]",
            "[-abc]",
            "[abc-]",
            "[]abc]",
            "[!a-z]",
            "[[:digit:]]",
            "[![:alpha:]_.-]"
        ];

        foreach(var pattern in _patterns) {
            var index = 0;
            var result = CharacterClassParser.Parse(pattern, ref index);

            result.Should().NotBeNull();
        }
    }

    /// <summary>
    /// Verifies that Parse_LiteralSet produces correct CharLiterals.
    /// </summary>
    [TestMethod]
    public void Parse_LiteralSet_ProducesCharLiterals() {
        var input = "[abc]".AsSpan();
        var i = 0;

        var result = CharacterClassParser.Parse(input, ref i);

        result.IsNegated.Should().BeFalse();
        result.Elements.Should().HaveCount(3);
        result.Elements[0].Should().BeAssignableTo<CharLiteral>();
        ((CharLiteral)result.Elements[0]).Value.Should().Be('a');
        ((CharLiteral)result.Elements[1]).Value.Should().Be('b');
        ((CharLiteral)result.Elements[2]).Value.Should().Be('c');
        i.Should().Be(5, because: "Index must be positioned after the closing ']'.");
    }

    /// <summary>
    /// Verifies that Parse_Range produces correct CharRange.
    /// </summary>
    [TestMethod]
    public void Parse_Range_ProducesCharRange() {
        var input = "[a-z]".AsSpan();
        var i = 0;

        var result = CharacterClassParser.Parse(input, ref i);

        result.Elements.Should().HaveCount(1);
        var range = (CharRange)(result.Elements[0]);
        range.Start.Should().Be('a');
        range.End.Should().Be('z');
    }

    /// <summary>
    /// Verifies that Parse_NegatedClass sets the IsNegated flag correctly.
    /// </summary>
    [TestMethod]
    public void Parse_NegatedClass_SetsIsNegated() {
        var input = "[!abc]".AsSpan();
        var i = 0;

        var result = CharacterClassParser.Parse(input, ref i);

        result.IsNegated.Should().BeTrue();
        result.Elements.Should().HaveCount(3);
        (result.Elements.All(static e => e is CharLiteral)).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that Parse_CaretNegation sets the IsNegated flag correctly.
    /// </summary>
    [TestMethod]
    public void Parse_CaretNegation_SetsIsNegated() {
        var input = "[^abc]".AsSpan();
        var i = 0;

        var result = CharacterClassParser.Parse(input, ref i);

        result.IsNegated.Should().BeTrue(because: "'^' must be treated as a negation prefix, identical to '!'.");
    }

    /// <summary>
    /// Verifies that Parse_DashAsFirstElement treats dash as a literal.
    /// </summary>
    [TestMethod]
    public void Parse_DashAsFirstElement_IsLiteral() {
        // "-" as the first element is a literal, not a range delimiter.
        var input = "[-abc]".AsSpan();
        var i = 0;

        var result = CharacterClassParser.Parse(input, ref i);

        var first = (CharLiteral)(result.Elements[0]);
        first.Value.Should().Be('-', because: "'-' at the start of a class must be treated as a literal.");
    }

    /// <summary>
    /// Verifies that Parse_ClosingBracketAsFirstElement treats the bracket as a literal.
    /// </summary>
    [TestMethod]
    public void Parse_ClosingBracketAsFirstElement_IsLiteral() {
        // ']' as the first element is a literal, not the class terminator.
        // "[]]" = a class containing ']', then the outer class closes.
        var input = "[]abc]".AsSpan();
        var i = 0;

        var result = CharacterClassParser.Parse(input, ref i);

        var first = (CharLiteral)(result.Elements[0]);
        first.Value.Should().Be(']', because: "']' at the start of a class must be treated as a literal.");

        result.Elements.Should().HaveCount(4, because: "']', 'a', 'b', 'c'.");
    }

    /// <summary>
    /// Verifies that Parse_DashBeforeClosingBracket treats the dash as a literal.
    /// </summary>
    [TestMethod]
    public void Parse_DashBeforeClosingBracket_IsLiteral() {
        // "[a-]" ? 'a' literal, '-' literal (not a range delimiter before ']').
        var input = "[a-]".AsSpan();
        var i = 0;

        var result = CharacterClassParser.Parse(input, ref i);

        result.Elements.Should().HaveCount(2);
        result.Elements[0].Should().BeAssignableTo<CharLiteral>();
        var dash = (CharLiteral)(result.Elements[1]);
        dash.Value.Should().Be('-', because: "'-' immediately before ']' must be a literal, not a range delimiter.");
    }

    /// <summary>
    /// Verifies that Parse_MixedElements correctly parses a mix of literals and ranges.
    /// </summary>
    [TestMethod]
    public void Parse_MixedElements_LiteralsAndRange() {
        var input = "[a-z0-9_]".AsSpan();
        var i = 0;

        var result = CharacterClassParser.Parse(input, ref i);

        result.Elements.Should().HaveCount(3);
        result.Elements[0].Should().BeAssignableTo<CharRange>(); // a-z
        result.Elements[1].Should().BeAssignableTo<CharRange>(); // 0-9
        result.Elements[2].Should().BeAssignableTo<CharLiteral>(); // _
    }

    /// <summary>
    /// Verifies that Parse_PosixDigit produces a correct PosixClass.
    /// </summary>
    [TestMethod]
    public void Parse_PosixDigit_ProducesPosixClass() {
        var input = "[[:digit:]]".AsSpan();
        var i = 0;

        var result = CharacterClassParser.Parse(input, ref i);

        result.Elements.Should().HaveCount(1);
        var posix = (PosixClass)(result.Elements[0]);
        posix.Name.Should().Be("digit");
        i.Should().Be(11);
    }

    /// <summary>
    /// Verifies that Parse_PosixAlpha produces a correct PosixClass.
    /// </summary>
    [TestMethod]
    public void Parse_PosixAlpha_ProducesPosixClass() {
        var input = "[[:alpha:]]".AsSpan();
        var i = 0;

        var result = CharacterClassParser.Parse(input, ref i);

        var posix = (PosixClass)(result.Elements[0]);
        posix.Name.Should().Be("alpha");
    }

    /// <summary>
    /// Verifies that Parse_PosixMixedWithLiteral produces correct elements.
    /// </summary>
    [TestMethod]
    public void Parse_PosixMixedWithLiteral_ProducesCorrectElements() {
        // [[:digit:]_] ? PosixClass("digit") + CharLiteral('_')
        var input = "[[:digit:]_]".AsSpan();
        var i = 0;

        var result = CharacterClassParser.Parse(input, ref i);

        result.Elements.Should().HaveCount(2);
        result.Elements[0].Should().BeAssignableTo<PosixClass>();
        var lit = (CharLiteral)(result.Elements[1]);
        lit.Value.Should().Be('_');
    }

    /// <summary>
    /// Verifies that Parse_Unterminated produces a CharacterClassParseError.
    /// </summary>
    [TestMethod]
    public void Parse_Unterminated_ProducesErrorSentinel_DoesNotThrow() {
        // "[abc" — no closing bracket
        var input = "[abc".AsSpan();
        var i = 0;

        var result = CharacterClassParser.Parse(input, ref i);

        result.Elements.Should().Contain(static e => e is CharacterClassParseError, because: "An unterminated class must produce a CharacterClassParseError sentinel.");

        i.Should().Be(input.Length, because: "Index must be at end-of-input after an unterminated class.");
    }

    /// <summary>
    /// Verifies that Parse_EmptyBrackets produces a CharacterClassParseError.
    /// </summary>
    [TestMethod]
    public void Parse_EmptyBrackets_TreatsClosingBracketAsLiteral() {
        // "[]" — the ']' at position 0 is a literal, then no closing ']' ? unterminated
        var input = "[]".AsSpan();
        var i = 0;

        var result = CharacterClassParser.Parse(input, ref i);

        // ']' at the start is a CharLiteral, and then there's no closing bracket.
        result.Elements.Should().Contain(static e => e is CharacterClassParseError, because: "'[]' produces an unterminated sentinel because ']' is consumed as a literal.");
    }

    /// <summary>
    /// Verifies that Parse_UnterminatedPosix produces a CharacterClassParseError.
    /// </summary>
    [TestMethod]
    public void Parse_UnterminatedPosix_ProducesErrorSentinel() {
        var input = "[[:digit]".AsSpan(); // missing ':' before the second ']'
        var i = 0;

        var result = CharacterClassParser.Parse(input, ref i);

        result.Elements.Should().Contain(static e => e is CharacterClassParseError, because: "An unterminated POSIX class must produce a CharacterClassParseError.");
    }

    /// <summary>
    /// Verifies that Parse_TrailingEscape produces a CharacterClassParseError.
    /// </summary>
    [TestMethod]
    public void Parse_TrailingEscape_ProducesErrorSentinel() {
        var input = @"[a\".AsSpan(); // backslash at end of input
        var i = 0;

        var result = CharacterClassParser.Parse(input, ref i);

        result.Elements.Should().Contain(static e => e is CharacterClassParseError, because: "An incomplete escape at end of input must produce a CharacterClassParseError.");
    }

    /// <summary>
    /// Verifies that Parse_InMiddleOfSegment advances index correctly.
    /// </summary>
    [TestMethod]
    public void Parse_InMiddleOfSegment_AdvancesIndexCorrectly() {
        // The class is embedded in a larger segment; the index must advance
        // exactly past the closing ']'.
        var input = "foo[a-z]bar".AsSpan();
        var i = 3; // pointing at '['

        var result = CharacterClassParser.Parse(input, ref i);

        result.Elements.Should().NotContain(static e => e is CharacterClassParseError);
        i.Should().Be(8, because: "Index must point at 'b' (first char after ']').");
    }

    /// <summary>
    /// Verifies that Invariant_UnterminatedClass reports a failure.
    /// </summary>
    [TestMethod]
    public void Invariant_UnterminatedClass_ReportsFailure() {
        var (HasException, _) = TryCompile("[abc");

        HasException.Should().BeTrue(because: "Unterminated class must fail invariant.");
    }

    /// <summary>
    /// Verifies that Invariant_ValidClass passes.
    /// </summary>
    [TestMethod]
    public void Invariant_ValidClass_Passes() {
        var (HasException, ExceptionMessage) = TryCompile("[c-z]");

        HasException.Should().BeFalse(because: ExceptionMessage);
    }

    /// <summary>
    /// Verifies that Invariant_InvertedRange reports a failure.
    /// </summary>
    [TestMethod]
    public void Invariant_InvertedRange_ReportsFailure() {
        var (HasException, _) = TryCompile("[z-a]");

        HasException.Should().BeTrue(because: "An inverted range 'z-a' must fail the range invariant.");
    }

    /// <summary>
    /// Verifies that Invariant_ValidRange passes.
    /// </summary>
    [TestMethod]
    public void Invariant_ValidRange_Passes() {
        // "a-z" is a valid range, so it should pass the invariant.
        var (HasException, ExceptionMessage) = TryCompile("[a-z]");

        HasException.Should().BeFalse(because: ExceptionMessage);
    }

    /// <summary>
    /// Verifies that EndToEnd_LiteralSet matches correctly.
    /// </summary>
    [TestMethod]
    public void EndToEnd_LiteralSet_MatchesCorrectly() {
        var engine = FileQueryRuntime.Create();
        using var env = new TestEnvironment();
        env.CreateFiles("a.txt", "b.txt", "c.txt", "d.txt");

        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(Patterns: ["**", "![abc].txt"])
            )
        );

        var results = engine.Execute(new(env.Root, options), TestContext.CancellationToken).ToList();

        results.Should().HaveCount(3);
        results.Should().Contain(static p => p.EndsWith("a.txt", StringComparison.Ordinal));
        results.Should().Contain(static p => p.EndsWith("b.txt", StringComparison.Ordinal));
        results.Should().Contain(static p => p.EndsWith("c.txt", StringComparison.Ordinal));
        results.Should().NotContain(static p => p.EndsWith("d.txt", StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifies that EndToEnd_CharacterRange matches correctly.
    /// </summary>
    [TestMethod]
    public void EndToEnd_CharacterRange_MatchesCorrectly() {
        var engine = FileQueryRuntime.Create();
        using var env = new TestEnvironment();
        env.CreateFiles("file0.txt", "file5.txt", "file9.txt", "fileX.txt");

        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(Patterns: ["**", "!file[0-9].txt"])
            )
        );

        var results = engine.Execute(new(env.Root, options), TestContext.CancellationToken).ToList();

        results.Should().HaveCount(3);
        results.Should().NotContain(static p => p.EndsWith("fileX.txt", StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifies that EndToEnd_NegatedClass matches correctly.
    /// </summary>
    [TestMethod]
    public void EndToEnd_NegatedClass_MatchesCorrectly() {
        var engine = FileQueryRuntime.Create();
        using var env = new TestEnvironment();
        env.CreateFiles("a.txt", "b.txt", "c.txt", "d.txt", "e.txt");

        // ![!abc] includes only characters NOT in {a,b,c} ? d, e
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(Patterns: ["**", "![!abc].txt"])
            )
        );

        var results = engine.Execute(new(env.Root, options), TestContext.CancellationToken).ToList();

        results.Should().HaveCount(2);
        results.Should().Contain(static p => p.EndsWith("d.txt", StringComparison.Ordinal));
        results.Should().Contain(static p => p.EndsWith("e.txt", StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifies that EndToEnd_PosixDigitClass matches correctly.
    /// </summary>
    [TestMethod]
    public void EndToEnd_PosixDigitClass_MatchesCorrectly() {
        var engine = FileQueryRuntime.Create();
        using var env = new TestEnvironment();
        env.CreateFiles("file1.txt", "file2.txt", "fileA.txt");

        // [[:digit:]] matches any single digit
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(Patterns: ["**", "!file[[:digit:]].txt"])
            )
        );

        var results = engine.Execute(new(env.Root, options), TestContext.CancellationToken).ToList();

        results.Should().HaveCount(2);
        results.Should().Contain(static p => p.EndsWith("file1.txt", StringComparison.Ordinal));
        results.Should().Contain(static p => p.EndsWith("file2.txt", StringComparison.Ordinal));
        results.Should().NotContain(static p => p.EndsWith("fileA.txt", StringComparison.Ordinal));
    }

    private static (bool HasException, string ExceptionMessage) TryCompile(string pattern) {
        try {
            CompiledPatternFactory.Compile(PatternKind.Glob, pattern);
            return (false, string.Empty);
        }
        catch(PatternException ex) {
            return (true, ex.Message);
        }
    }

    /// <summary>
    /// Gets or sets the test context, which provides information about the current test run.
    /// Used here primarily for accessing the <see cref="CancellationToken"/>.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;
}
