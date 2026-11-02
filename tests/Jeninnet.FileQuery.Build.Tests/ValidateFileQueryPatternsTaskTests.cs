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

        Assert.IsTrue(task.Execute());
        Assert.IsEmpty(engine.Errors);

        using var document = JsonDocument.Parse(File.ReadAllText(sarifPath));
        var results = document.RootElement.GetProperty("runs")[0].GetProperty("results");

        Assert.AreEqual(JsonValueKind.Array, results.ValueKind);
        Assert.AreEqual(0, results.GetArrayLength());
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

        Assert.IsFalse(task.Execute());
        Assert.HasCount(1, engine.Errors);
        Assert.AreEqual("JNQ2001", engine.Errors[0].Code);
        Assert.AreEqual("build/patterns.props", engine.Errors[0].File);
        Assert.AreEqual(12, engine.Errors[0].LineNumber);
        Assert.AreEqual(3, engine.Errors[0].ColumnNumber);

        using var document = JsonDocument.Parse(File.ReadAllText(sarifPath));
        var result = document.RootElement.GetProperty("runs")[0].GetProperty("results")[0];
        var location = result.GetProperty("locations")[0].GetProperty("physicalLocation");
        var region = location.GetProperty("region");

        Assert.AreEqual("JNQ2001", result.GetProperty("ruleId").GetString());
        Assert.AreEqual("build/patterns.props", location.GetProperty("artifactLocation").GetProperty("uri").GetString());
        Assert.AreEqual(12, region.GetProperty("startLine").GetInt32());
        Assert.AreEqual(3, region.GetProperty("startColumn").GetInt32());
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

        Assert.IsTrue(task.Execute());
        Assert.IsEmpty(engine.Errors);
    }

    /// <summary>
    /// A project supplied property is split into items and validated by the target.
    /// </summary>
    [TestMethod]
    public void Target_ValidatesPropertyPatterns() {
        var (output, sarif) = RunProject("**/*.cs;[abc", expectSuccess: false);

        Assert.Contains("JNQ2001", output);
        Assert.IsNotNull(sarif);
    }

    /// <summary>
    /// A valid property passes the target and still writes SARIF.
    /// </summary>
    [TestMethod]
    public void Target_AcceptsValidPropertyPatterns() {
        var (_, sarif) = RunProject("**/*.cs;!bin/", expectSuccess: true);

        Assert.IsNotNull(sarif);
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
                Assert.AreEqual(0, process.ExitCode, output);
            } else {
                Assert.AreNotEqual(0, process.ExitCode, output);
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
