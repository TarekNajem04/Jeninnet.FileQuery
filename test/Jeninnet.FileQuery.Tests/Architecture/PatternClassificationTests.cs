namespace Jeninnet.FileQuery.Tests.Architecture;

[TestClass]
public sealed class PatternClassificationTests
{
    [TestMethod]
    public void Specific_Mode_Rejects_Untyped_Pattern_When_Specific()
    {
        var canonical = new CanonicalPatternSet
        {
            Patterns = [
                new CanonicalPattern(Text: "*.cs",  ExplicitType: null)
            ]
        };

        Assert.ThrowsExactly<PatternException>(() =>
            PatternClassifier.Classify(canonical, PatternInterpretationMode.Specific));
    }
}
