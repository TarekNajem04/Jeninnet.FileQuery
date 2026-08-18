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
        Assert.AreEqual(PatternKind.Unknown, PatternClassifier.Classify("[a-z"));

        // Empty
        Assert.AreEqual(PatternKind.GitIgnore, PatternClassifier.Classify(""));

        // Regex
        Assert.AreEqual(PatternKind.Regex, PatternClassifier.Classify("r:.*"));
        Assert.AreEqual(PatternKind.GitIgnore, PatternClassifier.Classify("regex:^.*$"));
        Assert.AreEqual(PatternKind.GitIgnore, PatternClassifier.Classify("regex:"));

        // Glob (Stray bracket)
        Assert.AreEqual(PatternKind.Glob, PatternClassifier.Classify("a]"));

        // Glob (Windows Path)
        Assert.AreEqual(PatternKind.Glob, PatternClassifier.Classify(@"C:\foo"));
        Assert.AreEqual(PatternKind.Unknown, PatternClassifier.Classify(@"C:\"));

        // Glob (Windows Path - negative)
        Assert.AreNotEqual(PatternKind.Glob, PatternClassifier.Classify(@"!C:\foo"));

        // Literal with escaped characters (no backslash)
        Assert.AreEqual(PatternKind.GitIgnore, PatternClassifier.Classify(@"\!foo"));

        // GitIgnore (GitIgnore syntax)
        Assert.AreEqual(PatternKind.GitIgnore, PatternClassifier.Classify("!foo"));

        // GitIgnore (Wildcard)
        Assert.AreEqual(PatternKind.GitIgnore, PatternClassifier.Classify("*"));

        // Literal
        Assert.AreEqual(PatternKind.GitIgnore, PatternClassifier.Classify("foo"));
    }
}
