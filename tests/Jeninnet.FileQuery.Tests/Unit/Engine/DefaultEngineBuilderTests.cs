namespace Jeninnet.FileQuery.Tests.Unit.Engine;

/// <summary>
/// Tests for the default engine builder ensuring it produces a working FileQuery engine.
/// </summary>
[TestClass]
public sealed class DefaultEngineBuilderTests {
    /// <summary>
    /// Verifies that the engine created via the default builder produces identical results
    /// when used through both the direct API and the fluent API.
    /// </summary>
    [TestMethod]
    public void Should_ReturnWorkingEngine_When_Created() {
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

