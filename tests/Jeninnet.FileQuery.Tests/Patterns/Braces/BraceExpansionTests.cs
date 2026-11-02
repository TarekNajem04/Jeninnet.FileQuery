//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Patterns.Braces;

/// <summary>
/// Tests for brace alternation expansion.
/// </summary>
[TestClass]
public sealed class BraceExpansionTests {
    private static readonly string[] _expectedFiles = ["a.cs", "b.vb"];

    /// <summary>
    /// A brace group expands to individual alternatives.
    /// </summary>
    [TestMethod]
    public void Compile_ExpandsSimpleAlternation() {
        var patterns = FileQueryPatternSet.Compile(["*.{cs,vb}"]);

        patterns.IsMatch("Program.cs").Should().BeTrue();
        patterns.IsMatch("Program.vb").Should().BeTrue();
        patterns.IsMatch("Program.txt").Should().BeFalse();
    }

    /// <summary>
    /// Nested groups form a cartesian product.
    /// </summary>
    [TestMethod]
    public void Compile_ExpandsNestedAlternation() {
        var patterns = CompileGlobSet("{src,test}/**/*.{cs,vb}");

        (patterns.IsMatch("src/Program.cs")).Should().BeTrue();
        (patterns.IsMatch("test/Program.vb")).Should().BeTrue();
        (patterns.IsMatch("test/Program.txt")).Should().BeFalse();
    }

    /// <summary>
    /// Regex patterns are not subject to glob bracket/brace pairing rules.
    /// </summary>
    [TestMethod]
    public void Compile_RegexWithLiteralBrace_IsValid() {
        var patterns = FileQueryPatternSet.Compile(["r:^a}$"]);

        patterns.IsMatch("a}").Should().BeTrue();
        patterns.IsMatch("a").Should().BeFalse();
    }

    /// <summary>
    /// Typed regex patterns trim surrounding whitespace before adding the prefix.
    /// </summary>
    [TestMethod]
    public void Compile_TypedRegex_TrimsSurroundingWhitespace() {
        var patterns = FileQueryPatternSet.Compile(
            new PatternInput(
                TypedPatterns: new Dictionary<PatternKind, IEnumerable<string>> {
                    [PatternKind.Regex] = ["  ^a$  "]
                },
                InterpretationMode: PatternInterpretationMode.Specific),
            PatternMatchingMode.Regex);

        patterns.IsMatch("a").Should().BeTrue();
        (patterns.IsMatch(" a")).Should().BeFalse();
    }

    /// <summary>
    /// Escaped braces remain literal.
    /// </summary>
    [TestMethod]
    public void Compile_TreatsEscapedBracesAsLiteral() {
        var patterns = FileQueryPatternSet.Compile([@"\{a,b\}.txt"]);

        patterns.IsMatch("{a,b}.txt").Should().BeTrue();
        patterns.IsMatch("a.txt").Should().BeFalse();
    }

    /// <summary>
    /// Brace alternation is classified as a Glob feature.
    /// </summary>
    [TestMethod]
    public void Classification_RecognizesBraceAlternation() => (PatternClassifier.Classify("*.{cs,vb}")).Should().Be(PatternKind.Glob);

    /// <summary>
    /// An unterminated group reports a validation error with a position.
    /// </summary>
    [TestMethod]
    public void Compile_ReportsUnterminatedBrace() {
        var exception = FluentActions.Invoking(() => CompileGlob("src/{a,b")).Should().ThrowExactly<PatternException>().Which;

        exception.Position.Should().Be(4);
        exception.Segment.Should().Be(1);
        exception.Message.Should().Contain("matching closing brace");
    }
    /// <summary>
    /// A stray closing brace is rejected.
    /// </summary>
    [TestMethod]
    public void Compile_ReportsStrayClosingBrace() {
        var exception = FluentActions.Invoking(() => CompileGlob("src/a}b")).Should().ThrowExactly<PatternException>().Which;

        exception.Position.Should().Be(5);
        exception.Message.Should().Contain("no matching opening brace");
    }

    /// <summary>
    /// Empty alternatives are rejected instead of producing an empty pattern.
    /// </summary>
    [TestMethod]
    public void Compile_RejectsEmptyAlternative() {
        var exception = FluentActions.Invoking(() => CompileGlob("src/{a,}")).Should().ThrowExactly<PatternException>().Which;

        exception.Message.Should().Contain("cannot be empty");
    }

    /// <summary>
    /// Expansion is bounded to avoid runaway pattern input.
    /// </summary>
    [TestMethod]
    public void Compile_RejectsExcessiveExpansion() {
        var pattern = string.Concat(Enumerable.Repeat("{a,b}", 10));

        var exception = FluentActions.Invoking(() => CompileGlob(pattern)).Should().ThrowExactly<PatternException>().Which;

        exception.Message.Should().Contain("exceeds");
    }

    /// <summary>
    /// Regex quantifiers are not treated as brace alternation.
    /// </summary>
    [TestMethod]
    public void Compile_DoesNotExpandRegexBraces() {
        var patterns = FileQueryPatternSet.Compile(
            new PatternInput(
                TypedPatterns: new Dictionary<PatternKind, IEnumerable<string>> {
                    [PatternKind.Regex] = ["r:^a{2,3}$"]
                },
                InterpretationMode: PatternInterpretationMode.Specific),
            PatternMatchingMode.Regex);

        patterns.IsMatch("aa").Should().BeTrue();
        patterns.IsMatch("aaa").Should().BeTrue();
        patterns.IsMatch("a").Should().BeFalse();
    }

    /// <summary>
    /// Brace expansion works through the engine on a real tree.
    /// </summary>
    [TestMethod]
    public void Execute_MatchesBraceAlternatives() {
        using var environment = new TestEnvironment();
        environment.CreateFiles("a.cs", "b.vb", "c.txt");

        var paths = FileQuery.From(environment.Root)
            .Where("*.{cs,vb}")
            .Execute(TestContext.CancellationToken)
            .Select(path => Path.GetRelativePath(environment.Root, path))
            .OrderBy(static path => path, StringComparer.Ordinal)
            .ToArray();

        paths.Should().Equal(_expectedFiles);
    }

    private static void CompileGlob(string pattern) =>
        FileQueryPatternSet.Compile(
            new PatternInput(
                TypedPatterns: new Dictionary<PatternKind, IEnumerable<string>> {
                    [PatternKind.Glob] = [pattern]
                },
                InterpretationMode: PatternInterpretationMode.Specific),
            PatternMatchingMode.Glob);

    private static FileQueryPatternSet CompileGlobSet(string pattern) =>
        FileQueryPatternSet.Compile(
            new PatternInput(
                TypedPatterns: new Dictionary<PatternKind, IEnumerable<string>> {
                    [PatternKind.Glob] = [pattern]
                },
                InterpretationMode: PatternInterpretationMode.Specific),
            PatternMatchingMode.Glob);

    /// <summary>
    /// Gets or sets the test context, which provides information about the current test run.
    /// Used here primarily for accessing the <see cref="CancellationToken"/>.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;
}
