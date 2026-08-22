//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.PatternEngine;

/// <summary>
/// Provides unit tests for the <c>GlobPatternCompiler</c> class.
/// </summary>
[TestClass]
public class GlobPatternCompilerTests {
    /// <summary>Tests SingleStar_ShouldProduceWildcardToken.</summary>
    [TestMethod]
    public void SingleStar_ShouldProduceWildcardToken() {
        var compiledPatternSets = CompiledPatternFactory.Compile(PatternKind.Glob, "*.cs");

        TestAssertEx.HasCount(compiledPatternSets, 1, "because we compiled a single pattern string");
        var compiledPattern = compiledPatternSets[0];

        TestAssertEx.ContainsSingle(compiledPattern.Segments[0], static x => x is WildcardToken);
    }

    /// <summary>Tests DoubleStar_ShouldProduceRecursiveWildcardToken.</summary>
    [TestMethod]
    public void DoubleStar_ShouldProduceRecursiveWildcardToken() {
        var compiledPatternSets = CompiledPatternFactory.Compile(PatternKind.Glob, "**/*.cs");

        TestAssertEx.HasCount(compiledPatternSets, 1, "because we compiled a single pattern string");
        var compiledPattern = compiledPatternSets[0];

        TestAssertEx.ContainsSingle(compiledPattern.Segments[0], static x => x is RecursiveWildcardToken);
    }
}
