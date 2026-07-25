using Jeninnet.FileQuery.Patterns.Invariants;
using Jeninnet.FileQuery.Patterns.Classification;
using Jeninnet.FileQuery.Patterns;
using Moq; // Assuming Moq is available as it's common

namespace Jeninnet.FileQuery.Tests.Patterns.Invariants;

[TestClass]
public sealed class PatternInvariantContextTests {
    [TestMethod]
    public void Constructor_AllNull_SetsNulls() {
        var context = new PatternInvariantContext();
        
        Assert.IsNull(context.Text);
        Assert.IsNull(context.Segments);
        Assert.IsNull(context.Classified);
        Assert.IsNull(context.Compiled);
    }

    [TestMethod]
    public void Constructor_WithText_SetsText() {
        var context = new PatternInvariantContext { Text = "test" };
        
        Assert.AreEqual("test", context.Text);
    }

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
