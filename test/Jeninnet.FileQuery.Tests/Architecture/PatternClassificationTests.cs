namespace Jeninnet.FileQuery.Tests.Architecture;

/// <summary>
/// Contains architecture unit tests for pattern classification.
/// </summary>
[TestClass]
public sealed class PatternClassificationTests {
    /// <summary>
    /// Verifies that the <see cref="PatternClassifier.Classify(CanonicalPatternSet, PatternInterpretationMode)"/> method throws <see cref="PatternException"/> when an untyped pattern is provided in <see cref="PatternInterpretationMode.Specific"/> mode.
    /// </summary>
    [TestMethod]
    public void Specific_Mode_Rejects_Untyped_Pattern_When_Specific() {
        var canonical = new CanonicalPatternSet {
            Patterns = [
                new CanonicalPattern(Text: "*.cs",  ExplicitType: null)
            ]
        };

        Assert.ThrowsExactly<PatternException>(() =>
            PatternClassifier.Classify(canonical, PatternInterpretationMode.Specific));
    }
}

