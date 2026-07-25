namespace Jeninnet.FileQuery.Tests.Core.Observability;

[TestClass]
public sealed class FileQueryObservabilityDeepTests {
    [TestMethod]
    public void FileQueryErrorRecoveryOptions_Validation_ShouldThrowWhenNegativeRetryAttempts() =>
        TestAssertEx.Throws<ArgumentOutOfRangeException>(
            () => {
                var options = new FileQueryErrorRecoveryOptions(FileQueryErrorAction.Retry, -1);
                options.Validate();
            }
        );

    [TestMethod]
    public void FileQueryErrorRecoveryOptions_Validation_ShouldSucceedWhenZeroOrPositiveRetryAttempts() {
        new FileQueryErrorRecoveryOptions(FileQueryErrorAction.Retry, 0).Validate();
        new FileQueryErrorRecoveryOptions(FileQueryErrorAction.Retry, 5).Validate();
    }

    [TestMethod]
    public void FileQueryOptions_Validation_ShouldPropagateErrorRecoveryValidation() {
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new PatternInput(["*"]),
                ErrorRecovery: new FileQueryErrorRecoveryOptions(FileQueryErrorAction.Retry, -1)
            )
        );

        TestAssertEx.Throws<ArgumentOutOfRangeException>(options.Validate);
    }

    [TestMethod]
    public void FileQueryOptions_Constructor_ShouldSetSensibleDefaultsForErrorRecovery() {
        // When ignoreInaccessible is true (default)
        var optionsWithIgnore = new FileQueryOptions(new FileQueryOptionsConfig(PatternInput: new PatternInput(["*"]), IgnoreInaccessible: true));
        Assert.AreEqual(FileQueryErrorAction.Skip, optionsWithIgnore.ErrorRecovery.Action);

        // When ignoreInaccessible is false
        var optionsWithoutIgnore = new FileQueryOptions(new FileQueryOptionsConfig(PatternInput: new PatternInput(["*"]), IgnoreInaccessible: false));
        Assert.AreEqual(FileQueryErrorAction.Abort, optionsWithoutIgnore.ErrorRecovery.Action);
    }

    [TestMethod]
    public void FileQueryDiagnostic_ShouldStorePropertiesCorrectly() {
        var diagnostic = new FileQueryDiagnostic(
            Path: "/root/a.txt",
            RelativePath: "a.txt",
            EntryKind: "File",
            Outcome: "Include",
            Reason: "Matched pattern",
            PatternKind: PatternKind.GitIgnore,
            Pattern: "**/*.txt",
            PatternIndex: 5
        );

        Assert.AreEqual("/root/a.txt", diagnostic.Path);
        Assert.AreEqual("a.txt", diagnostic.RelativePath);
        Assert.AreEqual("File", diagnostic.EntryKind);
        Assert.AreEqual("Include", diagnostic.Outcome);
        Assert.AreEqual("Matched pattern", diagnostic.Reason);
        Assert.AreEqual(PatternKind.GitIgnore, diagnostic.PatternKind);
        Assert.AreEqual("**/*.txt", diagnostic.Pattern);
        Assert.AreEqual(5, diagnostic.PatternIndex);
    }

    [TestMethod]
    public void FileQueryProgress_ShouldStorePropertiesCorrectly() {
        var progress = new FileQueryProgress(1, 2, 3, "/root/a.txt");

        Assert.AreEqual(1, progress.DirectoriesVisited);
        Assert.AreEqual(2, progress.EntriesScanned);
        Assert.AreEqual(3, progress.FilesMatched);
        Assert.AreEqual("/root/a.txt", progress.CurrentPath);
    }
}

