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
        query.Options.PatternInput.TypedPatterns.Should().NotBeNull();
        query.Options.PatternInput.TypedPatterns.ContainsKey(PatternKind.Glob).Should().BeTrue();
        query.Options.PatternInput.TypedPatterns[PatternKind.Glob].Should().BeEquivalentTo(patterns);
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
        query.Options.PatternInput.InterpretationMode.Should().Be(PatternInterpretationMode.Hybrid);
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
        query.Options.PatternMatchingMode.Should().Be(PatternMatchingMode.GitIgnore);
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
        query.Options.PatternMatchingMode.Should().Be(PatternMatchingMode.Glob);
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
        query.Options.PatternMatchingMode.Should().Be(PatternMatchingMode.Regex);
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
        query.Options.RecurseSubdirectories.Should().BeFalse();
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
        query.Options.RecurseSubdirectories.Should().BeFalse();
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
        query.Options.CaseSensitivity.Should().Be(CaseSensitivity.Insensitive);
    }

    /// <summary>Tests WithMaxDepth_ShouldUpdateDepth_AndRejectInvalidValues.</summary>
    [TestMethod]
    public void WithMaxDepth_ShouldUpdateDepth_AndRejectInvalidValues() {
        var query = FileQuery.From(_tempRoot)
            .WithMaxDepth(2)
            .Build();

        query.Options.MaxRecursionDepth.Should().Be(2);

        FluentActions.Invoking(() => FileQuery.From(_tempRoot).WithMaxDepth(-2)).Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    /// <summary>Tests WithTraversal_ShouldApplyStrategyAndSymlinkPolicy.</summary>
    [TestMethod]
    public void WithTraversal_ShouldApplyStrategyAndSymlinkPolicy() {
        var traversal = new TraversalOptions(
            Strategy: TraversalStrategy.BreadthFirst,
            SymlinkPolicy: SymlinkPolicy.FollowWithCycleDetection);

        var query = FileQuery.From(_tempRoot)
            .WithTraversal(traversal)
            .Build();

        query.Options.Traversal.Strategy.Should().Be(TraversalStrategy.BreadthFirst);
        query.Options.Traversal.SymlinkPolicy.Should().Be(SymlinkPolicy.FollowWithCycleDetection);
        FluentActions.Invoking(() => FileQuery.From(_tempRoot).WithTraversal(null!)).Should().ThrowExactly<ArgumentNullException>();
    }

    /// <summary>Tests IgnoreInaccessible_ShouldUpdateOptionAndDefaultRecovery_WhenDisabled.</summary>
    [TestMethod]
    public void IgnoreInaccessible_ShouldUpdateOptionAndDefaultRecovery_WhenDisabled() {
        var explicitFalse = FileQuery.From(_tempRoot)
            .IgnoreInaccessible(false)
            .Build();

        explicitFalse.Options.IgnoreInaccessible.Should().BeFalse();
        explicitFalse.Options.ErrorRecovery.Action.Should().Be(FileQueryErrorAction.Abort);

        var defaulted = FileQuery.From(_tempRoot).Build();

        defaulted.Options.IgnoreInaccessible.Should().BeTrue();
        defaulted.Options.ErrorRecovery.Action.Should().Be(FileQueryErrorAction.Skip);
    }

    /// <summary>Tests ValidatePatternType_ShouldThrowInvalidOperationException_WhenModeAndKindConflict_AllCases.</summary>
    [TestMethod]
    public void ValidatePatternType_ShouldThrowInvalidOperationException_WhenModeAndKindConflict_AllCases() {
        // Test GitIgnore mode conflicts
        var builder1 = FileQuery.From(_tempRoot).UsingGitIgnore();
        FluentActions.Invoking(() => builder1.Where(PatternKind.Glob, ["*.txt"])).Should().Throw<InvalidOperationException>();

        var builder2 = FileQuery.From(_tempRoot).UsingGitIgnore();
        FluentActions.Invoking(() => builder2.Where(PatternKind.Regex, [".*"])).Should().Throw<InvalidOperationException>();

        // Test Glob mode conflicts
        var builder3 = FileQuery.From(_tempRoot).UsingGlob();
        FluentActions.Invoking(() => builder3.Where(PatternKind.GitIgnore, ["*.txt"])).Should().Throw<InvalidOperationException>();

        var builder4 = FileQuery.From(_tempRoot).UsingGlob();
        FluentActions.Invoking(() => builder4.Where(PatternKind.Regex, [".*"])).Should().Throw<InvalidOperationException>();

        // Test Regex mode conflicts
        var builder5 = FileQuery.From(_tempRoot).UsingRegex();
        FluentActions.Invoking(() => builder5.Where(PatternKind.GitIgnore, ["*.txt"])).Should().Throw<InvalidOperationException>();

        var builder6 = FileQuery.From(_tempRoot).UsingRegex();
        FluentActions.Invoking(() => builder6.Where(PatternKind.Glob, ["*.txt"])).Should().Throw<InvalidOperationException>();
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
        FluentActions.Invoking(() => builder.Build()).Should().Throw<InvalidOperationException>();
    }

    /// <summary>Tests Build_ShouldThrow_WhenRootPathDoesNotExist.</summary>
    [TestMethod]
    public void Build_ShouldThrow_WhenRootPathDoesNotExist() {
        var builder = FileQuery.From(Path.Combine(_tempRoot, "does-not-exist"));
        FluentActions.Invoking(() => builder.Build()).Should().Throw<DirectoryNotFoundException>();
    }
}
