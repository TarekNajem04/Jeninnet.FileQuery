//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Patterns.Invariants;

/// <summary>
/// Contains unit tests for the PatternInvariantContext model.
/// </summary>
[TestClass]
public sealed class PatternInvariantContextTests {
    /// <summary>
    /// Verifies that the parameterless constructor initializes all properties to null.
    /// </summary>
    [TestMethod]
    public void Constructor_AllNull_SetsNulls() {
        var context = new PatternInvariantContext();

        Assert.IsNull(context.Text);
        Assert.IsNull(context.Segments);
        Assert.IsNull(context.Classified);
        Assert.IsNull(context.Compiled);
    }

    /// <summary>
    /// Verifies that the constructor with a text argument initializes the Text property correctly.
    /// </summary>
    [TestMethod]
    public void Constructor_WithText_SetsText() {
        var context = new PatternInvariantContext { Text = "test" };

        Assert.AreEqual("test", context.Text);
    }

    /// <summary>
    /// Verifies that the constructor correctly initializes all properties when provided.
    /// </summary>
    [TestMethod]
    public void Constructor_FullInitialization_SetsAllProperties() {
        var classified = new ClassifiedPattern("a", PatternKind.Glob);
        var compiled = new FakeCompiledPattern();

        var context = new PatternInvariantContext {
            Text = "a",
            Segments = new List<List<IPatternToken>> { new() },
            Classified = classified,
            Compiled = compiled
        };

        Assert.AreEqual("a", context.Text);
        Assert.IsNotNull(context.Segments);
        Assert.AreEqual(classified, context.Classified);
        Assert.AreEqual(compiled, context.Compiled);
    }
}
