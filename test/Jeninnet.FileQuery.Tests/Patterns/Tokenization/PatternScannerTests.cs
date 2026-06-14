namespace Jeninnet.FileQuery.Tests.Patterns.Tokenization;

[TestClass]
public sealed class PatternScannerTests
{
    [TestMethod]
    public void Scan_ShouldTokenizeSimpleLiteral()
    {
        var pattern = new ClassifiedPattern("foo/bar", PatternKind.GitIgnore);
        var context = new PatternCompilationContext(pattern);
        var syntax = PatternSyntaxProfile.GitIgnore;

        PatternScanner.Scan(context, syntax);

        Assert.IsNotNull(context.Tokens);
        Assert.HasCount(2, context.Tokens); // foo, bar
    }
}
