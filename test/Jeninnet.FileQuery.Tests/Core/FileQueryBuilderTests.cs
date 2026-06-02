namespace Jeninnet.FileQuery.Tests.Core;

[TestClass]
public class FileQueryBuilderTests {
    private string _tempRoot = string.Empty;

    [TestInitialize]
    public void Init() {
        _tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempRoot);
    }

    [TestCleanup]
    public void Cleanup() {
        if(Directory.Exists(_tempRoot)) {
            Directory.Delete(_tempRoot, true);
        }
    }

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
        CollectionAssert.AreEquivalent(patterns, query.Options.PatternInput.TypedPatterns[PatternKind.Glob].ToList());
    }

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

    [TestMethod]
#pragma warning disable S2699
    public void ValidatePatternType_ShouldHandleAllPatternKinds_WhenHybrid() {
        // Arrange
        var builder = FileQuery.From(_tempRoot);
        builder.UsingHybrid();

        // Act & Assert
        builder.Where(PatternKind.Glob, new List<string> { "*" });
        builder.Where(PatternKind.Regex, new List<string> { ".*" });
        builder.Where(PatternKind.GitIgnore, new List<string> { "node_modules/" });
    }
#pragma warning restore S2699

    [TestMethod]
    public void Build_ShouldThrow_WhenRootPathIsNullOrEmpty() {
        var builder = new FileQueryBuilder("   ", FileSystem.Instance);
        Assert.Throws<InvalidOperationException>(builder.Build);
    }

    [TestMethod]
    public void Build_ShouldThrow_WhenRootPathDoesNotExist() {
        var builder = FileQuery.From(Path.Combine(_tempRoot, "does-not-exist"));
        Assert.Throws<DirectoryNotFoundException>(builder.Build);
    }
}
