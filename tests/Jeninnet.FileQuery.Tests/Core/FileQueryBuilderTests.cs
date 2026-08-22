//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Core;

/// <summary>
/// Contains tests for the <see cref="FileQueryBuilder"/> class.
/// </summary>
[TestClass]
public class FileQueryBuilderTests {
    private string _tempRoot = string.Empty;

    /// <summary>
    /// Initializes the test environment.
    /// </summary>
    [TestInitialize]
    public void Init() {
        _tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempRoot);
    }

    /// <summary>
    /// Cleans up the test environment.
    /// </summary>
    [TestCleanup]
    public void Cleanup() {
        if(Directory.Exists(_tempRoot)) {
            Directory.Delete(_tempRoot, true);
        }
    }

    /// <summary>Tests Where_ShouldAddPatterns_WhenPatternKindAndListProvided.</summary>
    [TestMethod]
    public void Where_ShouldAddPatterns_WhenPatternKindAndListProvided() {
        // Arrange
        var builder = FileQuery.From(_tempRoot);
        var patterns = new List<string> { "*.txt", "*.log" };

        // Act
        builder.Where(PatternKind.Glob, patterns);
        var query = builder.Build();

        // Assert
        Assert.IsNotNull(query.Options.PatternInput.TypedPatterns);
        Assert.IsTrue(query.Options.PatternInput.TypedPatterns.ContainsKey(PatternKind.Glob));
        Assert.AreSequenceEqual(patterns, [.. query.Options.PatternInput.TypedPatterns[PatternKind.Glob]], SequenceOrder.InAnyOrder);
    }

    /// <summary>Tests UsingHybrid_ShouldSetMatchingModeToHybrid_WhenCalled.</summary>
    [TestMethod]
    public void UsingHybrid_ShouldSetMatchingModeToHybrid_WhenCalled() {
        // Arrange
        var builder = FileQuery.From(_tempRoot);

        // Act
        builder.UsingHybrid();
        var query = builder.Build();

        // Assert
        Assert.AreEqual(PatternInterpretationMode.Hybrid, query.Options.PatternInput.InterpretationMode);
    }

    /// <summary>Tests UsingGitIgnore_ShouldSetMatchingModeToGitIgnore_WhenCalled.</summary>
    [TestMethod]
    public void UsingGitIgnore_ShouldSetMatchingModeToGitIgnore_WhenCalled() {
        // Arrange
        var builder = FileQuery.From(_tempRoot);

        // Act
        builder.UsingGitIgnore();
        var query = builder.Build();

        // Assert
        Assert.AreEqual(PatternMatchingMode.GitIgnore, query.Options.PatternMatchingMode);
    }

    /// <summary>Tests UsingGlob_ShouldSetMatchingModeToGlob_WhenCalled.</summary>
    [TestMethod]
    public void UsingGlob_ShouldSetMatchingModeToGlob_WhenCalled() {
        // Arrange
        var builder = FileQuery.From(_tempRoot);

        // Act
        builder.UsingGlob();
        var query = builder.Build();

        // Assert
        Assert.AreEqual(PatternMatchingMode.Glob, query.Options.PatternMatchingMode);
    }

    /// <summary>Tests UsingRegex_ShouldSetMatchingModeToRegex_WhenCalled.</summary>
    [TestMethod]
    public void UsingRegex_ShouldSetMatchingModeToRegex_WhenCalled() {
        // Arrange
        var builder = FileQuery.From(_tempRoot);

        // Act
        builder.UsingRegex();
        var query = builder.Build();

        // Assert
        Assert.AreEqual(PatternMatchingMode.Regex, query.Options.PatternMatchingMode);
    }

    /// <summary>Tests WithRecursion_ShouldUpdateRecurseSubdirectories_WhenValueProvided.</summary>
    [TestMethod]
    public void WithRecursion_ShouldUpdateRecurseSubdirectories_WhenValueProvided() {
        // Arrange
        var builder = FileQuery.From(_tempRoot);

        // Act
        builder.WithRecursion(false);
        var query = builder.Build();

        // Assert
        Assert.IsFalse(query.Options.RecurseSubdirectories);
    }

    /// <summary>Tests WithoutRecursion_ShouldSetRecurseSubdirectoriesToFalse_WhenCalled.</summary>
    [TestMethod]
    public void WithoutRecursion_ShouldSetRecurseSubdirectoriesToFalse_WhenCalled() {
        // Arrange
        var builder = FileQuery.From(_tempRoot);

        // Act
        builder.WithoutRecursion();
        var query = builder.Build();

        // Assert
        Assert.IsFalse(query.Options.RecurseSubdirectories);
    }

    /// <summary>Tests IgnoreCase_ShouldUpdateCaseSensitivity_WhenValueProvided.</summary>
    [TestMethod]
    public void IgnoreCase_ShouldUpdateCaseSensitivity_WhenValueProvided() {
        // Arrange
        var builder = FileQuery.From(_tempRoot);

        // Act
        builder.IgnoreCase(true);
        var query = builder.Build();

        // Assert
        Assert.AreEqual(CaseSensitivity.Insensitive, query.Options.CaseSensitivity);
    }

    /// <summary>Tests ValidatePatternType_ShouldThrowInvalidOperationException_WhenModeAndKindConflict_AllCases.</summary>
    [TestMethod]
    public void ValidatePatternType_ShouldThrowInvalidOperationException_WhenModeAndKindConflict_AllCases() {
        // Test GitIgnore mode conflicts
        var builder1 = FileQuery.From(_tempRoot).UsingGitIgnore();
        Assert.Throws<InvalidOperationException>(() => builder1.Where(PatternKind.Glob, ["*.txt"]));

        var builder2 = FileQuery.From(_tempRoot).UsingGitIgnore();
        Assert.Throws<InvalidOperationException>(() => builder2.Where(PatternKind.Regex, [".*"]));

        // Test Glob mode conflicts
        var builder3 = FileQuery.From(_tempRoot).UsingGlob();
        Assert.Throws<InvalidOperationException>(() => builder3.Where(PatternKind.GitIgnore, ["*.txt"]));

        var builder4 = FileQuery.From(_tempRoot).UsingGlob();
        Assert.Throws<InvalidOperationException>(() => builder4.Where(PatternKind.Regex, [".*"]));

        // Test Regex mode conflicts
        var builder5 = FileQuery.From(_tempRoot).UsingRegex();
        Assert.Throws<InvalidOperationException>(() => builder5.Where(PatternKind.GitIgnore, ["*.txt"]));

        var builder6 = FileQuery.From(_tempRoot).UsingRegex();
        Assert.Throws<InvalidOperationException>(() => builder6.Where(PatternKind.Glob, ["*.txt"]));
    }

    /// <summary>Tests ValidatePatternType_ShouldHandleAllPatternKinds_WhenHybrid.</summary>
    [TestMethod]
#pragma warning disable S2699
    public void ValidatePatternType_ShouldHandleAllPatternKinds_WhenHybrid() {
        // Arrange
        var builder = FileQuery.From(_tempRoot);
        builder.UsingHybrid();

        // Act & Assert
        builder.Where(PatternKind.Glob, ["*"]);
        builder.Where(PatternKind.Regex, [".*"]);
        builder.Where(PatternKind.GitIgnore, ["node_modules/"]);
    }
#pragma warning restore S2699

    /// <summary>Tests Build_ShouldThrow_WhenRootPathIsNullOrEmpty.</summary>
    [TestMethod]
    public void Build_ShouldThrow_WhenRootPathIsNullOrEmpty() {
        var builder = new FileQueryBuilder("   ", FileSystem.Instance);
        Assert.Throws<InvalidOperationException>(builder.Build);
    }

    /// <summary>Tests Build_ShouldThrow_WhenRootPathDoesNotExist.</summary>
    [TestMethod]
    public void Build_ShouldThrow_WhenRootPathDoesNotExist() {
        var builder = FileQuery.From(Path.Combine(_tempRoot, "does-not-exist"));
        Assert.Throws<DirectoryNotFoundException>(builder.Build);
    }
}
