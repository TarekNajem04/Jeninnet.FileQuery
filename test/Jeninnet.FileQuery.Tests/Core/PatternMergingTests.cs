namespace Jeninnet.FileQuery.Tests.Core;

[TestClass]
public class PatternMergingTests
{
    [TestMethod]
    public void Debug_Classifier_Output()
    {
        var result = PatternClassifier.Classify(@"src\**\*.cs");

        Assert.AreEqual(PatternKind.Glob, result);
    }

    [TestMethod]
    public void MergePatterns_WithOnlyRawPatterns_ClassifiesAndGroupsCorrectly()
    {
        // Arrange
        var options = new FileQueryOptions(
            patternInput: new(
                patterns: [
                    "*.cs",            // Classified as GitIgnore (Superset rule)
                    "bin/",            // Classified as GitIgnore (Trailing slash)
                    @"src\**\*.cs"     // Classified as Glob (Backslash rule)
                ]
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

    [TestMethod]
    public void MergePatterns_WithOnlyTypedPatterns_PreservesStructure()
    {
        // Arrange
        var options = new FileQueryOptions(
            patternInput: new(
                typedPatterns: PatternHelpers.Create(PatternKind.Regex, "LICENSE", "README")
            )
        );

        // Act
        var result = PatternsMerger.Merge(options.PatternInput);

        // Assert
        Assert.HasCount(1, result);
        Assert.HasCount(2, result[PatternKind.Regex]);
        Assert.AreEqual("LICENSE", result[PatternKind.Regex][0]);
    }

    [TestMethod]
    public void MergePatterns_WithOverlappingTypes_MergesListsSafely()
    {
        // Arrange
        var options = new FileQueryOptions(
            // Raw pattern that will be classified as GitIgnore
            patternInput: new(
                patterns: new List<string> { "node_modules/" },
                // Explicitly typed GitIgnore pattern
                typedPatterns: PatternHelpers.Create(PatternKind.GitIgnore, ".env")
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

    [TestMethod]
    public void MergePatterns_WithNullCollections_ReturnsEmptyDictionary()
    {
        // Arrange
        var options = new FileQueryOptions(patternInput: new());

        // Act
        var result = PatternsMerger.Merge(options.PatternInput);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void MergePatterns_DoesNotMutateOriginalOptions()
    {
        // Arrange
        var originalList = new List<string> { "original" };
        var options = new FileQueryOptions(
            patternInput: new(
                typedPatterns: new Dictionary<PatternKind, IEnumerable<string>>
                {
                    [PatternKind.Glob] = originalList
                }
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
