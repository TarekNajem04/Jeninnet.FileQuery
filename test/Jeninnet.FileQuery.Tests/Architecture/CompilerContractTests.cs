namespace Jeninnet.FileQuery.Tests.Architecture;

[TestClass]
public sealed class CompilerContractTests {
    [TestMethod]
    public void Compiler_Rejects_Wrong_PatternType() {
        var compiler = new GlobPatternCompiler();

        var pattern = new ClassifiedPattern(Text: "*.cs", Type: PatternKind.GitIgnore);

        Assert.ThrowsExactly<PatternException>(() =>
            compiler.Compile(new(pattern)));
    }
}
