//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Unit.Patterns.Classification;

/// <summary>
/// Tests for PatternClassifierAdditionalTests.
/// </summary>
[TestClass]
public class PatternClassifierAdditionalTests {
    /// <summary>
    /// Verifies that Should HandleVariousPrefixes When Classified.
    /// </summary>
    [TestMethod]
    public void Should_HandleVariousPrefixes_When_Classified() {
        // Test cases based on code review of PatternClassifier
        Assert.AreEqual(PatternKind.GitIgnore, PatternClassifier.Classify("!foo"));
        Assert.AreEqual(PatternKind.GitIgnore, PatternClassifier.Classify("/foo"));
        Assert.AreEqual(PatternKind.GitIgnore, PatternClassifier.Classify("foo/"));
        Assert.AreEqual(PatternKind.GitIgnore, PatternClassifier.Classify("**/foo"));
    }
}
