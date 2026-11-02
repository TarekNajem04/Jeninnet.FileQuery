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
        optionsWithIgnore.ErrorRecovery.Action.Should().Be(FileQueryErrorAction.Skip);

        // When ignoreInaccessible is false
        var optionsWithoutIgnore = new FileQueryOptions(new FileQueryOptionsConfig(PatternInput: new PatternInput(["*"]), IgnoreInaccessible: false));
        optionsWithoutIgnore.ErrorRecovery.Action.Should().Be(FileQueryErrorAction.Abort);
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

        diagnostic.Path.Should().Be("/root/a.txt");
        diagnostic.RelativePath.Should().Be("a.txt");
        diagnostic.EntryKind.Should().Be("File");
        diagnostic.Outcome.Should().Be("Include");
        diagnostic.Reason.Should().Be("Matched pattern");
        diagnostic.PatternKind.Should().Be(PatternKind.GitIgnore);
        diagnostic.Pattern.Should().Be("**/*.txt");
        diagnostic.PatternIndex.Should().Be(5);
    }

    /// <summary>
    /// Verifies that FileQueryProgress stores all properties correctly upon construction.
    /// </summary>
    [TestMethod]
    public void Should_StorePropertiesCorrectly_When_ProgressCreated() {
        var progress = new FileQueryProgress(1, 2, 3, "/root/a.txt");

        progress.DirectoriesVisited.Should().Be(1);
        progress.EntriesScanned.Should().Be(2);
        progress.FilesMatched.Should().Be(3);
        progress.CurrentPath.Should().Be("/root/a.txt");
    }
}
