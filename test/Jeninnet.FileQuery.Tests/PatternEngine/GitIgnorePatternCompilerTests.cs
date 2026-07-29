namespace Jeninnet.FileQuery.Tests.PatternEngine;

/// <summary>
/// Provides unit tests for the <c>GitIgnorePatternCompiler</c> class.
/// </summary>
[TestClass]
public class GitIgnorePatternCompilerTests {
    /// <summary>
    /// Verifies that a negated pattern string is correctly compiled and marked as negated.
    /// </summary>
    [TestMethod]
    public void NegatedPattern_ShouldSetIsNegated() {
        var compiledPatternSets = CompiledPatternFactory.Compile(PatternKind.GitIgnore, "!bin/");

        TestAssertEx.HasCount(compiledPatternSets, 1, "because we compiled a single pattern string");
        var compiledPattern = compiledPatternSets[0];

        Assert.IsTrue(compiledPattern.IsNegated);
        Assert.IsTrue(compiledPattern.DirectoryOnly);
        /*
         * 1- [! + end with /] if(negated && directoryOnly) Prduce segments:
         * [RecursiveWildcard(**)] // PatternToken[] { new RecursiveWildcardToken() }
         * 2- bin produce segments:
         * [Literal(bin)] // PatternToken[] { new LiteralToken() }
         */
        TestAssertEx.HasCount(compiledPattern.Segments, 2);
    }

    /// <summary>
    /// Verifies that an anchored pattern is correctly compiled into the expected segments.
    /// </summary>
    [TestMethod]
    public void AnchoredPattern_ShouldProduceSegments() {
        var compiledPatternSets = CompiledPatternFactory.Compile(PatternKind.GitIgnore, "/obj/**/*.tmp");

        TestAssertEx.HasCount(compiledPatternSets, 1, "because we compiled a single pattern string");
        var compiledPattern = compiledPatternSets[0];
        Assert.IsFalse(compiledPattern.IsNegated);
        Assert.IsFalse(compiledPattern.DirectoryOnly);
        // Should produce the segments: "obj", "**", "*.tmp"
        TestAssertEx.HasCount(compiledPattern.Segments, 3);
    }

    /// <summary>
    /// Verifies that a double star pattern is correctly compiled into a <see cref="RecursiveWildcardToken"/>.
    /// </summary>
    [TestMethod]
    public void DoubleStar_ShouldProduceRecursiveToken() {
        var compiledPatternSets = CompiledPatternFactory.Compile(PatternKind.GitIgnore, "**/test");

        TestAssertEx.HasCount(compiledPatternSets, 1, "because we compiled a single pattern string");
        var compiledPattern = compiledPatternSets[0];

        TestAssertEx.ContainsSingle(compiledPattern.Segments[0], t => t is RecursiveWildcardToken);
    }

    /// <summary>
    /// Verifies that an empty pattern string is handled gracefully, resulting in no compiled patterns.
    /// </summary>
    [TestMethod]
    public void GitIgnore_EmptyPattern_ShouldHandleCorrectly() {
        var compiledPatternSets = CompiledPatternFactory.Compile(PatternKind.GitIgnore, "");
        Assert.IsEmpty(compiledPatternSets);
    }
}

