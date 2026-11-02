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

        context.Text.Should().BeNull();
        context.Segments.Should().BeNull();
        context.Classified.Should().BeNull();
        context.Compiled.Should().BeNull();
    }

    /// <summary>
    /// Verifies that the constructor with a text argument initializes the Text property correctly.
    /// </summary>
    [TestMethod]
    public void Constructor_WithText_SetsText() {
        var context = new PatternInvariantContext { Text = "test" };

        context.Text.Should().Be("test");
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

        context.Text.Should().Be("a");
        context.Segments.Should().NotBeNull();
        context.Classified.Should().Be(classified);
        context.Compiled.Should().Be(compiled);
    }
}
