//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Unit.Patterns.Compilation;

/// <summary>
/// Tests for GlobPatternCompilerTests.
/// </summary>
[TestClass]
public class GlobPatternCompilerTests {
    /// <summary>
    /// Verifies that Should ProduceWildcardToken When SingleStarCompiled.
    /// </summary>
    [TestMethod]
    public void Should_ProduceWildcardToken_When_SingleStarCompiled() {
        var compiledPatternSets = CompiledPatternFactory.Compile(PatternKind.Glob, "*.cs");

        compiledPatternSets.Should().HaveCount(1, "because we compiled a single pattern string");
        var compiledPattern = compiledPatternSets[0];

        compiledPattern.Segments[0].Should().ContainSingle(static x => x is WildcardToken);
    }

    /// <summary>
    /// Verifies that Should ProduceRecursiveWildcardToken When DoubleStarCompiled.
    /// </summary>
    [TestMethod]
    public void Should_ProduceRecursiveWildcardToken_When_DoubleStarCompiled() {
        var compiledPatternSets = CompiledPatternFactory.Compile(PatternKind.Glob, "**/*.cs");

        compiledPatternSets.Should().HaveCount(1, "because we compiled a single pattern string");
        var compiledPattern = compiledPatternSets[0];

        compiledPattern.Segments[0].Should().ContainSingle(static x => x is RecursiveWildcardToken);
    }
}
