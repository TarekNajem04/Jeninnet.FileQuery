using Jeninnet.FileQuery.Patterns.Canonical;
using Jeninnet.FileQuery.Patterns;
using System.Collections.Immutable;

namespace Jeninnet.FileQuery.Tests.Patterns;

[TestClass]
public sealed class CanonicalPatternInputTests {
    [TestMethod]
    public void Constructor_Default_SetsEmptyPatternsAndInterpretationMode() {
        var input = new CanonicalPatternInput();
        
        Assert.AreEqual(0, input.Patterns.Length);
        Assert.AreEqual(0, input.TypedPatterns.Count);
        Assert.AreEqual(PatternInterpretationMode.Hybrid, input.InterpretationMode);
    }

    [TestMethod]
    public void Constructor_WithPatterns_SetsPatterns() {
        string[] patterns = ["a", "b"];
        var input = new CanonicalPatternInput(patterns: patterns);
        
        Assert.AreEqual(2, input.Patterns.Length);
        Assert.AreEqual("a", input.Patterns[0]);
        Assert.AreEqual("b", input.Patterns[1]);
    }

    [TestMethod]
    public void Constructor_WithTypedPatterns_SetsTypedPatterns() {
        var typed = new Dictionary<PatternKind, IEnumerable<string>> {
            { PatternKind.Glob, ["*.txt"] }
        };
        var input = new CanonicalPatternInput(typedPatterns: typed);
        
        Assert.AreEqual(1, input.TypedPatterns.Count);
        Assert.IsTrue(input.TypedPatterns.ContainsKey(PatternKind.Glob));
        Assert.AreEqual("*.txt", input.TypedPatterns[PatternKind.Glob][0]);
    }

    [TestMethod]
    public void Constructor_WithTypedPatternsNullList_SetsEmptyList() {
        var typed = new Dictionary<PatternKind, IEnumerable<string>> {
            { PatternKind.Glob, null! }
        };
        var input = new CanonicalPatternInput(typedPatterns: typed);
        
        Assert.AreEqual(1, input.TypedPatterns.Count);
        Assert.AreEqual(0, input.TypedPatterns[PatternKind.Glob].Length);
    }

    [TestMethod]
    public void Constructor_WithMultipleTypedPatterns_SetsAll() {
        var typed = new Dictionary<PatternKind, IEnumerable<string>> {
            { PatternKind.Glob, ["a"] },
            { PatternKind.Regex, ["b"] }
        };
        var input = new CanonicalPatternInput(typedPatterns: typed);
        
        Assert.AreEqual(2, input.TypedPatterns.Count);
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
        
        Assert.AreEqual(0, input.TypedPatterns.Count);
    }
}
