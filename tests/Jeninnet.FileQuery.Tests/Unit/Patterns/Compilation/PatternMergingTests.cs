//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
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

        result.Should().Be(PatternKind.Glob);
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
        result.ContainsKey(PatternKind.GitIgnore).Should().BeTrue(because: "Should have GitIgnore patterns");
        result[PatternKind.GitIgnore].Should().HaveCount(2);

        // 2. Verify Glob bucket (contains the Windows-style path)
        result.ContainsKey(PatternKind.Glob).Should().BeTrue(because: "Should have Glob patterns");
        result[PatternKind.Glob].Should().HaveCount(1);
        result[PatternKind.Glob][0].Should().Be(@"src\**\*.cs");
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
        result.Should().HaveCount(1);
        result[PatternKind.Regex].Should().HaveCount(2);
        result[PatternKind.Regex][0].Should().Be("LICENSE");
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
        result.Should().HaveCount(1);
        var gitIgnoreList = result[PatternKind.GitIgnore];
        gitIgnoreList.Should().HaveCount(2);

        // Verification of "Last Rule Wins" order: Typed first, then Raw appended
        gitIgnoreList[0].Should().Be(".env");
        gitIgnoreList[1].Should().Be("node_modules/");
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
        result.Should().NotBeNull();
        result.Should().BeEmpty();
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
        // Exception behavior is covered by TestAssertEx.Throws.
        result[PatternKind.Glob].Add("new_item");

        // Assert
        originalList.Should().HaveCount(1, because: "The original list in options should not have been modified.");
        result[PatternKind.Glob].Should().HaveCount(1, because: "Want the merged result to be immutable and not reflect changes to the original or the merged list.");
    }
}

