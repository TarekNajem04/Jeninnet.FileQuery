//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Unit.Patterns;

/// <summary>
/// Tests for <see cref="PatternInvariantContext"/>.
/// </summary>
[TestClass]
public sealed class PatternInvariantContextTests {
    /// <summary>
    /// Verifies that all properties are null when using the default constructor.
    /// </summary>
    [TestMethod]
    public void Should_SetNulls_When_AllNullConstructor() {
        var context = new PatternInvariantContext();

        Assert.IsNull(context.Text);
        Assert.IsNull(context.Segments);
        Assert.IsNull(context.Classified);
        Assert.IsNull(context.Compiled);
    }

    /// <summary>
    /// Verifies that the Text property is set correctly when initialized with text.
    /// </summary>
    [TestMethod]
    public void Should_SetText_When_ConstructedWithText() {
        var context = new PatternInvariantContext { Text = "test" };

        Assert.AreEqual("test", context.Text);
    }

    /// <summary>
    /// Verifies that all properties are set correctly during full initialization.
    /// </summary>
    [TestMethod]
    public void Should_SetAllProperties_When_FullInitialization() {
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
