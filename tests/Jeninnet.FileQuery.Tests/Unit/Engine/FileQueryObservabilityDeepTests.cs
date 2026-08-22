//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Unit.Engine;

/// <summary>
/// In-depth tests for FileQuery observability including error recovery options, diagnostics, and progress reporting.
/// </summary>
[TestClass]
public sealed class FileQueryObservabilityDeepTests {
    /// <summary>
    /// Verifies that error recovery options throw ArgumentOutOfRangeException for negative retry attempts.
    /// </summary>
    [TestMethod]
    public void Should_Throw_When_RetryAttemptsAreNegative() =>
        ((Action)(static () => {
            var options = new FileQueryErrorRecoveryOptions(FileQueryErrorAction.Retry, -1);
            options.Validate();
        }
        )).Should().Throw<ArgumentOutOfRangeException>();

    /// <summary>
    /// Verifies that error recovery options accept zero or positive retry attempts without throwing.
    /// </summary>
    [TestMethod]
    public void Should_Succeed_When_RetryAttemptsAreZeroOrPositive() {
        new FileQueryErrorRecoveryOptions(FileQueryErrorAction.Retry, 0).Validate();
        new FileQueryErrorRecoveryOptions(FileQueryErrorAction.Retry, 5).Validate();
    }

    /// <summary>
    /// Verifies that invalid error recovery options in FileQueryOptions are validated and throw.
    /// </summary>
    [TestMethod]
    public void Should_PropagateErrorRecoveryValidation_When_OptionsInvalid() {
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new PatternInput(["*"]),
                ErrorRecovery: new FileQueryErrorRecoveryOptions(FileQueryErrorAction.Retry, -1)
            )
        );

        ((Action)(() => options.Validate())).Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that error recovery defaults are set based on the IgnoreInaccessible option.
    /// </summary>
    [TestMethod]
    public void Should_SetSensibleDefaults_When_ErrorRecoveryConstructed() {
        // When ignoreInaccessible is true (default)
        var optionsWithIgnore = new FileQueryOptions(new FileQueryOptionsConfig(PatternInput: new PatternInput(["*"]), IgnoreInaccessible: true));
        Assert.AreEqual(FileQueryErrorAction.Skip, optionsWithIgnore.ErrorRecovery.Action);

        // When ignoreInaccessible is false
        var optionsWithoutIgnore = new FileQueryOptions(new FileQueryOptionsConfig(PatternInput: new PatternInput(["*"]), IgnoreInaccessible: false));
        Assert.AreEqual(FileQueryErrorAction.Abort, optionsWithoutIgnore.ErrorRecovery.Action);
    }

    /// <summary>
    /// Verifies that FileQueryDiagnostic stores all properties correctly upon construction.
    /// </summary>
    [TestMethod]
    public void Should_StorePropertiesCorrectly_When_DiagnosticCreated() {
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
    /// Verifies that FileQueryProgress stores all properties correctly upon construction.
    /// </summary>
    [TestMethod]
    public void Should_StorePropertiesCorrectly_When_ProgressCreated() {
        var progress = new FileQueryProgress(1, 2, 3, "/root/a.txt");

        Assert.AreEqual(1, progress.DirectoriesVisited);
        Assert.AreEqual(2, progress.EntriesScanned);
        Assert.AreEqual(3, progress.FilesMatched);
        Assert.AreEqual("/root/a.txt", progress.CurrentPath);
    }
}
