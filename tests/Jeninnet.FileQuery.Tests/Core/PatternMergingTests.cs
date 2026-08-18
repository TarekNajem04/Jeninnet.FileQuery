//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Core;

/// <summary>
/// Contains unit tests for the PatternsMerger functionality.
/// </summary>
[TestClass]
public class PatternMergingTests {
    /// <summary>
    /// Verifies the debugging output of the classifier for a sample pattern.
    /// </summary>
    [TestMethod]
    public void Debug_Classifier_Output() {
        var result = PatternClassifier.Classify(@"src\**\*.cs");

        Assert.AreEqual(PatternKind.Glob, result);
    }

    /// <summary>
    /// Verifies that Merge correctly classifies and groups raw input patterns.
    /// </summary>
    [TestMethod]
    public void MergePatterns_WithOnlyRawPatterns_ClassifiesAndGroupsCorrectly() {
        // Arrange
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "*.cs",            // Classified as GitIgnore (Superset rule)
                        "bin/",            // Classified as GitIgnore (Trailing slash)
                        @"src\**\*.cs"     // Classified as Glob (Backslash rule)
                    ]
                )
            )
        );

        // Act
        var result = PatternsMerger.Merge(options.PatternInput);

        // Assert
        // 1. Verify GitIgnore bucket (contains *.cs and bin/)
        Assert.IsTrue(result.ContainsKey(PatternKind.GitIgnore), "Should have GitIgnore patterns");
        Assert.HasCount(2, result[PatternKind.GitIgnore]);

        // 2. Verify Glob bucket (contains the Windows-style path)
        Assert.IsTrue(result.ContainsKey(PatternKind.Glob), "Should have Glob patterns");
        Assert.HasCount(1, result[PatternKind.Glob]);
        Assert.AreEqual(@"src\**\*.cs", result[PatternKind.Glob][0]);
    }

    /// <summary>
    /// Verifies that Merge preserves structure when only typed patterns are provided.
    /// </summary>
    [TestMethod]
    public void MergePatterns_WithOnlyTypedPatterns_PreservesStructure() {
        // Arrange
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(TypedPatterns: PatternHelpers.Create(PatternKind.Regex, "LICENSE", "README"))
            )
        );

        // Act
        var result = PatternsMerger.Merge(options.PatternInput);

        // Assert
        Assert.HasCount(1, result);
        Assert.HasCount(2, result[PatternKind.Regex]);
        Assert.AreEqual("LICENSE", result[PatternKind.Regex][0]);
    }

    /// <summary>
    /// Verifies that Merge correctly merges raw and typed patterns of the same type.
    /// </summary>
    [TestMethod]
    public void MergePatterns_WithOverlappingTypes_MergesListsSafely() {
        // Arrange
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                // Raw pattern that will be classified as GitIgnore
                PatternInput: new(
                    Patterns: ["node_modules/"],
                    // Explicitly typed GitIgnore pattern
                    TypedPatterns: PatternHelpers.Create(PatternKind.GitIgnore, ".env")
                )
            )
        );

        // Act
        var result = PatternsMerger.Merge(options.PatternInput);

        // Assert
        Assert.HasCount(1, result);
        var gitIgnoreList = result[PatternKind.GitIgnore];
        Assert.HasCount(2, gitIgnoreList);

        // Verification of "Last Rule Wins" order: Typed first, then Raw appended
        Assert.AreEqual(".env", gitIgnoreList[0]);
        Assert.AreEqual("node_modules/", gitIgnoreList[1]);
    }

    /// <summary>
    /// Verifies that Merge handles null collections by returning an empty dictionary.
    /// </summary>
    [TestMethod]
    public void MergePatterns_WithNullCollections_ReturnsEmptyDictionary() {
        // Arrange
        var options = new FileQueryOptions(new FileQueryOptionsConfig(PatternInput: new()));

        // Act
        var result = PatternsMerger.Merge(options.PatternInput);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    /// <summary>
    /// Verifies that Merge does not mutate the original options.
    /// </summary>
    [TestMethod]
    public void MergePatterns_DoesNotMutateOriginalOptions() {
        // Arrange
        var originalList = new List<string> { "original" };
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    TypedPatterns: new Dictionary<PatternKind, IEnumerable<string>> {
                        [PatternKind.Glob] = originalList
                    }
                )
            )
        );

        // Act
        var result = PatternsMerger.Merge(options.PatternInput);
        //Assert.Throws
        result[PatternKind.Glob].Add("new_item");

        // Assert
        Assert.HasCount(1, originalList, "The original list in options should not have been modified.");
        Assert.HasCount(1, result[PatternKind.Glob], "Want the merged result to be immutable and not reflect changes to the original or the merged list.");
    }
}
