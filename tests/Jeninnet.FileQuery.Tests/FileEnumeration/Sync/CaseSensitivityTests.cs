namespace Jeninnet.FileQuery.Tests.FileEnumeration.Sync;

/// <summary>
/// Tests handling of IgnoreCase and OS defaults.
/// </summary>
[TestClass]
public class CaseSensitivityTests {
    /// <summary>
    /// Ensures IgnoreCase=true matches regardless of filename case.
    /// </summary>
    [TestMethod]
    public void Should_MatchFilesRegardlessOfCase_When_IgnoreCaseTrue() {
        using var env = new TestEnvironment();
        env.CreateFiles("FILE.TXT", "file.txt", "FiLe.TxT");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!file.txt"
                    ]
                ),
                CaseSensitivity: Enums.CaseSensitivity.Insensitive
            )
        );

        var result = fileQueryEngine.Execute(new(env.Root, options)).ToList();

        result.Should().HaveCount(RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? 3 : 1);
    }

    /// <summary>
    /// Ensures IgnoreCase=false respects filename case.
    /// </summary>
    [TestMethod]
    public void Should_MatchOnlyExactCase_When_IgnoreCaseFalse() {
        using var env = new TestEnvironment();
        env.CreateFiles("FILE.TXT", "file.txt", "FiLe.TxT");

        var fileQueryEngine = FileQueryRuntime.Create();
        IEnumerable<string> patterns = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
            ? ["**", "!file.txt"]
            : ["**", "!FILE.TXT"]; // Windows macOS are case-insensitive file systems by default, therefore, it does not change the case of the letter
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(Patterns: patterns),
                CaseSensitivity: Enums.CaseSensitivity.Sensitive
            )
        );

        var result = fileQueryEngine.Execute(new(env.Root, options)).ToList();
        var stringComparison = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;

        result.Should().HaveCount(1);
        Assert.IsTrue(result.Single().EndsWith("file.txt", stringComparison));
    }

    /// <summary>
    /// OS-specific behavior: Windows/macOS ? case-insensitive, Linux ? case-sensitive.
    /// </summary>
    [TestMethod]
    public void Should_MatchOSRules_When_DefaultCaseSensitivity() {
        using var env = new TestEnvironment();
        env.CreateFiles("Sample.TXT", "sample.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!sample.txt"
                    ]
                )
            )
        );

        var result = fileQueryEngine.Execute(new(env.Root, options)).ToList();

        if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
            result.Should().ContainSingle(static x => x.EndsWith("sample.txt", StringComparison.Ordinal));
        } else {
            // The Windows operating system is not case-sensitive, so both files are identical, and the second operation is to replace the first file.
            result.Should().ContainSingle(static x => x.EndsWith("sample.txt", StringComparison.OrdinalIgnoreCase));
        }
    }
}

