//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Unit.Patterns.Classification;

/// <summary>
/// Tests for PatternClassifierTests.
/// </summary>
[TestClass]
public sealed class PatternClassifierTests {
    /// <summary>
    /// Verifies that Should HandleAllBranches When Classified.
    /// </summary>
    [TestMethod]
    public void Should_HandleAllBranches_When_Classified() {
        // Unknown (Malformed)
        (PatternClassifier.Classify("[a-z")).Should().Be(PatternKind.Unknown);

        // Empty
        PatternClassifier.Classify("").Should().Be(PatternKind.GitIgnore);

        // Regex
        (PatternClassifier.Classify("r:.*")).Should().Be(PatternKind.Regex);
        (PatternClassifier.Classify("regex:^.*$")).Should().Be(PatternKind.GitIgnore);
        PatternClassifier.Classify("regex:").Should().Be(PatternKind.GitIgnore);

        // Glob (Stray bracket)
        PatternClassifier.Classify("a]").Should().Be(PatternKind.Glob);

        // Glob (Windows Path)
        PatternClassifier.Classify(@"C:\foo").Should().Be(PatternKind.Glob);
        PatternClassifier.Classify(@"C:\").Should().Be(PatternKind.Unknown);

        // Glob (Windows Path - negative)
        PatternClassifier.Classify(@"!C:\foo").Should().NotBe(PatternKind.Glob);

        // Literal with escaped characters (no backslash)
        PatternClassifier.Classify(@"\!foo").Should().Be(PatternKind.GitIgnore);

        // GitIgnore (GitIgnore syntax)
        PatternClassifier.Classify("!foo").Should().Be(PatternKind.GitIgnore);

        // GitIgnore (Wildcard)
        (PatternClassifier.Classify("*")).Should().Be(PatternKind.GitIgnore);

        // Literal
        PatternClassifier.Classify("foo").Should().Be(PatternKind.GitIgnore);
    }
}
