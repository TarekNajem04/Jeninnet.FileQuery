namespace Jeninnet.FileQuery.Tests.Core.FileCollectorSync.Basic;

[TestClass]
public class NegationAndOrderTests {
    /// <summary>
    /// Basic !pattern exclusion behavior.
    /// </summary>
    [TestMethod]
    public void Negation_ShouldExcludeSpecifiedFiles() {
        using var env = new TestEnvironment();
        env.CreateFiles("a.txt", "b.txt", "c.log");

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

        var result = fileQueryEngine.Execute(new(env.Root, options))
                                    .Order()
                                    .ToList();

        TestAssertEx.HasCount(result, 1);
        Assert.EndsWith("a.txt", result.Single());
    }

    /// <summary>
    /// Ensures last matching rule wins.
    /// </summary>
    [TestMethod]
    public void LastRuleWins_ShouldOverrideEarlierRules() {
        using var env = new TestEnvironment();
        env.CreateFiles("data.log", "data.tmp");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            patternInput: new(
                patterns: [
                    "**",
                    "!*.log",
                    "data.log",
                    "!data.log" // last rule → include again
                ]
            )
        );

        var result = fileQueryEngine.Execute(new(env.Root, options))
                                    .ToList();

        TestAssertEx.ContainsSingle(result, x => x.EndsWith("data.log"));
    }

    /// <summary>
    /// Directory-only negation cases.
    /// </summary>
    [TestMethod]
    public void NegateDirectoryOnlyRules() {
        using var env = new TestEnvironment();

        env.CreateFile("sub/file.txt");
        env.CreateFile("file.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            patternInput: new(
                patterns: [
                    "**",
                    "sub/**",       // exclude everything under sub
                    "!sub/file.txt" // but re-include this file
                ]
            ),
            recurseSubdirectories: true
        );

        var result = fileQueryEngine.Execute(new(env.Root, options))
                                    .ToList();

        TestAssertEx.Contains(result, x => x.EndsWith(Path.Combine("sub", "file.txt")));
        TestAssertEx.Contains(result, x => x.EndsWith("file.txt"));
    }
}
