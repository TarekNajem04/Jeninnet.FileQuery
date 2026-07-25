using Jeninnet.FileQuery.Patterns.Canonical;
using Jeninnet.FileQuery.Patterns;

namespace Jeninnet.FileQuery.Tests.Patterns;

[TestClass]
public sealed class PatternCanonicalizerTests {
    [TestMethod]
    public void Canonicalize_NullInput_ThrowsArgumentNullException() {
        TestAssertEx.Throws<ArgumentNullException>(() => PatternCanonicalizer.Canonicalize(null!));
    }

    [TestMethod]
    public void Canonicalize_EmptyInput_ReturnsEmptySet() {
        var input = new CanonicalPatternInput();
        var result = PatternCanonicalizer.Canonicalize(input);
        
        Assert.AreEqual(0, result.Patterns.Count);
    }

    [TestMethod]
    public void Canonicalize_TypedPatternsOnly_ReturnsTypedPatterns() {
        var typed = new Dictionary<PatternKind, IEnumerable<string>> {
            { PatternKind.Glob, ["*.txt"] }
        };
        var input = new CanonicalPatternInput(typedPatterns: typed);
        var result = PatternCanonicalizer.Canonicalize(input);
        
        Assert.AreEqual(1, result.Patterns.Count);
        Assert.AreEqual("*.txt", result.Patterns[0].Text);
        Assert.AreEqual(PatternKind.Glob, result.Patterns[0].ExplicitType);
    }

    [TestMethod]
    public void Canonicalize_RawPatternsOnly_ReturnsRawPatternsWithTypeNull() {
        var input = new CanonicalPatternInput(patterns: ["*.cs"]);
        var result = PatternCanonicalizer.Canonicalize(input);
        
        Assert.AreEqual(1, result.Patterns.Count);
        Assert.AreEqual("*.cs", result.Patterns[0].Text);
        Assert.IsNull(result.Patterns[0].ExplicitType);
    }

    [TestMethod]
    public void Canonicalize_DuplicateTypedPatterns_Deduplicates() {
        var typed = new Dictionary<PatternKind, IEnumerable<string>> {
            { PatternKind.Glob, ["*.txt", "*.txt"] }
        };
        var input = new CanonicalPatternInput(typedPatterns: typed);
        var result = PatternCanonicalizer.Canonicalize(input);
        
        Assert.AreEqual(1, result.Patterns.Count);
    }

    [TestMethod]
    public void Canonicalize_DuplicateRawPatterns_Deduplicates() {
        var input = new CanonicalPatternInput(patterns: ["*.cs", "*.cs"]);
        var result = PatternCanonicalizer.Canonicalize(input);
        
        Assert.AreEqual(1, result.Patterns.Count);
    }

    [TestMethod]
    public void Canonicalize_OverlapRawAndTyped_PreservesBoth() {
        var typed = new Dictionary<PatternKind, IEnumerable<string>> {
            { PatternKind.Regex, ["test"] }
        };
        var input = new CanonicalPatternInput(patterns: ["test"], typedPatterns: typed);
        var result = PatternCanonicalizer.Canonicalize(input);
        
        Assert.AreEqual(2, result.Patterns.Count);
    }

    [TestMethod]
    public void Canonicalize_MultipleTypes_PreservesAll() {
        var typed = new Dictionary<PatternKind, IEnumerable<string>> {
            { PatternKind.Glob, ["*.txt"] },
            { PatternKind.Regex, ["\\.log$"] }
        };
        var input = new CanonicalPatternInput(typedPatterns: typed);
        var result = PatternCanonicalizer.Canonicalize(input);
        
        Assert.AreEqual(2, result.Patterns.Count);
    }
}
