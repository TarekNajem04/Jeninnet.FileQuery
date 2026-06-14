namespace Jeninnet.FileQuery.Tests.PatternEngine;

[TestClass]
public class GitIgnorePatternCompilerTests
{
    [TestMethod]
    public void NegatedPattern_ShouldSetIsNegated()
    {
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

    [TestMethod]
    public void AnchoredPattern_ShouldProduceSegments()
    {
        var compiledPatternSets = CompiledPatternFactory.Compile(PatternKind.GitIgnore, "/obj/**/*.tmp");

        TestAssertEx.HasCount(compiledPatternSets, 1, "because we compiled a single pattern string");
        var compiledPattern = compiledPatternSets[0];
        Assert.IsFalse(compiledPattern.IsNegated);
        Assert.IsFalse(compiledPattern.DirectoryOnly);
        // Should produce the segments: "obj", "**", "*.tmp"
        TestAssertEx.HasCount(compiledPattern.Segments, 3);
    }

    [TestMethod]
    public void DoubleStar_ShouldProduceRecursiveToken()
    {
        var compiledPatternSets = CompiledPatternFactory.Compile(PatternKind.GitIgnore, "**/test");

        TestAssertEx.HasCount(compiledPatternSets, 1, "because we compiled a single pattern string");
        var compiledPattern = compiledPatternSets[0];

        TestAssertEx.ContainsSingle(compiledPattern.Segments[0], t => t is RecursiveWildcardToken);
    }

    [TestMethod]
    public void GitIgnore_EmptyPattern_ShouldHandleCorrectly()
    {
        var compiledPatternSets = CompiledPatternFactory.Compile(PatternKind.GitIgnore, "");
        Assert.IsEmpty(compiledPatternSets);
    }
}
