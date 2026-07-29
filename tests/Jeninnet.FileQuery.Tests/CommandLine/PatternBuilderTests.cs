namespace Jeninnet.FileQuery.Tests.CommandLine;

/// <summary>
/// Contains unit tests for the <see cref="PatternBuilder"/> class, verifying its ability to correctly build and categorize patterns.
/// </summary>
[TestClass]
public sealed class PatternBuilderTests {
    /// <summary>
    /// Verifies that the <see cref="PatternBuilder.Build(string?, string?, string?, string?)"/> method returns a default pattern when no input is provided.
    /// </summary>
    [TestMethod]
    public void Build_ShouldReturnDefaultPattern_WhenInputIsEmpty() {
        var result = PatternBuilder.Build();

        Assert.HasCount(1, result);
        Assert.IsTrue(result.ContainsKey(PatternKind.GitIgnore));
        Assert.AreEqual("!**", result[PatternKind.GitIgnore][0]);
    }

    /// <summary>
    /// Verifies that the <see cref="PatternBuilder.Build(string?, string?, string?, string?)"/> method correctly categorizes input patterns.
    /// </summary>
    [TestMethod]
    public void Build_ShouldCategorizePatternsCorrectly() {
        const string patterns = "r:.*\\.txt;*.log";
        var result = PatternBuilder.Build(patterns);

        Assert.IsTrue(result.ContainsKey(PatternKind.Regex));
        Assert.IsTrue(result.ContainsKey(PatternKind.GitIgnore), $"Result keys: {string.Join(", ", result.Keys)}");
    }

    /// <summary>
    /// Verifies that the <see cref="PatternBuilder.Build(string?, string?, string?, string?)"/> method merges patterns of the same type.
    /// </summary>
    [TestMethod]
    public void Build_ShouldMergeSameTypePatterns() {
        var result = PatternBuilder.Build("a.txt;b.txt");

        Assert.IsTrue(result.ContainsKey(PatternKind.GitIgnore));
        Assert.HasCount(2, result[PatternKind.GitIgnore]);
    }

    /// <summary>
    /// Verifies that the <see cref="PatternBuilder.Build(string?, string?, string?, string?)"/> method handles specific inputs for different pattern types.
    /// </summary>
    [TestMethod]
    public void Build_ShouldHandleSpecificInputs() {
        var result = PatternBuilder.Build(
            patterns: "a.txt",
            gitignore: "b.txt",
            glob: "*.c",
            regex: "r:d.txt"
        );

        Assert.HasCount(3, result);
        Assert.IsTrue(result.ContainsKey(PatternKind.GitIgnore));
        Assert.IsTrue(result.ContainsKey(PatternKind.Glob));
        Assert.IsTrue(result.ContainsKey(PatternKind.Regex));
        Assert.HasCount(2, result[PatternKind.GitIgnore]);
        Assert.HasCount(1, result[PatternKind.Glob]);
        Assert.HasCount(1, result[PatternKind.Regex]);
    }

    /// <summary>
    /// Verifies that the <see cref="PatternBuilder.Build(ParseResult, CommandLinePatternOptions)"/> method correctly calls the parser.
    /// </summary>
    [TestMethod]
    public void Build_ParseResult_ShouldCallParser() {
        var options = new TestOptions();
        var rootCommand = new RootCommand {
            options.Patterns
        };
        var result = rootCommand.Parse("--patterns p1");

        var parsed = PatternBuilder.Build(result, options);

        Assert.IsTrue(parsed.ContainsKey(PatternKind.GitIgnore));
        Assert.AreEqual("p1", parsed[PatternKind.GitIgnore][0]);
    }

    private sealed class TestOptions : CommandLinePatternOptions {
        public string Param1 { get; set; } = string.Empty;
    }
}
