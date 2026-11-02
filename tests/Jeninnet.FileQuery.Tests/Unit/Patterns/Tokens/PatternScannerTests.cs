//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Unit.Patterns.Tokens;

/// <summary>
/// Tests for PatternScannerTests.
/// </summary>
[TestClass]
public sealed class PatternScannerTests {
    /// <summary>
    /// Verifies that Should TokenizeSimpleLiteral When Scanned.
    /// </summary>
    [TestMethod]
    public void Should_TokenizeSimpleLiteral_When_Scanned() {
        var pattern = new ClassifiedPattern("foo/bar", PatternKind.GitIgnore);
        var context = new PatternCompilationContext(pattern);
        var syntax = PatternSyntaxProfile.GitIgnore;

        PatternScanner.Scan(context, syntax);

        context.Tokens.Should().NotBeNull();
        context.Tokens.Should().HaveCount(2); // foo, bar
    }
}
