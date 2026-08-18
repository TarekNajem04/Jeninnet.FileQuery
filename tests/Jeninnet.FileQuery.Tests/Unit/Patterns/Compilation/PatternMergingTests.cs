namespace Jeninnet.FileQuery.Tests.Unit.Patterns.Compilation;

/// <summary>
/// Tests for PatternMergingTests.
/// </summary>
[TestClass]
public class PatternMergingTests {
    /// <summary>
    /// Verifies that Should ClassifyCorrectly When DebugOutputChecked.
    /// </summary>
    [TestMethod]
    public void Should_ClassifyCorrectly_When_DebugOutputChecked() {
        var result = PatternClassifier.Classify(@"src\**\*.cs");

        Assert.AreEqual(PatternKind.Glob, result);
    }

    /// <summary>
    /// Verifies that Should ClassifyAndGroupCorrectly When OnlyRawPatternsMerged.
    /// </summary>
    [TestMethod]
    public void Should_ClassifyAndGroupCorrectly_When_OnlyRawPatternsMerged() {
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
    /// Verifies that Should PreserveStructure When OnlyTypedPatternsMerged.
    /// </summary>
    [TestMethod]
    public void Should_PreserveStructure_When_OnlyTypedPatternsMerged() {
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
    /// Verifies that Should MergeListsSafely When OverlappingTypesMerged.
    /// </summary>
    [TestMethod]
    public void Should_MergeListsSafely_When_OverlappingTypesMerged() {
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
    /// Verifies that Should ReturnEmptyDictionary When NullCollectionsMerged.
    /// </summary>
    [TestMethod]
    public void Should_ReturnEmptyDictionary_When_NullCollectionsMerged() {
        // Arrange
        var options = new FileQueryOptions(new FileQueryOptionsConfig(PatternInput: new()));

        // Act
        var result = PatternsMerger.Merge(options.PatternInput);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    /// <summary>
    /// Verifies that Should NotMutateOriginalOptions When PatternsMerged.
    /// </summary>
    [TestMethod]
    public void Should_NotMutateOriginalOptions_When_PatternsMerged() {
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
