namespace Jeninnet.FileQuery.Tests.Unit.Patterns.Parsing;

/// <summary>
/// Tests for the redesigned character class system.
/// Covers the parser, the AST discriminated union, the invariants, and end-to-end matching.
/// </summary>
[TestClass]
public sealed class CharacterClassParserTests {
    /// <summary>
    /// Verifies that parsing succeeds for a variety of valid character class patterns.
    /// </summary>
    [TestMethod]
    public void Should_Succeed_When_ValidCharacterClassParsed() {
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

            Assert.IsNotNull(result);
        }
    }

    /// <summary>
    /// Verifies that a literal set produces individual CharLiteral elements.
    /// </summary>
    [TestMethod]
    public void Should_ProduceCharLiterals_When_LiteralSetParsed() {
        var input = "[abc]".AsSpan();
        var i = 0;

        var result = CharacterClassParser.Parse(input, ref i);

        Assert.IsFalse(result.IsNegated);
        Assert.HasCount(3, result.Elements);
        Assert.IsInstanceOfType<CharLiteral>(result.Elements[0]);
        Assert.AreEqual('a', ((CharLiteral)result.Elements[0]).Value);
        Assert.AreEqual('b', ((CharLiteral)result.Elements[1]).Value);
        Assert.AreEqual('c', ((CharLiteral)result.Elements[2]).Value);
        Assert.AreEqual(5, i, "Index must be positioned after the closing ']'.");
    }

    /// <summary>
    /// Verifies that a range produces a single CharRange element.
    /// </summary>
    [TestMethod]
    public void Should_ProduceCharRange_When_RangeParsed() {
        var input = "[a-z]".AsSpan();
        var i = 0;

        var result = CharacterClassParser.Parse(input, ref i);

        Assert.HasCount(1, result.Elements);
        var range = Assert.IsInstanceOfType<CharRange>(result.Elements[0]);
        Assert.AreEqual('a', range.Start);
        Assert.AreEqual('z', range.End);
    }

    /// <summary>
    /// Verifies that the IsNegated flag is set when a class is prefixed with '!'.
    /// </summary>
    [TestMethod]
    public void Should_SetIsNegated_When_NegatedClassParsed() {
        var input = "[!abc]".AsSpan();
        var i = 0;

        var result = CharacterClassParser.Parse(input, ref i);

        Assert.IsTrue(result.IsNegated);
        Assert.HasCount(3, result.Elements);
        Assert.IsTrue(result.Elements.All(static e => e is CharLiteral));
    }

    /// <summary>
    /// Verifies that '^' is treated as a negation prefix identical to '!'.
    /// </summary>
    [TestMethod]
    public void Should_SetIsNegated_When_CaretNegationParsed() {
        var input = "[^abc]".AsSpan();
        var i = 0;

        var result = CharacterClassParser.Parse(input, ref i);

        Assert.IsTrue(result.IsNegated,
            "'^' must be treated as a negation prefix, identical to '!'.");
    }

    /// <summary>
    /// Verifies that a dash as the first element is treated as a literal, not a range delimiter.
    /// </summary>
    [TestMethod]
    public void Should_TreatAsLiteral_When_DashIsFirstElement() {
        // "-" as the first element is a literal, not a range delimiter.
        var input = "[-abc]".AsSpan();
        var i = 0;

        var result = CharacterClassParser.Parse(input, ref i);

        var first = Assert.IsInstanceOfType<CharLiteral>(result.Elements[0]);
        Assert.AreEqual('-', first.Value,
            "'-' at the start of a class must be treated as a literal.");
    }

    /// <summary>
    /// Verifies that a closing bracket as the first element is treated as a literal, not the class terminator.
    /// </summary>
    [TestMethod]
    public void Should_TreatAsLiteral_When_ClosingBracketIsFirstElement() {
        // ']' as the first element is a literal, not the class terminator.
        // "[]]" = a class containing ']', then the outer class closes.
        var input = "[]abc]".AsSpan();
        var i = 0;

        var result = CharacterClassParser.Parse(input, ref i);

        var first = Assert.IsInstanceOfType<CharLiteral>(result.Elements[0]);
        Assert.AreEqual(']', first.Value,
            "']' at the start of a class must be treated as a literal.");

        Assert.HasCount(4, result.Elements, "']', 'a', 'b', 'c'.");
    }

    /// <summary>
    /// Verifies that a dash immediately before the closing bracket is treated as a literal.
    /// </summary>
    [TestMethod]
    public void Should_TreatAsLiteral_When_DashBeforeClosingBracket() {
        // "[a-]" ? 'a' literal, '-' literal (not a range delimiter before ']').
        var input = "[a-]".AsSpan();
        var i = 0;

        var result = CharacterClassParser.Parse(input, ref i);

        Assert.HasCount(2, result.Elements);
        Assert.IsInstanceOfType<CharLiteral>(result.Elements[0]);
        var dash = Assert.IsInstanceOfType<CharLiteral>(result.Elements[1]);
        Assert.AreEqual('-', dash.Value,
            "'-' immediately before ']' must be a literal, not a range delimiter.");
    }

    /// <summary>
    /// Verifies that a class containing both literals and ranges produces the correct mixed elements.
    /// </summary>
    [TestMethod]
    public void Should_ParseMixedElements_When_LiteralsAndRangePresent() {
        var input = "[a-z0-9_]".AsSpan();
        var i = 0;

        var result = CharacterClassParser.Parse(input, ref i);

        Assert.HasCount(3, result.Elements);
        Assert.IsInstanceOfType<CharRange>(result.Elements[0]); // a-z
        Assert.IsInstanceOfType<CharRange>(result.Elements[1]); // 0-9
        Assert.IsInstanceOfType<CharLiteral>(result.Elements[2]); // _
    }

    /// <summary>
    /// Verifies that the POSIX digit class is parsed correctly.
    /// </summary>
    [TestMethod]
    public void Should_ProducePosixClass_When_PosixDigitParsed() {
        var input = "[[:digit:]]".AsSpan();
        var i = 0;

        var result = CharacterClassParser.Parse(input, ref i);

        Assert.HasCount(1, result.Elements);
        var posix = Assert.IsInstanceOfType<PosixClass>(result.Elements[0]);
        Assert.AreEqual("digit", posix.Name);
        Assert.AreEqual(11, i);
    }

    /// <summary>
    /// Verifies that the POSIX alpha class is parsed correctly.
    /// </summary>
    [TestMethod]
    public void Should_ProducePosixClass_When_PosixAlphaParsed() {
        var input = "[[:alpha:]]".AsSpan();
        var i = 0;

        var result = CharacterClassParser.Parse(input, ref i);

        var posix = Assert.IsInstanceOfType<PosixClass>(result.Elements[0]);
        Assert.AreEqual("alpha", posix.Name);
    }

    /// <summary>
    /// Verifies that a POSIX class mixed with a literal produces the correct elements.
    /// </summary>
    [TestMethod]
    public void Should_ProduceCorrectElements_When_PosixMixedWithLiteral() {
        // [[:digit:]_] ? PosixClass("digit") + CharLiteral('_')
        var input = "[[:digit:]_]".AsSpan();
        var i = 0;

        var result = CharacterClassParser.Parse(input, ref i);

        Assert.HasCount(2, result.Elements);
        Assert.IsInstanceOfType<PosixClass>(result.Elements[0]);
        var lit = Assert.IsInstanceOfType<CharLiteral>(result.Elements[1]);
        Assert.AreEqual('_', lit.Value);
    }

    /// <summary>
    /// Verifies that an unterminated class produces a CharacterClassParseError sentinel.
    /// </summary>
    [TestMethod]
    public void Should_ProduceErrorSentinel_When_Unterminated() {
        // "[abc" — no closing bracket
        var input = "[abc".AsSpan();
        var i = 0;

        var result = CharacterClassParser.Parse(input, ref i);

        Assert.Contains(
            static e => e is CharacterClassParseError, result.Elements,
            "An unterminated class must produce a CharacterClassParseError sentinel.");

        Assert.AreEqual(input.Length, i,
            "Index must be at end-of-input after an unterminated class.");
    }

    /// <summary>
    /// Verifies that empty brackets treat the closing bracket as a literal and produce an unterminated sentinel.
    /// </summary>
    [TestMethod]
    public void Should_TreatClosingBracketAsLiteral_When_EmptyBrackets() {
        // "[]" — the ']' at position 0 is a literal, then no closing ']' ? unterminated
        var input = "[]".AsSpan();
        var i = 0;

        var result = CharacterClassParser.Parse(input, ref i);

        // ']' at the start is a CharLiteral, and then there's no closing bracket.
        Assert.Contains(
            static e => e is CharacterClassParseError, result.Elements,
            "'[]' produces an unterminated sentinel because ']' is consumed as a literal.");
    }

    /// <summary>
    /// Verifies that an unterminated POSIX class produces a CharacterClassParseError sentinel.
    /// </summary>
    [TestMethod]
    public void Should_ProduceErrorSentinel_When_UnterminatedPosix() {
        var input = "[[:digit]".AsSpan(); // missing ':' before the second ']'
        var i = 0;

        var result = CharacterClassParser.Parse(input, ref i);

        Assert.Contains(
            static e => e is CharacterClassParseError, result.Elements,
            "An unterminated POSIX class must produce a CharacterClassParseError.");
    }

    /// <summary>
    /// Verifies that a trailing escape character at end of input produces a CharacterClassParseError sentinel.
    /// </summary>
    [TestMethod]
    public void Should_ProduceErrorSentinel_When_TrailingEscape() {
        var input = @"[a\".AsSpan(); // backslash at end of input
        var i = 0;

        var result = CharacterClassParser.Parse(input, ref i);

        Assert.Contains(
            static e => e is CharacterClassParseError, result.Elements,
            "An incomplete escape at end of input must produce a CharacterClassParseError.");
    }

    /// <summary>
    /// Verifies that the index advances correctly when the class is embedded in the middle of a segment.
    /// </summary>
    [TestMethod]
    public void Should_AdvanceIndexCorrectly_When_InMiddleOfSegment() {
        // The class is embedded in a larger segment; the index must advance
        // exactly past the closing ']'.
        var input = "foo[a-z]bar".AsSpan();
        var i = 3; // pointing at '['

        var result = CharacterClassParser.Parse(input, ref i);

        Assert.DoesNotContain(static e => e is CharacterClassParseError, result.Elements);
        Assert.AreEqual(8, i, "Index must point at 'b' (first char after ']').");
    }

    /// <summary>
    /// Verifies that compiling an unterminated class reports a failure.
    /// </summary>
    [TestMethod]
    public void Should_ReportFailure_When_UnterminatedClass() {
        var (HasException, _) = TryCompile("[abc");

        Assert.IsTrue(HasException, "Unterminated class must fail invariant.");
    }

    /// <summary>
    /// Verifies that a valid class compiles without throwing an exception.
    /// </summary>
    [TestMethod]
    public void Should_Pass_When_ValidClass() {
        var (HasException, ExceptionMessage) = TryCompile("[c-z]");

        Assert.IsFalse(HasException, ExceptionMessage);
    }

    /// <summary>
    /// Verifies that an inverted range such as 'z-a' fails the range invariant.
    /// </summary>
    [TestMethod]
    public void Should_ReportFailure_When_InvertedRange() {
        var (HasException, _) = TryCompile("[z-a]");

        Assert.IsTrue(HasException,
            "An inverted range 'z-a' must fail the range invariant.");
    }

    /// <summary>
    /// Verifies that a valid range passes the invariant check.
    /// </summary>
    [TestMethod]
    public void Should_Pass_When_ValidRange() {
        // "a-z" is a valid range, so it should pass the invariant.
        var (HasException, ExceptionMessage) = TryCompile("[a-z]");

        Assert.IsFalse(HasException, ExceptionMessage);
    }

    /// <summary>
    /// Verifies end-to-end matching with a literal set character class.
    /// </summary>
    [TestMethod]
    public void Should_MatchCorrectly_When_LiteralSetEndToEnd() {
        var engine = FileQueryRuntime.Create();
        using var env = new TestEnvironment();
        env.CreateFiles("a.txt", "b.txt", "c.txt", "d.txt");

        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(Patterns: ["**", "![abc].txt"])
            )
        );

        var results = engine.Execute(new(env.Root, options)).ToList();

        Assert.HasCount(3, results);
        results.Should().Contain(static p => p.EndsWith("a.txt", StringComparison.Ordinal));
        results.Should().Contain(static p => p.EndsWith("b.txt", StringComparison.Ordinal));
        results.Should().Contain(static p => p.EndsWith("c.txt", StringComparison.Ordinal));
        results.Should().NotContain(static p => p.EndsWith("d.txt", StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifies end-to-end matching with a character range class.
    /// </summary>
    [TestMethod]
    public void Should_MatchCorrectly_When_CharacterRangeEndToEnd() {
        var engine = FileQueryRuntime.Create();
        using var env = new TestEnvironment();
        env.CreateFiles("file0.txt", "file5.txt", "file9.txt", "fileX.txt");

        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(Patterns: ["**", "!file[0-9].txt"])
            )
        );

        var results = engine.Execute(new(env.Root, options)).ToList();

        Assert.HasCount(3, results);
        results.Should().NotContain(static p => p.EndsWith("fileX.txt", StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifies end-to-end matching with a negated character class.
    /// </summary>
    [TestMethod]
    public void Should_MatchCorrectly_When_NegatedClassEndToEnd() {
        var engine = FileQueryRuntime.Create();
        using var env = new TestEnvironment();
        env.CreateFiles("a.txt", "b.txt", "c.txt", "d.txt", "e.txt");

        // ![!abc] includes only characters NOT in {a,b,c} ? d, e
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(Patterns: ["**", "![!abc].txt"])
            )
        );

        var results = engine.Execute(new(env.Root, options)).ToList();

        Assert.HasCount(2, results);
        results.Should().Contain(static p => p.EndsWith("d.txt", StringComparison.Ordinal));
        results.Should().Contain(static p => p.EndsWith("e.txt", StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifies end-to-end matching with a POSIX digit class.
    /// </summary>
    [TestMethod]
    public void Should_MatchCorrectly_When_PosixDigitClassEndToEnd() {
        var engine = FileQueryRuntime.Create();
        using var env = new TestEnvironment();
        env.CreateFiles("file1.txt", "file2.txt", "fileA.txt");

        // [[:digit:]] matches any single digit
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(Patterns: ["**", "!file[[:digit:]].txt"])
            )
        );

        var results = engine.Execute(new(env.Root, options)).ToList();

        Assert.HasCount(2, results);
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
}
