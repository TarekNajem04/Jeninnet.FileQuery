namespace Jeninnet.FileQuery.Tests.Patterns;

[TestClass]
public sealed class CanonicalPatternInputTests {
    [TestMethod]
    public void Constructor_Default_SetsEmptyPatternsAndInterpretationMode() {
        var input = new CanonicalPatternInput();

        Assert.IsEmpty(input.Patterns);
        Assert.IsEmpty(input.TypedPatterns);
        Assert.AreEqual(PatternInterpretationMode.Hybrid, input.InterpretationMode);
    }

    [TestMethod]
    public void Constructor_WithPatterns_SetsPatterns() {
        string[] patterns = ["a", "b"];
        var input = new CanonicalPatternInput(patterns: patterns);

        Assert.HasCount(2, input.Patterns);
        Assert.AreEqual("a", input.Patterns[0]);
        Assert.AreEqual("b", input.Patterns[1]);
    }

    [TestMethod]
    public void Constructor_WithTypedPatterns_SetsTypedPatterns() {
        var typed = new Dictionary<PatternKind, IEnumerable<string>> {
            { PatternKind.Glob, ["*.txt"] }
        };
        var input = new CanonicalPatternInput(typedPatterns: typed);

        Assert.HasCount(1, input.TypedPatterns);
        Assert.IsTrue(input.TypedPatterns.ContainsKey(PatternKind.Glob));
        Assert.AreEqual("*.txt", input.TypedPatterns[PatternKind.Glob][0]);
    }

    [TestMethod]
    public void Constructor_WithTypedPatternsNullList_SetsEmptyList() {
        var typed = new Dictionary<PatternKind, IEnumerable<string>> {
            { PatternKind.Glob, null! }
        };
        var input = new CanonicalPatternInput(typedPatterns: typed);

        Assert.HasCount(1, input.TypedPatterns);
        Assert.IsEmpty(input.TypedPatterns[PatternKind.Glob]);
    }

    [TestMethod]
    public void Constructor_WithMultipleTypedPatterns_SetsAll() {
        var typed = new Dictionary<PatternKind, IEnumerable<string>> {
            { PatternKind.Glob, ["a"] },
            { PatternKind.Regex, ["b"] }
        };
        var input = new CanonicalPatternInput(typedPatterns: typed);

        Assert.HasCount(2, input.TypedPatterns);
        Assert.AreEqual("a", input.TypedPatterns[PatternKind.Glob][0]);
        Assert.AreEqual("b", input.TypedPatterns[PatternKind.Regex][0]);
    }

    [TestMethod]
    public void Constructor_WithExplicitMode_SetsMode() {
        var input = new CanonicalPatternInput(interpretationMode: PatternInterpretationMode.Specific);

        Assert.AreEqual(PatternInterpretationMode.Specific, input.InterpretationMode);
    }

    [TestMethod]
    public void Constructor_WithEmptyTypedPatterns_SetsEmpty() {
        var input = new CanonicalPatternInput(typedPatterns: new Dictionary<PatternKind, IEnumerable<string>>());

        Assert.IsEmpty(input.TypedPatterns);
    }
}
