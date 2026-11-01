//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tool.Tests;

/// <summary>
/// Tests the explain output used by IDE tooling.
/// </summary>
[TestClass]
public sealed class ExplainTests {
    /// <summary>
    /// Verifies that included and excluded files are reported with reasons.
    /// </summary>
    [TestMethod]
    public void Run_ShouldExplainIncludedAndExcludedFiles() {
        using var environment = CreateEnvironment();

        var result = Run("--root", environment.Root, "--gitignore", "**;!*.cs", "--explain");

        result.ExitCode.Should().Be(0);

        var lines = SplitLines(result.Output);
        lines.Should().Contain(line => line.StartsWith("include\tsrc/c.cs\t", StringComparison.Ordinal));
        lines.Should().Contain(line => line.StartsWith("include\tsrc/nested/d.cs\t", StringComparison.Ordinal));
        lines.Should().Contain(line => line.StartsWith("exclude\ta.txt\t", StringComparison.Ordinal));
        lines.Should().Contain(line => line.StartsWith("exclude\tbin/e.txt\t", StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifies that explanations only mention files, never directories.
    /// </summary>
    [TestMethod]
    public void Run_ShouldExplainFilesOnly() {
        using var environment = CreateEnvironment();

        var result = Run("--root", environment.Root, "--explain");

        result.ExitCode.Should().Be(0);
        SplitLines(result.Output).Should().NotContain(line => line.Split('\t')[1].EndsWith('/'));
    }

    /// <summary>
    /// Verifies that explain mode rejects count output.
    /// </summary>
    [TestMethod]
    public void Run_ShouldRejectExplainWithCount() {
        using var environment = CreateEnvironment();

        var result = Run("--root", environment.Root, "--count", "--explain");

        result.ExitCode.Should().Be(2);
        result.Error.Should().Contain("--explain");
    }

    /// <summary>
    /// Verifies that explain mode rejects NUL-separated output.
    /// </summary>
    [TestMethod]
    public void Run_ShouldRejectExplainWithNull() {
        using var environment = CreateEnvironment();

        var result = Run("--root", environment.Root, "--null", "--explain");

        result.ExitCode.Should().Be(2);
        result.Error.Should().Contain("--explain");
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
    /// Splits captured output into lines.
    /// </summary>
    /// <param name="output">The captured standard output.</param>
    /// <returns>The individual lines.</returns>
    private static List<string> SplitLines(string output) =>
        [.. output.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(static line => line.TrimEnd('\r'))];

    /// <summary>
    /// Creates the file tree used by these tests.
    /// </summary>
    /// <returns>The initialized test environment.</returns>
    private static TestEnvironment CreateEnvironment() {
        var environment = new TestEnvironment();
        environment.CreateFiles(
            "a.txt",
            "b.log",
            "src/c.cs",
            "src/nested/d.cs",
            "bin/e.txt");

        return environment;
    }

    /// <summary>
    /// Captures the result of a command-line invocation.
    /// </summary>
    /// <param name="ExitCode">The process exit code.</param>
    /// <param name="Output">The captured standard output.</param>
    /// <param name="Error">The captured standard error.</param>
    private sealed record CliResult(int ExitCode, string Output, string Error);
}
