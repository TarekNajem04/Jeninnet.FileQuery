//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tooling.Tests;

/// <summary>
/// Tests the public tooling surface: classification, validation, and inspection.
/// </summary>
[TestClass]
public sealed class PatternInspectorTests {
    /// <summary>
    /// Verifies the classifier results for the supported dialects.
    /// </summary>
    [TestMethod]
    public void Classify_ShouldReturnExpectedDialect() {
        PatternInspector.Classify("*.cs").Should().Be(PatternKind.GitIgnore);
        PatternInspector.Classify("!*.tmp").Should().Be(PatternKind.GitIgnore);
        PatternInspector.Classify(@"src\*.cs").Should().Be(PatternKind.Glob);
        PatternInspector.Classify(@"r:.*\.cs$").Should().Be(PatternKind.Regex);
    }

    /// <summary>
    /// Verifies that a malformed pattern is classified as unknown.
    /// </summary>
    [TestMethod]
    public void Classify_ShouldReturnUnknown_WhenPatternIsMalformed() => PatternInspector.Classify("[").Should().Be(PatternKind.Unknown);

    /// <summary>
    /// Verifies structural validation messages.
    /// </summary>
    [TestMethod]
    public void Validate_ShouldReportMissingClosingBracket() {
        var validation = PatternInspector.Validate("[a-z");

        validation.IsValid.Should().BeFalse();
        validation.Error.Should().Contain("Missing closing bracket");
    }

    /// <summary>
    /// Verifies that a valid pattern passes validation.
    /// </summary>
    [TestMethod]
    public void Validate_ShouldAcceptValidPattern() {
        var validation = PatternInspector.Validate("src/**/*.cs");

        validation.IsValid.Should().BeTrue();
        validation.Error.Should().BeNull();
    }

    /// <summary>
    /// Verifies the structural flags reported by inspection.
    /// </summary>
    [TestMethod]
    public void Inspect_ShouldExposeStructuralFlags() {
        var inspection = PatternInspector.Inspect("!src/**/*.cs");

        inspection.Kind.Should().Be(PatternKind.GitIgnore);
        inspection.IsNegated.Should().BeTrue();
        inspection.HasWildcard.Should().BeTrue();
        inspection.HasRecursiveWildcard.Should().BeTrue();
        inspection.HasCharacterClass.Should().BeFalse();
        inspection.IsRootAnchored.Should().BeFalse();
        inspection.IsDirectoryOnly.Should().BeFalse();
        inspection.SegmentCount.Should().Be(3);
        inspection.Segments.Should().HaveCount(3);
    }

    /// <summary>
    /// Verifies root anchoring and directory-only detection.
    /// </summary>
    [TestMethod]
    public void Inspect_ShouldDetectAnchoringAndDirectoryOnly() {
        var inspection = PatternInspector.Inspect("!/src/");

        inspection.IsNegated.Should().BeTrue();
        inspection.IsRootAnchored.Should().BeTrue();
        inspection.IsDirectoryOnly.Should().BeTrue();
    }

    /// <summary>
    /// Verifies tokenization of literals and wildcards.
    /// </summary>
    [TestMethod]
    public void Inspect_ShouldTokenizeSegments() {
        var inspection = PatternInspector.Inspect("src/*.cs");

        inspection.Segments.Should().HaveCount(2);
        inspection.Segments[0].Tokens.Should().BeEquivalentTo(
            [new PatternTokenInfo(PatternTokenKind.Literal, "src")]);
        inspection.Segments[1].Tokens.Should().BeEquivalentTo(
            [
                new PatternTokenInfo(PatternTokenKind.Wildcard, "*"),
                new PatternTokenInfo(PatternTokenKind.Literal, ".cs")
            ]);
    }

    /// <summary>
    /// Verifies character class rendering.
    /// </summary>
    [TestMethod]
    public void Inspect_ShouldRenderCharacterClasses() {
        var inspection = PatternInspector.Inspect("[!a-z0-9]");

        inspection.HasCharacterClass.Should().BeTrue();
        inspection.Segments.Should().HaveCount(1);
        inspection.Segments[0].Tokens.Should().Contain(
            new PatternTokenInfo(PatternTokenKind.CharacterClass, "[!a-z0-9]"));
    }

    /// <summary>
    /// Verifies POSIX class rendering.
    /// </summary>
    [TestMethod]
    public void Inspect_ShouldRenderPosixClasses() {
        var inspection = PatternInspector.Inspect("[[:digit:]]");

        inspection.Segments[0].Tokens.Should().Contain(
            new PatternTokenInfo(PatternTokenKind.CharacterClass, "[[:digit:]]"));
    }

    /// <summary>
    /// Verifies escaped character rendering.
    /// </summary>
    [TestMethod]
    public void Inspect_ShouldRenderEscapedCharacters() {
        var inspection = PatternInspector.Inspect(@"\*");

        inspection.HasEscapedCharacters.Should().BeTrue();
        inspection.Segments[0].Tokens.Should().Contain(
            new PatternTokenInfo(PatternTokenKind.Literal, "*"));
    }

    /// <summary>
    /// Verifies regex token rendering.
    /// </summary>
    [TestMethod]
    public void Inspect_ShouldRenderRegexTokens() {
        var inspection = PatternInspector.Inspect(@"r:^a.*$");

        inspection.Kind.Should().Be(PatternKind.Regex);
        inspection.IsRegex.Should().BeTrue();
        inspection.Segments[0].Tokens.Should().Contain(
            new PatternTokenInfo(PatternTokenKind.RegularExpression, "^a.*$"));
    }

    /// <summary>
    /// Verifies inspection with a forced dialect.
    /// </summary>
    [TestMethod]
    public void Inspect_ShouldUseForcedDialect() {
        var inspection = PatternInspector.Inspect("*.cs", PatternKind.Glob);

        inspection.Kind.Should().Be(PatternKind.Glob);
        inspection.IsRegex.Should().BeFalse();
        inspection.Segments[0].Tokens[0].Kind.Should().Be(PatternTokenKind.Wildcard);
    }

    /// <summary>
    /// Verifies that a forced unknown dialect is rejected.
    /// </summary>
    [TestMethod]
    public void Inspect_ShouldRejectForcedUnknownDialect() => Assert.Throws<ArgumentException>(() =>
            PatternInspector.Inspect("*.cs", PatternKind.Unknown));

    /// <summary>
    /// Verifies malformed pattern reporting.
    /// </summary>
    [TestMethod]
    public void Inspect_ShouldReportMalformedPattern() {
        var inspection = PatternInspector.Inspect("[");

        inspection.Kind.Should().Be(PatternKind.Unknown);
        inspection.Validation.IsValid.Should().BeFalse();
        inspection.Validation.Error.Should().NotBeNull();
    }

    /// <summary>
    /// Verifies empty pattern handling.
    /// </summary>
    [TestMethod]
    public void Inspect_ShouldHandleEmptyPattern() {
        var inspection = PatternInspector.Inspect(string.Empty);

        inspection.Kind.Should().Be(PatternKind.GitIgnore);
        inspection.Segments.Should().BeEmpty();
        inspection.SegmentCount.Should().Be(0);
        inspection.Validation.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that null input is rejected.
    /// </summary>
    [TestMethod]
    public void Methods_ShouldRejectNullPattern() {
        Assert.Throws<ArgumentNullException>(() => PatternInspector.Classify(null!));
        Assert.Throws<ArgumentNullException>(() => PatternInspector.Validate(null!));
        Assert.Throws<ArgumentNullException>(() => PatternInspector.Inspect(null!));
    }
}
