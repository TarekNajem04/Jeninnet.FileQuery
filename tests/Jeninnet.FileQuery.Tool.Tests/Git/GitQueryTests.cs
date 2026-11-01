//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tool.Tests.Git;

/// <summary>
/// Tests diff-aware queries end to end against disposable Git repositories.
/// </summary>
[TestClass]
public sealed class GitQueryTests {
    /// <summary>
    /// Verifies that only changed files are returned and deletions are excluded.
    /// </summary>
    [TestMethod]
    public void Run_ShouldReturnOnlyChangedFiles_WhenChangedSinceIsUsed() {
        using var repository = new TemporaryGitRepository();
        repository.Write("a.txt", "one");
        repository.Write("b.txt", "one");
        repository.Commit("initial");

        repository.Write("a.txt", "two");
        repository.Delete("b.txt");
        repository.Write("c.txt", "one");

        var result = Run("--root", repository.Root, "--changed-since", "HEAD", "--relative");

        result.ExitCode.Should().Be(0);
        SplitLines(result.Output).Should().BeEquivalentTo(["a.txt", "c.txt"]);
    }

    /// <summary>
    /// Verifies that changes are further filtered by patterns.
    /// </summary>
    [TestMethod]
    public void Run_ShouldCombineChangeSetWithPatterns() {
        using var repository = new TemporaryGitRepository();
        repository.Write("a.txt", "one");
        repository.Write("b.cs", "one");
        repository.Commit("initial");

        repository.Write("a.txt", "two");
        repository.Write("b.cs", "two");

        var result = Run("--root", repository.Root, "--changed-since", "HEAD", "--relative", "**;!*.cs");

        result.ExitCode.Should().Be(0);
        SplitLines(result.Output).Should().BeEquivalentTo(["b.cs"]);
    }

    /// <summary>
    /// Verifies that a subdirectory root receives rebased change paths.
    /// </summary>
    [TestMethod]
    public void Run_ShouldRebaseChanges_WhenRootIsSubdirectory() {
        using var repository = new TemporaryGitRepository();
        repository.Write("sub/a.txt", "one");
        repository.Write("root.txt", "one");
        repository.Commit("initial");

        repository.Write("sub/a.txt", "two");
        repository.Write("root.txt", "two");

        var result = Run(
            "--root",
            Path.Combine(repository.Root, "sub"),
            "--changed-since",
            "HEAD",
            "--relative");

        result.ExitCode.Should().Be(0);
        SplitLines(result.Output).Should().BeEquivalentTo(["a.txt"]);
    }

    /// <summary>
    /// Verifies that untracked files can be excluded.
    /// </summary>
    [TestMethod]
    public void Run_ShouldExcludeUntrackedFiles_WhenRequested() {
        using var repository = new TemporaryGitRepository();
        repository.Write("a.txt", "one");
        repository.Commit("initial");
        repository.Write("new.txt", "one");

        var result = Run(
            "--root",
            repository.Root,
            "--changed-since",
            "HEAD",
            "--no-untracked");

        result.ExitCode.Should().Be(0);
        result.Output.Should().Be(string.Empty);
    }

    /// <summary>
    /// Verifies that the two baselines are mutually exclusive.
    /// </summary>
    [TestMethod]
    public void Run_ShouldReturnUsageError_WhenBothBaselinesAreSupplied() {
        using var repository = new TemporaryGitRepository();
        repository.Write("a.txt", "one");
        repository.Commit("initial");

        var result = Run(
            "--root",
            repository.Root,
            "--changed-since",
            "HEAD",
            "--changed-since-date",
            "2026-01-01T00:00:00Z");

        result.ExitCode.Should().Be(2);
        result.Error.Should().Contain("--changed-since");
    }

    /// <summary>
    /// Verifies that an invalid timestamp is a usage error.
    /// </summary>
    [TestMethod]
    public void Run_ShouldReturnUsageError_WhenTimestampIsInvalid() {
        using var repository = new TemporaryGitRepository();
        repository.Write("a.txt", "one");
        repository.Commit("initial");

        var result = Run(
            "--root",
            repository.Root,
            "--changed-since-date",
            "not-a-date");

        result.ExitCode.Should().Be(2);
        result.Error.Should().Contain("--changed-since-date");
    }

    /// <summary>
    /// Verifies that an unknown revision is an execution failure.
    /// </summary>
    [TestMethod]
    public void Run_ShouldReturnFailure_WhenRevisionIsUnknown() {
        using var repository = new TemporaryGitRepository();
        repository.Write("a.txt", "one");
        repository.Commit("initial");

        var result = Run("--root", repository.Root, "--changed-since", "does-not-exist");

        result.ExitCode.Should().Be(1);
        result.Error.Should().Contain("Unknown Git revision");
    }

    /// <summary>
    /// Verifies that a non-repository root is an execution failure.
    /// </summary>
    [TestMethod]
    public void Run_ShouldReturnFailure_WhenDirectoryIsNotRepository() {
        using var environment = new TestEnvironment();

        var result = Run("--root", environment.Root, "--changed-since", "HEAD");

        result.ExitCode.Should().Be(1);
        result.Error.Should().Contain("Not a Git repository");
    }

    /// <summary>
    /// Runs the application with captured output streams.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>The exit code and captured streams.</returns>
    private static CliResult Run(params string[] args) {
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = CliApplication.Run(args, output, error);

        return new CliResult(exitCode, output.ToString(), error.ToString());
    }

    /// <summary>
    /// Splits captured output into trimmed lines.
    /// </summary>
    /// <param name="output">The captured standard output.</param>
    /// <returns>The individual lines.</returns>
    private static List<string> SplitLines(string output) =>
        [.. output.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(static line => line.TrimEnd('\r'))];

    /// <summary>
    /// Captures the result of a command-line invocation.
    /// </summary>
    /// <param name="ExitCode">The process exit code.</param>
    /// <param name="Output">The captured standard output.</param>
    /// <param name="Error">The captured standard error.</param>
    private sealed record CliResult(int ExitCode, string Output, string Error);
}
