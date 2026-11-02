//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
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

        var expected = fileQueryEngine.Execute(new(env.Root, options), TestContext.CancellationToken).ToList();

        // Fluent API usage
        var query = FileQuery.From(env.Root)
                             .Where("!**")
                             .Build()
                             ;

        var actual = engine.Execute(query, TestContext.CancellationToken).ToList();

        expected.Should().NotBeNull();
        actual.Should().NotBeNull();
        actual.Should().Equal(expected);
    }

    /// <summary>
    /// Gets or sets the test context, which provides information about the current test run.
    /// Used here primarily for accessing the <see cref="CancellationToken"/>.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;
}
