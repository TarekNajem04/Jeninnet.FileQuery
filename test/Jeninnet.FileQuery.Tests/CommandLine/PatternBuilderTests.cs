namespace Jeninnet.FileQuery.Tests.CommandLine;

[TestClass]
public sealed class PatternBuilderTests
{
    [TestMethod]
    public void Build_ShouldReturnDefaultPattern_WhenInputIsEmpty()
    {
        var result = PatternBuilder.Build();

        Assert.HasCount(1, result);
        Assert.IsTrue(result.ContainsKey(PatternKind.GitIgnore));
        Assert.AreEqual("!**", result[PatternKind.GitIgnore][0]);
    }

    [TestMethod]
    public void Build_ShouldCategorizePatternsCorrectly()
    {
        const string patterns = "r:.*\\.txt;*.log";
        var result = PatternBuilder.Build(patterns);

        Assert.IsTrue(result.ContainsKey(PatternKind.Regex));
        Assert.IsTrue(result.ContainsKey(PatternKind.GitIgnore), $"Result keys: {string.Join(", ", result.Keys)}");
    }

    [TestMethod]
    public void Build_ShouldMergeSameTypePatterns()
    {
        var result = PatternBuilder.Build("a.txt;b.txt");

        Assert.IsTrue(result.ContainsKey(PatternKind.GitIgnore));
        Assert.HasCount(2, result[PatternKind.GitIgnore]);
    }

    [TestMethod]
    public void Build_ShouldHandleSpecificInputs()
    {
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

    [TestMethod]
    public void Build_ParseResult_ShouldCallParser()
    {
        var options = new TestOptions();
        var rootCommand = new RootCommand {
            options.Patterns
        };
        var result = rootCommand.Parse("--patterns p1");

        var parsed = PatternBuilder.Build(result, options);

        Assert.IsTrue(parsed.ContainsKey(PatternKind.GitIgnore));
        Assert.AreEqual("p1", parsed[PatternKind.GitIgnore][0]);
    }

    private sealed class TestOptions : CommandLinePatternOptions
    {
        public TestOptions() : base() { }
    }
}
