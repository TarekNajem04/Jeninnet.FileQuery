//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Patterns.Tokenization;

/// <summary>
/// Contains unit tests for the <see cref="PatternScanner"/> class, focusing on pattern tokenization.
/// </summary>
[TestClass]
public sealed class PatternScannerTests {
    /// <summary>
    /// Verifies that <see cref="PatternScanner.Scan"/> correctly tokenizes a simple literal pattern string.
    /// </summary>
    [TestMethod]
    public void Scan_ShouldTokenizeSimpleLiteral() {
        var pattern = new ClassifiedPattern("foo/bar", PatternKind.GitIgnore);
        var context = new PatternCompilationContext(pattern);
        var syntax = PatternSyntaxProfile.GitIgnore;

        PatternScanner.Scan(context, syntax);

        Assert.IsNotNull(context.Tokens);
        Assert.HasCount(2, context.Tokens); // foo, bar
    }
}
