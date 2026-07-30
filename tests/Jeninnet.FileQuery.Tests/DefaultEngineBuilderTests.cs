namespace Jeninnet.FileQuery.Tests;

/// <summary>
/// Contains tests for the default engine builder.
/// </summary>
[TestClass]
public sealed class DefaultEngineBuilderTests {
    /// <summary>
    /// Tests that the default engine builder creates a working engine.
    /// </summary>
    [TestMethod]
    public void Create_ShouldReturnWorkingEngine() {
        using var env = new TestEnvironment();

        env.CreateFiles("a.txt", "b.txt", "c.log", "sub/d.txt");

        var engine = FileQueryRuntime.Create();

        // Direct API usage
        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "!**"
                    ]
                )
            )
        );

        var expected = fileQueryEngine.Execute(new(env.Root, options)).ToList();

        // Fluent API usage
        var query = FileQuery.From(env.Root)
                             .Where("!**")
                             .Build()
                             ;

        var actual = engine.Execute(query).ToList();

        Assert.IsNotNull(expected);
        Assert.IsNotNull(actual);
        Assert.AreSequenceEqual(expected, actual);
    }
}
