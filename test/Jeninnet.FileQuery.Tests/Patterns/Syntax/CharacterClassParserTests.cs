namespace Jeninnet.FileQuery.Tests.Patterns.Syntax;

/// <summary>
/// Tests for the redesigned character class system.
/// Covers the parser, the AST discriminated union, the invariants, and end-to-end matching.
/// </summary>
[TestClass]
public sealed class CharacterClassParserTests {
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

            Assert.IsNotNull(result);
        }
    }

    [TestMethod]
    public void Parse_LiteralSet_ProducesCharLiterals() {
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

    [TestMethod]
    public void Parse_Range_ProducesCharRange() {
        var input = "[a-z]".AsSpan();
        var i = 0;

        var result = CharacterClassParser.Parse(input, ref i);

        Assert.HasCount(1, result.Elements);
        var range = Assert.IsInstanceOfType<CharRange>(result.Elements[0]);
        Assert.AreEqual('a', range.Start);
        Assert.AreEqual('z', range.End);
    }

    [TestMethod]
    public void Parse_NegatedClass_SetsIsNegated() {
        var input = "[!abc]".AsSpan();
        var i = 0;

        var result = CharacterClassParser.Parse(input, ref i);

        Assert.IsTrue(result.IsNegated);
        Assert.HasCount(3, result.Elements);
        Assert.IsTrue(result.Elements.All(e => e is CharLiteral));
    }

    [TestMethod]
    public void Parse_CaretNegation_SetsIsNegated() {
        var input = "[^abc]".AsSpan();
        var i = 0;

        var result = CharacterClassParser.Parse(input, ref i);

        Assert.IsTrue(result.IsNegated,
            "'^' must be treated as a negation prefix, identical to '!'.");
    }

    [TestMethod]
    public void Parse_DashAsFirstElement_IsLiteral() {
        // "-" as the first element is a literal, not a range delimiter.
        var input = "[-abc]".AsSpan();
        var i = 0;

        var result = CharacterClassParser.Parse(input, ref i);

        var first = Assert.IsInstanceOfType<CharLiteral>(result.Elements[0]);
        Assert.AreEqual('-', first.Value,
            "'-' at the start of a class must be treated as a literal.");
    }

    [TestMethod]
    public void Parse_ClosingBracketAsFirstElement_IsLiteral() {
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

    [TestMethod]
    public void Parse_DashBeforeClosingBracket_IsLiteral() {
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

    [TestMethod]
    public void Parse_MixedElements_LiteralsAndRange() {
        var input = "[a-z0-9_]".AsSpan();
        var i = 0;

        var result = CharacterClassParser.Parse(input, ref i);

        Assert.HasCount(3, result.Elements);
        Assert.IsInstanceOfType<CharRange>(result.Elements[0]); // a-z
        Assert.IsInstanceOfType<CharRange>(result.Elements[1]); // 0-9
        Assert.IsInstanceOfType<CharLiteral>(result.Elements[2]); // _
    }

    [TestMethod]
    public void Parse_PosixDigit_ProducesPosixClass() {
        var input = "[[:digit:]]".AsSpan();
        var i = 0;

        var result = CharacterClassParser.Parse(input, ref i);

        Assert.HasCount(1, result.Elements);
        var posix = Assert.IsInstanceOfType<PosixClass>(result.Elements[0]);
        Assert.AreEqual("digit", posix.Name);
        Assert.AreEqual(11, i);
    }

    [TestMethod]
    public void Parse_PosixAlpha_ProducesPosixClass() {
        var input = "[[:alpha:]]".AsSpan();
        var i = 0;

        var result = CharacterClassParser.Parse(input, ref i);

        var posix = Assert.IsInstanceOfType<PosixClass>(result.Elements[0]);
        Assert.AreEqual("alpha", posix.Name);
    }

    [TestMethod]
    public void Parse_PosixMixedWithLiteral_ProducesCorrectElements() {
        // [[:digit:]_] ? PosixClass("digit") + CharLiteral('_')
        var input = "[[:digit:]_]".AsSpan();
        var i = 0;

        var result = CharacterClassParser.Parse(input, ref i);

        Assert.HasCount(2, result.Elements);
        Assert.IsInstanceOfType<PosixClass>(result.Elements[0]);
        var lit = Assert.IsInstanceOfType<CharLiteral>(result.Elements[1]);
        Assert.AreEqual('_', lit.Value);
    }

    [TestMethod]
    public void Parse_Unterminated_ProducesErrorSentinel_DoesNotThrow() {
        // "[abc" — no closing bracket
        var input = "[abc".AsSpan();
        var i = 0;

        var result = CharacterClassParser.Parse(input, ref i);

        Assert.Contains(
            e => e is CharacterClassParseError, result.Elements,
            "An unterminated class must produce a CharacterClassParseError sentinel.");

        Assert.AreEqual(input.Length, i,
            "Index must be at end-of-input after an unterminated class.");
    }

    [TestMethod]
    public void Parse_EmptyBrackets_TreatsClosingBracketAsLiteral() {
        // "[]" — the ']' at position 0 is a literal, then no closing ']' ? unterminated
        var input = "[]".AsSpan();
        var i = 0;

        var result = CharacterClassParser.Parse(input, ref i);

        // ']' at the start is a CharLiteral, and then there's no closing bracket.
        Assert.Contains(
            e => e is CharacterClassParseError, result.Elements,
            "'[]' produces an unterminated sentinel because ']' is consumed as a literal.");
    }

    [TestMethod]
    public void Parse_UnterminatedPosix_ProducesErrorSentinel() {
        var input = "[[:digit]".AsSpan(); // missing ':' before the second ']'
        var i = 0;

        var result = CharacterClassParser.Parse(input, ref i);

        Assert.Contains(
            e => e is CharacterClassParseError, result.Elements,
            "An unterminated POSIX class must produce a CharacterClassParseError.");
    }

    [TestMethod]
    public void Parse_TrailingEscape_ProducesErrorSentinel() {
        var input = @"[a\".AsSpan(); // backslash at end of input
        var i = 0;

        var result = CharacterClassParser.Parse(input, ref i);

        Assert.Contains(
            e => e is CharacterClassParseError, result.Elements,
            "An incomplete escape at end of input must produce a CharacterClassParseError.");
    }

    [TestMethod]
    public void Parse_InMiddleOfSegment_AdvancesIndexCorrectly() {
        // The class is embedded in a larger segment; the index must advance
        // exactly past the closing ']'.
        var input = "foo[a-z]bar".AsSpan();
        var i = 3; // pointing at '['

        var result = CharacterClassParser.Parse(input, ref i);

        Assert.DoesNotContain(e => e is CharacterClassParseError, result.Elements);
        Assert.AreEqual(8, i, "Index must point at 'b' (first char after ']').");
    }

    [TestMethod]
    public void Invariant_UnterminatedClass_ReportsFailure() {
        var (HasException, _) = TryCompile("[abc");

        Assert.IsTrue(HasException, "Unterminated class must fail invariant.");
    }

    [TestMethod]
    public void Invariant_ValidClass_Passes() {
        var (HasException, ExceptionMessage) = TryCompile("[c-z]");

        Assert.IsFalse(HasException, ExceptionMessage);
    }

    [TestMethod]
    public void Invariant_InvertedRange_ReportsFailure() {
        var (HasException, _) = TryCompile("[z-a]");

        Assert.IsTrue(HasException,
            "An inverted range 'z-a' must fail the range invariant.");
    }

    [TestMethod]
    public void Invariant_ValidRange_Passes() {
        // "a-z" is a valid range, so it should pass the invariant.
        var (HasException, ExceptionMessage) = TryCompile("[a-z]");

        Assert.IsFalse(HasException, ExceptionMessage);
    }

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

        var results = engine.Execute(new(env.Root, options)).ToList();

        Assert.HasCount(3, results);
        Assert.Contains(p => p.EndsWith("a.txt", StringComparison.Ordinal), results);
        Assert.Contains(p => p.EndsWith("b.txt", StringComparison.Ordinal), results);
        Assert.Contains(p => p.EndsWith("c.txt", StringComparison.Ordinal), results);
        Assert.DoesNotContain(p => p.EndsWith("d.txt", StringComparison.Ordinal), results);
    }

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

        var results = engine.Execute(new(env.Root, options)).ToList();

        Assert.HasCount(3, results);
        Assert.DoesNotContain(p => p.EndsWith("fileX.txt", StringComparison.Ordinal), results);
    }

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

        var results = engine.Execute(new(env.Root, options)).ToList();

        Assert.HasCount(2, results);
        Assert.Contains(p => p.EndsWith("d.txt", StringComparison.Ordinal), results);
        Assert.Contains(p => p.EndsWith("e.txt", StringComparison.Ordinal), results);
    }

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

        var results = engine.Execute(new(env.Root, options)).ToList();

        Assert.HasCount(2, results);
        Assert.Contains(p => p.EndsWith("file1.txt", StringComparison.Ordinal), results);
        Assert.Contains(p => p.EndsWith("file2.txt", StringComparison.Ordinal), results);
        Assert.DoesNotContain(p => p.EndsWith("fileA.txt", StringComparison.Ordinal), results);
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

