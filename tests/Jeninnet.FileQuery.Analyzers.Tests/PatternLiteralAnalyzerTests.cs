//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Analyzers.Tests;

/// <summary>
/// Tests for <see cref="PatternLiteralAnalyzer"/> and its code fix.
/// </summary>
[TestClass]
public sealed class PatternLiteralAnalyzerTests {
    private const string HEADER =
        """
        using Jeninnet.FileQuery;
        using Jeninnet.FileQuery.Enums;

        public static class Consumer
        {
            public static void Use(FileQueryBuilder builder)
            {
        """;

    private const string FOOTER =
        """
            }
        }
        """;

    /// <summary>
    /// A missing closing bracket is reported.
    /// </summary>
    [TestMethod]
    public async Task Analyzer_Reports_MissingClosingBracketAsync() {
        var source = Compose("""builder.Where(PatternKind.GitIgnore, ["a[b"]);""");

        var diagnostics = await AnalyzerTestHost.GetDiagnosticsAsync(source);

        diagnostics.Should().HaveCount(1);
        diagnostics[0].Id.Should().Be(PatternLiteralAnalyzer.DiagnosticId);
        diagnostics[0].GetMessage(CultureInfo.InvariantCulture).Should().Contain("missing closing bracket");
    }

    /// <summary>
    /// A trailing backslash is reported.
    /// </summary>
    [TestMethod]
    public async Task Analyzer_Reports_TrailingBackslashAsync() {
        var source = Compose("""builder.Where(PatternKind.GitIgnore, ["a\\"]);""");

        var diagnostics = await AnalyzerTestHost.GetDiagnosticsAsync(source);

        diagnostics.Should().HaveCount(1);
        diagnostics[0].GetMessage(CultureInfo.InvariantCulture).Should().Contain("unescaped backslash");
    }

    /// <summary>
    /// Delimited string overloads are split before validation.
    /// </summary>
    [TestMethod]
    public async Task Analyzer_Reports_MalformedPartOfDelimitedStringAsync() {
        var source = Compose("""builder.Where("*.cs;[a-]");""");

        var diagnostics = await AnalyzerTestHost.GetDiagnosticsAsync(source);

        diagnostics.Should().HaveCount(1);
        diagnostics[0].GetMessage(CultureInfo.InvariantCulture).Should().Contain("invalid range");
    }

    /// <summary>
    /// Valid patterns, including POSIX classes and escaped brackets, do not produce diagnostics.
    /// </summary>
    [TestMethod]
    public async Task Analyzer_DoesNotReport_ValidPatternsAsync() {
        var source = Compose("""builder.Where(PatternKind.GitIgnore, ["**/*.cs", "!bin/", "[[:digit:]]", "a[0-9]", "\\[literal\\]"]);""");

        var diagnostics = await AnalyzerTestHost.GetDiagnosticsAsync(source);

        diagnostics.Should().BeEmpty();
    }

    /// <summary>
    /// Non-constant values are ignored to avoid false positives.
    /// </summary>
    [TestMethod]
    public async Task Analyzer_DoesNotReport_NonConstantPatternsAsync() {
        var source = Compose(
            """
            var pattern = LoadPattern();
            builder.Where(PatternKind.GitIgnore, [pattern]);

            static string LoadPattern() => "a[b";
            """);

        var diagnostics = await AnalyzerTestHost.GetDiagnosticsAsync(source);

        diagnostics.Should().BeEmpty();
    }

    /// <summary>
    /// Other <c>Where</c> methods are not analyzed.
    /// </summary>
    [TestMethod]
    public async Task Analyzer_DoesNotReport_UnrelatedWhereMethodAsync() {
        var source = Compose(
            """
            Unrelated.Where("a[b");

            static class Unrelated
            {
                public static void Where(string value) { }
            }
            """);

        var diagnostics = await AnalyzerTestHost.GetDiagnosticsAsync(source);

        diagnostics.Should().BeEmpty();
    }

    /// <summary>
    /// The code fix closes a character class when the rewrite is safe.
    /// </summary>
    [TestMethod]
    public async Task CodeFix_ClosesCharacterClassAsync() {
        var source = Compose("""builder.Where(PatternKind.GitIgnore, ["a[b"]);""");

        var fixedSource = await AnalyzerTestHost.ApplyFirstFixAsync(source);

        fixedSource.Should().Contain("""["a[b]"]""");
    }

    /// <summary>
    /// Builds a compilable source file around a method body statement.
    /// </summary>
    /// <param name="body">The statement to insert.</param>
    private static string Compose(string body) => HEADER + body + FOOTER;
}
