//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Integration;

/// <summary>
/// Tests for the file query builder configuration and option-setting methods.
/// </summary>
[TestClass]
public sealed class FileQueryBuilderTests : IDisposable {
    private string _tempRoot = string.Empty;
    private TestEnvironment? _env;

    /// <summary>
    /// Verifies that Init.
    /// </summary>
    [TestInitialize]
    public void Init() {
        _env = new TestEnvironment();
        _tempRoot = _env.Root;
    }

    /// <summary>
    /// Verifies that Dispose.
    /// </summary>
    public void Dispose() {
        _env?.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Verifies that the Where method adds patterns for the specified pattern kind.
    /// </summary>
    [TestMethod]
    public void Should_AddPatterns_When_WhereCalledWithPatternKindAndList() {
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

    /// <summary>
    /// Verifies that UsingHybrid sets the interpretation mode to hybrid.
    /// </summary>
    [TestMethod]
    public void Should_SetMatchingModeToHybrid_When_UsingHybridCalled() {
        // Arrange
        var builder = FileQuery.From(_tempRoot);

        // Act
        builder.UsingHybrid();
        var query = builder.Build();

        // Assert
        Assert.AreEqual(PatternInterpretationMode.Hybrid, query.Options.PatternInput.InterpretationMode);
    }

    /// <summary>
    /// Verifies that UsingGitIgnore sets the pattern matching mode to gitignore.
    /// </summary>
    [TestMethod]
    public void Should_SetMatchingModeToGitIgnore_When_UsingGitIgnoreCalled() {
        // Arrange
        var builder = FileQuery.From(_tempRoot);

        // Act
        builder.UsingGitIgnore();
        var query = builder.Build();

        // Assert
        Assert.AreEqual(PatternMatchingMode.GitIgnore, query.Options.PatternMatchingMode);
    }

    /// <summary>
    /// Verifies that UsingGlob sets the pattern matching mode to glob.
    /// </summary>
    [TestMethod]
    public void Should_SetMatchingModeToGlob_When_UsingGlobCalled() {
        // Arrange
        var builder = FileQuery.From(_tempRoot);

        // Act
        builder.UsingGlob();
        var query = builder.Build();

        // Assert
        Assert.AreEqual(PatternMatchingMode.Glob, query.Options.PatternMatchingMode);
    }

    /// <summary>
    /// Verifies that UsingRegex sets the pattern matching mode to regex.
    /// </summary>
    [TestMethod]
    public void Should_SetMatchingModeToRegex_When_UsingRegexCalled() {
        // Arrange
        var builder = FileQuery.From(_tempRoot);

        // Act
        builder.UsingRegex();
        var query = builder.Build();

        // Assert
        Assert.AreEqual(PatternMatchingMode.Regex, query.Options.PatternMatchingMode);
    }

    /// <summary>
    /// Verifies that WithRecursion updates the recurse subdirectories option.
    /// </summary>
    [TestMethod]
    public void Should_UpdateRecurseSubdirectories_When_WithRecursionCalled() {
        // Arrange
        var builder = FileQuery.From(_tempRoot);

        // Act
        builder.WithRecursion(false);
        var query = builder.Build();

        // Assert
        Assert.IsFalse(query.Options.RecurseSubdirectories);
    }

    /// <summary>
    /// Verifies that WithoutRecursion sets recurse subdirectories to false.
    /// </summary>
    [TestMethod]
    public void Should_SetRecurseSubdirectoriesToFalse_When_WithoutRecursionCalled() {
        // Arrange
        var builder = FileQuery.From(_tempRoot);

        // Act
        builder.WithoutRecursion();
        var query = builder.Build();

        // Assert
        Assert.IsFalse(query.Options.RecurseSubdirectories);
    }

    /// <summary>
    /// Verifies that IgnoreCase updates the case sensitivity option to insensitive.
    /// </summary>
    [TestMethod]
    public void Should_UpdateCaseSensitivity_When_IgnoreCaseCalled() {
        // Arrange
        var builder = FileQuery.From(_tempRoot);

        // Act
        builder.IgnoreCase(true);
        var query = builder.Build();

        // Assert
        Assert.AreEqual(CaseSensitivity.Insensitive, query.Options.CaseSensitivity);
    }

    /// <summary>
    /// Verifies that conflicting mode and pattern kind combinations throw an invalid operation exception.
    /// </summary>
    [TestMethod]
    public void Should_ThrowInvalidOperationException_When_ModeAndKindConflict() {
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

    /// <summary>
    /// Verifies that hybrid mode accepts all pattern kinds without throwing.
    /// </summary>
    [TestMethod]
#pragma warning disable S2699
    public void Should_HandleAllPatternKinds_When_HybridMode() {
        // Arrange
        var builder = FileQuery.From(_tempRoot);
        builder.UsingHybrid();

        // Act & Assert
        builder.Where(PatternKind.Glob, ["*"]);
        builder.Where(PatternKind.Regex, [".*"]);
        builder.Where(PatternKind.GitIgnore, ["node_modules/"]);
    }
#pragma warning restore S2699

    /// <summary>
    /// Verifies that building a query with a null or empty root path throws an invalid operation exception.
    /// </summary>
    [TestMethod]
    public void Should_Throw_When_RootPathIsNullOrEmpty() {
        var builder = new FileQueryBuilder("   ", FileSystem.Instance);
        Assert.Throws<InvalidOperationException>(builder.Build);
    }

    /// <summary>
    /// Verifies that building a query with a non-existent root path throws a directory not found exception.
    /// </summary>
    [TestMethod]
    public void Should_Throw_When_RootPathDoesNotExist() {
        var builder = FileQuery.From(Path.Combine(_tempRoot, "does-not-exist"));
        Assert.Throws<DirectoryNotFoundException>(builder.Build);
    }
}
