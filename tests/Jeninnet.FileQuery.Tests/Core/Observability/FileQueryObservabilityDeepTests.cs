//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Core.Observability;

/// <summary>
/// Provides deep observation and validation tests for <see cref="FileQuery"/> operations.
/// </summary>
[TestClass]
public sealed class FileQueryObservabilityDeepTests {
    /// <summary>
    /// Verifies that <see cref="FileQueryErrorRecoveryOptions"/> validation throws when retry attempts are negative.
    /// </summary>
    [TestMethod]
    public void FileQueryErrorRecoveryOptions_Validation_ShouldThrowWhenNegativeRetryAttempts() =>
        TestAssertEx.Throws<ArgumentOutOfRangeException>(
            static () => {
                var options = new FileQueryErrorRecoveryOptions(FileQueryErrorAction.Retry, -1);
                options.Validate();
            }
        );

    /// <summary>
    /// Verifies that <see cref="FileQueryErrorRecoveryOptions"/> validation succeeds for non-negative retry attempts.
    /// </summary>
    [TestMethod]
    public void FileQueryErrorRecoveryOptions_Validation_ShouldSucceedWhenZeroOrPositiveRetryAttempts() {
        new FileQueryErrorRecoveryOptions(FileQueryErrorAction.Retry, 0).Validate();
        new FileQueryErrorRecoveryOptions(FileQueryErrorAction.Retry, 5).Validate();
    }

    /// <summary>
    /// Verifies that <see cref="FileQueryOptions"/> validation propagates error recovery validation.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="FileQueryOptions"/> constructor sets sensible defaults for error recovery.
    /// </summary>
    [TestMethod]
    public void FileQueryOptions_Constructor_ShouldSetSensibleDefaultsForErrorRecovery() {
        // When ignoreInaccessible is true (default)
        var optionsWithIgnore = new FileQueryOptions(new FileQueryOptionsConfig(PatternInput: new PatternInput(["*"]), IgnoreInaccessible: true));
        Assert.AreEqual(FileQueryErrorAction.Skip, optionsWithIgnore.ErrorRecovery.Action);

        // When ignoreInaccessible is false
        var optionsWithoutIgnore = new FileQueryOptions(new FileQueryOptionsConfig(PatternInput: new PatternInput(["*"]), IgnoreInaccessible: false));
        Assert.AreEqual(FileQueryErrorAction.Abort, optionsWithoutIgnore.ErrorRecovery.Action);
    }

    /// <summary>
    /// Verifies that <see cref="FileQueryDiagnostic"/> stores properties correctly.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="FileQueryProgress"/> stores properties correctly.
    /// </summary>
    [TestMethod]
    public void FileQueryProgress_ShouldStorePropertiesCorrectly() {
        var progress = new FileQueryProgress(1, 2, 3, "/root/a.txt");

        Assert.AreEqual(1, progress.DirectoriesVisited);
        Assert.AreEqual(2, progress.EntriesScanned);
        Assert.AreEqual(3, progress.FilesMatched);
        Assert.AreEqual("/root/a.txt", progress.CurrentPath);
    }
}
