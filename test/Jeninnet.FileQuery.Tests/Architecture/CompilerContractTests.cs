namespace Jeninnet.FileQuery.Tests.Architecture;

/// <summary>
/// Contains architecture tests for the compiler contracts.
/// </summary>
[TestClass]
public sealed class CompilerContractTests {
    /// <summary>
    /// Tests that the compiler rejects the wrong pattern type.
    /// </summary>
    [TestMethod]
    public void Compiler_Rejects_Wrong_PatternType() {
        var compiler = new GlobPatternCompiler();

        var pattern = new ClassifiedPattern(Text: "*.cs", Type: PatternKind.GitIgnore);

        Assert.ThrowsExactly<PatternException>(() =>
            compiler.Compile(new(pattern)));
    }
}
