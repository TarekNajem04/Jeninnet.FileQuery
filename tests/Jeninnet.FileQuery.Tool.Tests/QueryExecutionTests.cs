//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tool.Tests;

/// <summary>
/// Tests query construction and output across the supported pattern dialects and traversal options.
/// </summary>
[TestClass]
public sealed class QueryExecutionTests {
    /// <summary>
    /// Verifies that every file is returned when no pattern is supplied.
    /// </summary>
    [TestMethod]
    public void Run_ShouldReturnAllFiles_WhenNoPatternIsSupplied() {
        using var environment = CreateEnvironment();

        var result = Run("--root", environment.Root);

        result.ExitCode.Should().Be(0);
        AssertPaths(result.Output, environment,
            "a.txt",
            "b.log",
            "src/c.cs",
            "src/nested/d.cs",
            "bin/e.txt");
    }

    /// <summary>
    /// Verifies the positional pattern argument.
    /// </summary>
    [TestMethod]
    public void Run_ShouldApplyPositionalPatterns() {
        using var environment = CreateEnvironment();

        var result = Run("--root", environment.Root, "**;!*.cs");

        result.ExitCode.Should().Be(0);
        AssertPaths(result.Output, environment, "src/c.cs", "src/nested/d.cs");
    }

    /// <summary>
    /// Verifies the inherited <c>--patterns</c> option.
    /// </summary>
    [TestMethod]
    public void Run_ShouldApplyPatternsOption() {
        using var environment = CreateEnvironment();

        var result = Run("--root", environment.Root, "--patterns", "**;!*.log");

        result.ExitCode.Should().Be(0);
        AssertPaths(result.Output, environment, "b.log");
    }

    /// <summary>
    /// Verifies explicit Glob patterns.
    /// </summary>
    [TestMethod]
    public void Run_ShouldApplyGlobPatterns() {
        using var environment = CreateEnvironment();

        var result = Run("--root", environment.Root, "--glob", "**/*.cs");

        result.ExitCode.Should().Be(0);
        AssertPaths(result.Output, environment, "src/c.cs", "src/nested/d.cs");
    }

    /// <summary>
    /// Verifies explicit GitIgnore patterns.
    /// </summary>
    [TestMethod]
    public void Run_ShouldApplyGitIgnorePatterns() {
        using var environment = CreateEnvironment();

        var result = Run("--root", environment.Root, "--gitignore", "**;!*.cs");

        result.ExitCode.Should().Be(0);
        AssertPaths(result.Output, environment, "src/c.cs", "src/nested/d.cs");
    }

    /// <summary>
    /// Verifies the regex option without an explicit <c>r:</c> prefix.
    /// </summary>
    [TestMethod]
    public void Run_ShouldApplyRegexPatterns_WithoutDialectPrefix() {
        using var environment = CreateEnvironment();

        var result = Run("--root", environment.Root, "--regex", @"\.log$");

        result.ExitCode.Should().Be(0);
        AssertPaths(result.Output, environment, "b.log");
    }

    /// <summary>
    /// Verifies a forced regex dialect.
    /// </summary>
    [TestMethod]
    public void Run_ShouldApplyForcedRegexDialect() {
        using var environment = CreateEnvironment();

        var result = Run("--root", environment.Root, "--dialect", "regex", @"\.cs$");

        result.ExitCode.Should().Be(0);
        AssertPaths(result.Output, environment, "src/c.cs", "src/nested/d.cs");
    }

    /// <summary>
    /// Verifies that a forced dialect without patterns includes every file.
    /// </summary>
    [TestMethod]
    public void Run_ShouldIncludeEveryFile_WhenForcedDialectHasNoPattern() {
        using var environment = CreateEnvironment();

        var result = Run("--root", environment.Root, "--dialect", "glob");

        result.ExitCode.Should().Be(0);
        AssertPaths(result.Output, environment,
            "a.txt",
            "b.log",
            "src/c.cs",
            "src/nested/d.cs",
            "bin/e.txt");
    }

    /// <summary>
    /// Verifies that recursion can be disabled.
    /// </summary>
    [TestMethod]
    public void Run_ShouldRespectNoRecursive() {
        using var environment = CreateEnvironment();

        var result = Run("--root", environment.Root, "--no-recursive");

        result.ExitCode.Should().Be(0);
        AssertPaths(result.Output, environment, "a.txt", "b.log");
    }

    /// <summary>
    /// Verifies that depth zero restricts the results to the root directory.
    /// </summary>
    [TestMethod]
    public void Run_ShouldRespectDepthZero() {
        using var environment = CreateEnvironment();

        var result = Run("--root", environment.Root, "--depth", "0");

        result.ExitCode.Should().Be(0);
        AssertPaths(result.Output, environment, "a.txt", "b.log");
    }

    /// <summary>
    /// Verifies that depth one includes the first directory level.
    /// </summary>
    [TestMethod]
    public void Run_ShouldRespectDepthOne() {
        using var environment = CreateEnvironment();

        var result = Run("--root", environment.Root, "--depth", "1");

        result.ExitCode.Should().Be(0);
        AssertPaths(result.Output, environment, "a.txt", "b.log", "src/c.cs", "bin/e.txt");
    }

    /// <summary>
    /// Verifies relative path output.
    /// </summary>
    [TestMethod]
    public void Run_ShouldWriteRelativePaths_WhenRequested() {
        using var environment = CreateEnvironment();

        var result = Run("--root", environment.Root, "--relative", "--no-recursive");

        result.ExitCode.Should().Be(0);
        SplitLines(result.Output).Should().BeEquivalentTo("a.txt", "b.log");
    }

    /// <summary>
    /// Verifies case-insensitive matching.
    /// </summary>
    [TestMethod]
    public void Run_ShouldIgnoreCase_WhenRequested() {
        using var environment = CreateEnvironment();

        var result = Run("--root", environment.Root, "--ignore-case", "--gitignore", "**;!*.TXT");

        result.ExitCode.Should().Be(0);
        AssertPaths(result.Output, environment, "a.txt", "bin/e.txt");
    }

    /// <summary>
    /// Verifies NUL-separated output for pipeline consumers.
    /// </summary>
    [TestMethod]
    public void Run_ShouldSeparatePathsWithNul_WhenRequested() {
        using var environment = CreateEnvironment();

        var result = Run("--root", environment.Root, "--null", "--no-recursive");

        result.ExitCode.Should().Be(0);
        result.Output.Contains('\0', StringComparison.Ordinal).Should().BeTrue();
        result.Output.Contains('\n', StringComparison.Ordinal).Should().BeFalse();

        var paths = result.Output.Split('\0', StringSplitOptions.RemoveEmptyEntries);
        paths.Select(static path => Path.GetFullPath(path)).Should().BeEquivalentTo(
            Path.GetFullPath(environment.Abs("a.txt")), Path.GetFullPath(environment.Abs("b.log")));
    }

    /// <summary>
    /// Verifies count-only output.
    /// </summary>
    [TestMethod]
    public void Run_ShouldWriteCount_WhenRequested() {
        using var environment = CreateEnvironment();

        var result = Run("--root", environment.Root, "--count");

        result.ExitCode.Should().Be(0);
        result.Output.Trim().Should().Be("5");
    }

    /// <summary>
    /// Verifies that an empty result set is still a successful run.
    /// </summary>
    [TestMethod]
    public void Run_ShouldReturnSuccess_WhenNothingMatches() {
        using var environment = CreateEnvironment();

        var result = Run("--root", environment.Root, "--glob", "*.nomatch");

        result.ExitCode.Should().Be(0);
        result.Output.Should().Be(string.Empty);
    }

    /// <summary>
    /// Verifies that a malformed regex is reported as an execution failure.
    /// </summary>
    [TestMethod]
    public void Run_ShouldReturnFailure_WhenPatternIsMalformed() {
        using var environment = CreateEnvironment();

        var result = Run("--root", environment.Root, "--regex", "[");

        result.ExitCode.Should().Be(1);
        result.Error.Should().Contain("Invalid regex");
    }

    /// <summary>
    /// Verifies that <c>--ignore-files</c> loads the repository's Git ignore files.
    /// </summary>
    [TestMethod]
    public void Run_ShouldLoadIgnoreFiles_WhenRequested() {
        using var environment = CreateEnvironment();
        environment.CreateFile(".gitignore", "*.log\n");

        var result = Run("--root", environment.Root, "--ignore-files");

        result.ExitCode.Should().Be(0);
        AssertPaths(result.Output, environment, ".gitignore", "a.txt", "src/c.cs", "src/nested/d.cs", "bin/e.txt");
    }

    /// <summary>
    /// An explicit include-all pattern is honored even when ignore files are loaded.
    /// </summary>
    [TestMethod]
    public void Run_ShouldHonorExplicitIncludeAll_WithIgnoreFiles() {
        using var environment = CreateEnvironment();
        environment.CreateFile(".gitignore", "*.log\n");

        var result = Run("--root", environment.Root, "--ignore-files", "--patterns", "!**");

        result.ExitCode.Should().Be(0);
        AssertPaths(
            result.Output,
            environment,
            ".gitignore",
            "a.txt",
            "b.log",
            "src/c.cs",
            "src/nested/d.cs",
            "bin/e.txt");
    }

    /// <summary>
    /// Verifies that <c>--global-ignore</c> requires <c>--ignore-files</c>.
    /// </summary>
    [TestMethod]
    public void Run_ShouldReturnUsageError_WhenGlobalIgnoreIsUsedAlone() {
        using var environment = CreateEnvironment();

        var result = Run("--root", environment.Root, "--global-ignore", "ignores.txt");

        result.ExitCode.Should().Be(CliApplication.EXIT_USAGE);
        result.Error.Should().Contain("--global-ignore requires --ignore-files");
    }

    /// <summary>
    /// Verifies that a forced non-GitIgnore dialect rejects <c>--ignore-files</c> at
    /// parse time instead of failing later when the query is built.
    /// </summary>
    [TestMethod]
    public void Run_ShouldReturnUsageError_WhenIgnoreFilesUsedWithForcedDialect() {
        using var environment = CreateEnvironment();

        var result = Run("--root", environment.Root, "--ignore-files", "--dialect", "glob");

        result.ExitCode.Should().Be(CliApplication.EXIT_USAGE);
        result.Error.Should().Contain("--ignore-files requires GitIgnore semantics");
    }

    /// <summary>
    /// Verifies that JSON output contains the documented metadata fields.
    /// </summary>
    [TestMethod]
    public void Run_ShouldWriteJsonMetadata_WhenRequested() {
        using var environment = CreateEnvironment();

        var result = Run("--root", environment.Root, "--json");

        result.ExitCode.Should().Be(0);

        var lines = SplitLines(result.Output);
        lines.Count.Should().Be(5);

        using var document = JsonDocument.Parse(lines[0]);
        var root = document.RootElement;
        root.GetProperty("path").GetString().Should().Be(environment.Abs("a.txt"));
        root.GetProperty("relativePath").GetString().Should().Be("a.txt");
        root.GetProperty("length").GetInt64().Should().Be(0L);
        string.IsNullOrEmpty(root.GetProperty("createdUtc").GetString()).Should().BeFalse();
        string.IsNullOrEmpty(root.GetProperty("modifiedUtc").GetString()).Should().BeFalse();
        root.GetProperty("attributes").ValueKind.Should().Be(JsonValueKind.Array);
        root.GetProperty("isDirectory").GetBoolean().Should().BeFalse();
    }

    /// <summary>
    /// Verifies that JSON output rejects count and NUL formats.
    /// </summary>
    [TestMethod]
    public void Run_ShouldRejectJsonWithCountOrNull() {
        using var environment = CreateEnvironment();

        var count = Run("--root", environment.Root, "--json", "--count");
        var nul = Run("--root", environment.Root, "--json", "--null");

        count.ExitCode.Should().Be(CliApplication.EXIT_USAGE);
        count.Error.Should().Contain("--json");
        nul.ExitCode.Should().Be(CliApplication.EXIT_USAGE);
        nul.Error.Should().Contain("--json");
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
    /// Asserts that the output contains exactly the expected absolute paths.
    /// </summary>
    /// <param name="output">The captured standard output.</param>
    /// <param name="environment">The test environment that owns the files.</param>
    /// <param name="relativePaths">The expected paths relative to the environment root.</param>
    private static void AssertPaths(string output, TestEnvironment environment, params string[] relativePaths) {
        var expected = relativePaths.Select(path => Path.GetFullPath(environment.Abs(path))).ToList();
        SplitLines(output).Select(static path => Path.GetFullPath(path)).Should().BeEquivalentTo(expected);
    }

    /// <summary>
    /// Splits captured output into trimmed lines.
    /// </summary>
    /// <param name="output">The captured standard output.</param>
    /// <returns>The individual lines.</returns>
    private static List<string> SplitLines(string output) =>
        [.. output.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(static line => line.TrimEnd('\r'))];

    /// <summary>
    /// Creates the file tree used by the execution tests.
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
