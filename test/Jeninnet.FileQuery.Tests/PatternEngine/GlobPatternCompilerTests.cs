namespace Jeninnet.FileQuery.Tests.PatternEngine;

[TestClass]
public class GlobPatternCompilerTests {
    [TestMethod]
    public void SingleStar_ShouldProduceWildcardToken() {
        var compiledPatternSets = CompiledPatternFactory.Compile(PatternKind.Glob, "*.cs");

        TestAssertEx.HasCount(compiledPatternSets, 1, "because we compiled a single pattern string");
        var compiledPattern = compiledPatternSets[0];

        TestAssertEx.ContainsSingle(compiledPattern.Segments[0], x => x is WildcardToken);
    }

    [TestMethod]
    public void DoubleStar_ShouldProduceRecursiveWildcardToken() {
        var compiledPatternSets = CompiledPatternFactory.Compile(PatternKind.Glob, "**/*.cs");

        TestAssertEx.HasCount(compiledPatternSets, 1, "because we compiled a single pattern string");
        var compiledPattern = compiledPatternSets[0];

        TestAssertEx.ContainsSingle(compiledPattern.Segments[0], x => x is RecursiveWildcardToken);
    }
}
