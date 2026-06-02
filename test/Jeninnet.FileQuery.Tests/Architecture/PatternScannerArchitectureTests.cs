namespace Jeninnet.FileQuery.Tests.Architecture;

[TestClass]
public sealed class PatternScannerArchitectureTests {
    private static readonly PatternSyntaxProfile[] _profiles =
    [
        PatternSyntaxProfile.GitIgnore,
        PatternSyntaxProfile.Glob,
        PatternSyntaxProfile.Regex
    ];

    private static readonly string[] _invalidPatterns =
    [
        "",
        " ",
        "***",
        "**a",
        "a**b",
        "[",
        "[a-]",
        "[z-a]",
        "\\",
        "!",
        "/",
        "///",
        "****/",
        "r:["
    ];

    [TestMethod]
    public void PatternScanner_MustNotThrow_PatternException() {
        foreach(var syntax in _profiles) {
            foreach(var pattern in _invalidPatterns) {
                var context = new PatternCompilationContext(
                    pattern: new(Text: pattern, Type: PatternKind.GitIgnore)
                );

                try {
                    PatternScanner.Scan(context, syntax);
                }
                catch(PatternException ex) {
                    throw new AssertFailedException(
                        $"""
                        PatternScanner threw PatternException.

                        Pattern: "{pattern}"
                        Syntax: {syntax}

                        Exception:
                        {ex}
                        """
                    );
                }
            }
        }
    }
}
