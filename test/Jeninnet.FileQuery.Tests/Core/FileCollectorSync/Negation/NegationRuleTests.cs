namespace Jeninnet.FileQuery.Tests.Core.FileCollectorSync.Negation;

/// <summary>
/// Tests GitIgnore-style negation rules (!pattern).
/// Last matching rule wins.
/// </summary>
[TestClass]
public class NegationRuleTests
{
    /// <summary>
    /// Basic inclusion via catch-all + negation for a specific file.
    /// </summary>
    [TestMethod]
    public void NegatedPattern_ShouldExcludeSpecificFile()
    {
        using var env = new TestEnvironment();

        env.CreateFiles("a.txt", "b.txt", "c.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            patternInput: new(
                patterns: [
                    "**",
                    "!*.txt",
                    "b.txt"
                ]
            )
        );

        var result = fileQueryEngine.Execute(new(env.Root, options)).Order().ToList();

        TestAssertEx.HasCount(result, 2);
        Assert.DoesNotContain(x => x.EndsWith("b.txt", StringComparison.Ordinal), result);
    }

    /// <summary>
    /// Negation should re-include files previously excluded.
    /// </summary>
    [TestMethod]
    public void NegationShouldReIncludeFiles()
    {
        using var env = new TestEnvironment();

        env.CreateFiles("keep.txt", "ignore.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            patternInput: new(
                patterns: [
                    "**",           // exclude all
                    "!*.txt",       // re-include all .txt files
                    "ignore.txt"    // explicitly exclude ignore.txt
                ]
            )
        );

        var result = fileQueryEngine.Execute(new(env.Root, options)).ToList();

        TestAssertEx.ContainsSingle(result, x => x.EndsWith("keep.txt", StringComparison.Ordinal));
    }

    /// <summary>
    /// More complex rule chain: include all, exclude folder, re-include specific file.
    /// </summary>
    [TestMethod]
    public void NegationAppliedAfterExclusion_ShouldWin()
    {
        using var env = new TestEnvironment();

        env.CreateFile("sub/inside.txt");
        env.CreateFile("sub/revive.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            patternInput: new(
                patterns: [
                    "**",           // exclude all
                    "!**/*.txt",    // include all txt
                    "sub/**",        // exclude directory "sub"
                    "!sub/revive.txt" // but re-include file revive.txt
                ]
            )
        );

        var result = fileQueryEngine.Execute(new(env.Root, options)).ToList();

        TestAssertEx.ContainsSingle(result, x => x.EndsWith("revive.txt", StringComparison.Ordinal));
    }
}
