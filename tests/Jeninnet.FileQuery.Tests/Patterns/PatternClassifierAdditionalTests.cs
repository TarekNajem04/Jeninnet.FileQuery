//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Patterns;

/// <summary>
/// Contains additional unit tests for the <see cref="PatternClassifier"/> class, verifying its behavior with various pattern prefixes.
/// </summary>
[TestClass]
public class PatternClassifierAdditionalTests {
    /// <summary>
    /// Verifies that the <see cref="PatternClassifier.Classify(string)"/> method correctly handles various GitIgnore pattern prefixes.
    /// </summary>
    [TestMethod]
    public void Classify_HandleVariousPrefixes() {
        // Test cases based on code review of PatternClassifier
        Assert.AreEqual(PatternKind.GitIgnore, PatternClassifier.Classify("!foo"));
        Assert.AreEqual(PatternKind.GitIgnore, PatternClassifier.Classify("/foo"));
        Assert.AreEqual(PatternKind.GitIgnore, PatternClassifier.Classify("foo/"));
        Assert.AreEqual(PatternKind.GitIgnore, PatternClassifier.Classify("**/foo"));
    }
}
