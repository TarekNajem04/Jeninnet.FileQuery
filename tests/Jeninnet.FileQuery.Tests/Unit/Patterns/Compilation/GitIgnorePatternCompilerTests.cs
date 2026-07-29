namespace Jeninnet.FileQuery.Tests.Unit.Patterns.Compilation;

/// <summary>
/// Tests for GitIgnorePatternCompilerTests.
/// </summary>
[TestClass]
public class GitIgnorePatternCompilerTests {
    /// <summary>
    /// Verifies that Should SetIsNegated When NegatedPatternCompiled.
    /// </summary>
    [TestMethod]
    public void Should_SetIsNegated_When_NegatedPatternCompiled() {
        var compiledPatternSets = CompiledPatternFactory.Compile(PatternKind.GitIgnore, "!bin/");

        compiledPatternSets.Should().HaveCount(1, "because we compiled a single pattern string");
        var compiledPattern = compiledPatternSets[0];

        Assert.IsTrue(compiledPattern.IsNegated);
        Assert.IsTrue(compiledPattern.DirectoryOnly);
        /*
         * 1- [! + end with /] if(negated && directoryOnly) Prduce segments:
         * [RecursiveWildcard(**)] // PatternToken[] { new RecursiveWildcardToken() }
         * 2- bin produce segments:
         * [Literal(bin)] // PatternToken[] { new LiteralToken() }
         */
        compiledPattern.Segments.Should().HaveCount(2);
    }

    /// <summary>
    /// Verifies that Should ProduceSegments When AnchoredPatternCompiled.
    /// </summary>
    [TestMethod]
    public void Should_ProduceSegments_When_AnchoredPatternCompiled() {
        var compiledPatternSets = CompiledPatternFactory.Compile(PatternKind.GitIgnore, "/obj/**/*.tmp");

        compiledPatternSets.Should().HaveCount(1, "because we compiled a single pattern string");
        var compiledPattern = compiledPatternSets[0];
        Assert.IsFalse(compiledPattern.IsNegated);
        Assert.IsFalse(compiledPattern.DirectoryOnly);
        // Should produce the segments: "obj", "**", "*.tmp"
        compiledPattern.Segments.Should().HaveCount(3);
    }

    /// <summary>
    /// Verifies that Should ProduceRecursiveToken When DoubleStarCompiled.
    /// </summary>
    [TestMethod]
    public void Should_ProduceRecursiveToken_When_DoubleStarCompiled() {
        var compiledPatternSets = CompiledPatternFactory.Compile(PatternKind.GitIgnore, "**/test");

        compiledPatternSets.Should().HaveCount(1, "because we compiled a single pattern string");
        var compiledPattern = compiledPatternSets[0];

        compiledPattern.Segments[0].Should().ContainSingle(t => t is RecursiveWildcardToken);
    }

    /// <summary>
    /// Verifies that Should HandleCorrectly When EmptyPatternCompiled.
    /// </summary>
    [TestMethod]
    public void Should_HandleCorrectly_When_EmptyPatternCompiled() {
        var compiledPatternSets = CompiledPatternFactory.Compile(PatternKind.GitIgnore, "");
        Assert.IsEmpty(compiledPatternSets);
    }
}

