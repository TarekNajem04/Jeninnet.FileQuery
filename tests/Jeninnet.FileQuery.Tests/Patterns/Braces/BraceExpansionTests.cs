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
    private static readonly string[] EXPECTED_FILES = ["a.cs", "b.vb"];

    /// <summary>
    /// A brace group expands to individual alternatives.
    /// </summary>
    [TestMethod]
    public void Compile_ExpandsSimpleAlternation() {
        var patterns = FileQueryPatternSet.Compile(["*.{cs,vb}"]);

        Assert.IsTrue(patterns.IsMatch("Program.cs"));
        Assert.IsTrue(patterns.IsMatch("Program.vb"));
        Assert.IsFalse(patterns.IsMatch("Program.txt"));
    }

    /// <summary>
    /// Nested groups form a cartesian product.
    /// </summary>
    [TestMethod]
    public void Compile_ExpandsNestedAlternation() {
        var patterns = CompileGlobSet("{src,test}/**/*.{cs,vb}");

        Assert.IsTrue(patterns.IsMatch("src/Program.cs"));
        Assert.IsTrue(patterns.IsMatch("test/Program.vb"));
        Assert.IsFalse(patterns.IsMatch("test/Program.txt"));
    }

    /// <summary>
    /// Escaped braces remain literal.
    /// </summary>
    [TestMethod]
    public void Compile_TreatsEscapedBracesAsLiteral() {
        var patterns = FileQueryPatternSet.Compile([@"\{a,b\}.txt"]);

        Assert.IsTrue(patterns.IsMatch("{a,b}.txt"));
        Assert.IsFalse(patterns.IsMatch("a.txt"));
    }

    /// <summary>
    /// Brace alternation is classified as a Glob feature.
    /// </summary>
    [TestMethod]
    public void Classification_RecognizesBraceAlternation() {
        Assert.AreEqual(PatternKind.Glob, PatternClassifier.Classify("*.{cs,vb}"));
    }

    /// <summary>
    /// An unterminated group reports a validation error with a position.
    /// </summary>
    [TestMethod]
    public void Compile_ReportsUnterminatedBrace() {
        var exception = Assert.ThrowsExactly<PatternException>(() => CompileGlob("src/{a,b"));

        Assert.AreEqual(4, exception.Position);
        Assert.AreEqual(1, exception.Segment);
        StringAssert.Contains(exception.Message, "matching closing brace");
    }
    /// <summary>
    /// A stray closing brace is rejected.
    /// </summary>
    [TestMethod]
    public void Compile_ReportsStrayClosingBrace() {
        var exception = Assert.ThrowsExactly<PatternException>(() => CompileGlob("src/a}b"));

        Assert.AreEqual(5, exception.Position);
        StringAssert.Contains(exception.Message, "no matching opening brace");
    }

    /// <summary>
    /// Empty alternatives are rejected instead of producing an empty pattern.
    /// </summary>
    [TestMethod]
    public void Compile_RejectsEmptyAlternative() {
        var exception = Assert.ThrowsExactly<PatternException>(() => CompileGlob("src/{a,}"));

        StringAssert.Contains(exception.Message, "cannot be empty");
    }

    /// <summary>
    /// Expansion is bounded to avoid runaway pattern input.
    /// </summary>
    [TestMethod]
    public void Compile_RejectsExcessiveExpansion() {
        var pattern = string.Concat(Enumerable.Repeat("{a,b}", 10));

        var exception = Assert.ThrowsExactly<PatternException>(() => CompileGlob(pattern));

        StringAssert.Contains(exception.Message, "exceeds");
    }

    /// <summary>
    /// Regex quantifiers are not treated as brace alternation.
    /// </summary>
    [TestMethod]
    public void Compile_DoesNotExpandRegexBraces() {
        var patterns = FileQueryPatternSet.Compile(
            new PatternInput(
                TypedPatterns: new Dictionary<PatternKind, IEnumerable<string>> {
                    [PatternKind.Regex] = [@"r:^a{2,3}$"]
                },
                InterpretationMode: PatternInterpretationMode.Specific),
            PatternMatchingMode.Regex);

        Assert.IsTrue(patterns.IsMatch("aa"));
        Assert.IsTrue(patterns.IsMatch("aaa"));
        Assert.IsFalse(patterns.IsMatch("a"));
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
            .Execute()
            .Select(path => Path.GetRelativePath(environment.Root, path))
            .OrderBy(static path => path, StringComparer.Ordinal)
            .ToArray();

        CollectionAssert.AreEqual(EXPECTED_FILES, paths);
    }

    private static void CompileGlob(string pattern) =>
        _ = FileQueryPatternSet.Compile(
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
}
