//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tool.Tests;

/// <summary>
/// Tests the command-line surface: help, version, validation, and exit codes.
/// </summary>
[TestClass]
public sealed class CliApplicationTests {
    /// <summary>
    /// Verifies that help lists the supported options.
    /// </summary>
    [TestMethod]
    public void Run_ShouldShowHelp_WhenHelpIsRequested() {
        var result = Run("--help");

        result.ExitCode.Should().Be(0);
        result.Output.Should().Contain("--dialect");
        result.Output.Should().Contain("--null");
        result.Output.Should().Contain("--count");
    }

    /// <summary>
    /// Verifies that version output is produced.
    /// </summary>
    [TestMethod]
    public void Run_ShouldShowVersion_WhenVersionIsRequested() {
        var result = Run("--version");

        result.ExitCode.Should().Be(0);
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.Output));
    }

    /// <summary>
    /// Verifies that an unknown long option is a usage error.
    /// </summary>
    [TestMethod]
    public void Run_ShouldReturnUsageError_WhenOptionIsUnknown() {
        var result = Run("--nope");

        result.ExitCode.Should().Be(2);
        result.Error.Should().Contain("--nope");
    }

    /// <summary>
    /// Verifies that an unknown short option is a usage error.
    /// </summary>
    [TestMethod]
    public void Run_ShouldReturnUsageError_WhenShortOptionIsUnknown() {
        var result = Run("-x");

        result.ExitCode.Should().Be(2);
        result.Error.Should().Contain("-x");
    }

    /// <summary>
    /// Verifies that patterns that start with a dash are accepted after the end-of-options marker.
    /// </summary>
    [TestMethod]
    public void Run_ShouldAllowDashPatterns_AfterDoubleDash() {
        using var environment = CreateEnvironment();

        var result = Run("--root", environment.Root, "--count", "--", "--nope");

        result.ExitCode.Should().Be(0);
        result.Output.Trim().Should().Be("5");
    }

    /// <summary>
    /// Verifies that an unknown dialect is a usage error.
    /// </summary>
    [TestMethod]
    public void Run_ShouldReturnUsageError_WhenDialectIsInvalid() {
        using var environment = CreateEnvironment();

        var result = Run("--root", environment.Root, "--dialect", "bogus");

        result.ExitCode.Should().Be(2);
        result.Error.Should().Contain("--dialect");
    }

    /// <summary>
    /// Verifies that an unknown symbolic link policy is a usage error.
    /// </summary>
    [TestMethod]
    public void Run_ShouldReturnUsageError_WhenSymlinkPolicyIsInvalid() {
        using var environment = CreateEnvironment();

        var result = Run("--root", environment.Root, "--symlinks", "bogus");

        result.ExitCode.Should().Be(2);
        result.Error.Should().Contain("--symlinks");
    }

    /// <summary>
    /// Verifies that a negative depth is a usage error.
    /// </summary>
    [TestMethod]
    public void Run_ShouldReturnUsageError_WhenDepthIsNegative() {
        using var environment = CreateEnvironment();

        var result = Run("--root", environment.Root, "--depth=-1");

        result.ExitCode.Should().Be(2);
        result.Error.Should().Contain("--depth");
    }

    /// <summary>
    /// Verifies that recursion cannot be configured twice.
    /// </summary>
    [TestMethod]
    public void Run_ShouldReturnUsageError_WhenNoRecursiveAndDepthAreCombined() {
        using var environment = CreateEnvironment();

        var result = Run("--root", environment.Root, "--no-recursive", "--depth", "1");

        result.ExitCode.Should().Be(2);
        result.Error.Should().Contain("--no-recursive");
    }

    /// <summary>
    /// Verifies that a forced dialect cannot be combined with a typed pattern option.
    /// </summary>
    [TestMethod]
    public void Run_ShouldReturnUsageError_WhenForcedDialectUsesTypedOption() {
        using var environment = CreateEnvironment();

        var result = Run("--root", environment.Root, "--dialect", "glob", "--gitignore", "!*.cs");

        result.ExitCode.Should().Be(2);
        result.Error.Should().Contain("--dialect");
    }

    /// <summary>
    /// Verifies that a missing root directory is an execution failure.
    /// </summary>
    [TestMethod]
    public void Run_ShouldReturnFailure_WhenRootDoesNotExist() {
        using var environment = CreateEnvironment();
        var missing = Path.Combine(environment.Root, "missing");

        var result = Run("--root", missing);

        result.ExitCode.Should().Be(1);
        result.Error.Should().Contain("does not exist");
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
    /// Creates a small file tree used by validation tests.
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
