//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Architecture;

/// <summary>
/// Contains architectural tests for the <see cref="PatternScanner"/> class, focusing on pattern validation.
/// </summary>
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

    /// <summary>
    /// Verifies that <see cref="PatternScanner.Scan"/> does not throw <see cref="PatternException"/> for known invalid pattern strings.
    /// </summary>
    [TestMethod]
    public void PatternScanner_MustNotThrow_PatternException() {
        foreach(var syntax in _profiles) {
            foreach(var pattern in _invalidPatterns) {
                var context = new PatternCompilationContext(
                    pattern: new(Text: pattern, Type: PatternKind.GitIgnore)
                );

                FluentActions.Invoking(() => PatternScanner.Scan(context, syntax)).Should().NotThrow();
            }
        }
    }
}

