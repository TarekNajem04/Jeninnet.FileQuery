//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
using System.Security;

namespace Jeninnet.FileQuery.Build.Tests;

/// <summary>
/// Tests the MSBuild validation task and its SARIF output.
/// </summary>
[TestClass]
public sealed class ValidateFileQueryPatternsTaskTests {
    /// <summary>
    /// Valid patterns do not fail the build and produce an empty SARIF run.
    /// </summary>
    [TestMethod]
    public void Execute_ValidPatterns_ReturnsTrueAndWritesEmptySarif() {
        using var environment = new TestEnvironment();
        var sarifPath = environment.Abs("filequery.sarif");
        var engine = new MockBuildEngine();
        var task = new ValidateFileQueryPatternsTask {
            BuildEngine = engine,
            Patterns = [new TaskItem("**/*.cs"), new TaskItem("!bin/")],
            SarifOutput = sarifPath
        };

        task.Execute().Should().BeTrue();
        engine.Errors.Should().BeEmpty();

        using var document = JsonDocument.Parse(File.ReadAllText(sarifPath));
        var results = document.RootElement.GetProperty("runs")[0].GetProperty("results");

        results.ValueKind.Should().Be(JsonValueKind.Array);
        results.GetArrayLength().Should().Be(0);
    }

    /// <summary>
    /// Invalid patterns fail with JNQ2001 and include source locations in SARIF.
    /// </summary>
    [TestMethod]
    public void Execute_InvalidPattern_ReturnsFalseAndWritesSarifLocation() {
        using var environment = new TestEnvironment();
        var sarifPath = environment.Abs("filequery.sarif");
        var engine = new MockBuildEngine();
        var item = new TaskItem("src/[abc", new Dictionary<string, string> {
            ["SourceFile"] = "build/patterns.props",
            ["SourceLine"] = "12",
            ["SourceColumn"] = "3"
        });
        var task = new ValidateFileQueryPatternsTask {
            BuildEngine = engine,
            Patterns = [item],
            SarifOutput = sarifPath
        };

        task.Execute().Should().BeFalse();
        engine.Errors.Should().HaveCount(1);
        engine.Errors[0].Code.Should().Be("JNQ2001");
        engine.Errors[0].File.Should().Be("build/patterns.props");
        engine.Errors[0].LineNumber.Should().Be(12);
        engine.Errors[0].ColumnNumber.Should().Be(3);

        using var document = JsonDocument.Parse(File.ReadAllText(sarifPath));
        var result = document.RootElement.GetProperty("runs")[0].GetProperty("results")[0];
        var location = result.GetProperty("locations")[0].GetProperty("physicalLocation");
        var region = location.GetProperty("region");

        result.GetProperty("ruleId").GetString().Should().Be("JNQ2001");
        location.GetProperty("artifactLocation").GetProperty("uri").GetString().Should().Be("build/patterns.props");
        region.GetProperty("startLine").GetInt32().Should().Be(12);
        region.GetProperty("startColumn").GetInt32().Should().Be(3);
    }

    /// <summary>
    /// Empty pattern sets are valid and produce no diagnostics.
    /// </summary>
    [TestMethod]
    public void Execute_EmptyPatterns_ReturnsTrue() {
        var engine = new MockBuildEngine();
        var task = new ValidateFileQueryPatternsTask {
            BuildEngine = engine,
            Patterns = []
        };

        task.Execute().Should().BeTrue();
        engine.Errors.Should().BeEmpty();
    }

    /// <summary>
    /// A project supplied property is split into items and validated by the target.
    /// </summary>
    [TestMethod]
    public void Target_ValidatesPropertyPatterns() {
        var (output, sarif) = RunProject("**/*.cs;[abc", expectSuccess: false);

        output.Should().Contain("JNQ2001");
        sarif.Should().NotBeNull();
    }

    /// <summary>
    /// A valid property passes the target and still writes SARIF.
    /// </summary>
    [TestMethod]
    public void Target_AcceptsValidPropertyPatterns() {
        var (_, sarif) = RunProject("**/*.cs;!bin/", expectSuccess: true);

        sarif.Should().NotBeNull();
    }

    private static (string Output, string? Sarif) RunProject(string patterns, bool expectSuccess) {
        var root = Path.Combine(Path.GetTempPath(), "jeninnet-msbuild-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try {
            var sarifPath = Path.Combine(root, "filequery.sarif");
            var projectPath = Path.Combine(root, "validate.proj");
            var taskPath = SecurityElement.Escape(typeof(ValidateFileQueryPatternsTask).Assembly.Location);
            var escapedSarif = SecurityElement.Escape(sarifPath);
            var escapedPatterns = SecurityElement.Escape(patterns);

            File.WriteAllText(projectPath, $"""
                <Project>
                  <PropertyGroup>
                    <FileQueryPatterns>{escapedPatterns}</FileQueryPatterns>
                    <SarifOutput>{escapedSarif}</SarifOutput>
                  </PropertyGroup>
                  <UsingTask TaskName="Jeninnet.FileQuery.Build.Tasks.ValidateFileQueryPatternsTask"
                             AssemblyFile="{taskPath}" />
                  <Target Name="Validate">
                    <ItemGroup>
                      <FileQueryPattern Include="$(FileQueryPatterns)" />
                    </ItemGroup>
                    <ValidateFileQueryPatternsTask Patterns="@(FileQueryPattern)" SarifOutput="$(SarifOutput)" />
                  </Target>
                </Project>
                """);

            using var process = Process.Start(new ProcessStartInfo("dotnet") {
                WorkingDirectory = root,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                ArgumentList = {
                    "msbuild",
                    projectPath,
                    "-target:Validate",
                    "-nologo",
                    "-verbosity:minimal"
                }
            })!;

            var output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
            process.WaitForExit();

            if(expectSuccess) {
                process.ExitCode.Should().Be(0, because: output);
            } else {
                process.ExitCode.Should().NotBe(0, because: output);
            }

            var sarif = File.Exists(sarifPath) ? File.ReadAllText(sarifPath) : null;
            return (output, sarif);
        }
        finally {
            if(Directory.Exists(root)) {
                Directory.Delete(root, recursive: true);
            }
        }
    }
}
