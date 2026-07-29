namespace Jeninnet.FileQuery.Tests.Integration;

/// <summary>
/// Contains end-to-end tests for file enumeration scenarios.
/// </summary>
[TestClass]
public class EndToEnd_FileEnumTests {
    /// <summary>Tests ShouldEnumerate_AllCsFiles.</summary>
    [TestMethod]
    public void ShouldEnumerate_AllCsFiles() {
        using var env = new TestEnvironment();
        env.CreateFiles(
            "a.txt",
            "b.cs",
            "keep.md",
            "bin/skip.dll",
            "obj/temp/x.tmp",
            "src/main.cs",
            "src/util/helpers.cs");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(Patterns: ["**", "!**/*.cs"]),
                RecurseSubdirectories: true
            )
        );

        var result = fileQueryEngine.Execute(new(env.Root, options)).ToList();

        TestAssertEx.AreEquivalent(result, [
            env.Abs("b.cs"),
            env.Abs("src", "main.cs"),
            env.Abs("src", "util", "helpers.cs")
        ]);
    }

    /// <summary>Tests ShouldRespect_IgnoreDirectories.</summary>
    [TestMethod]
    public void ShouldRespect_IgnoreDirectories() {
        using var env = new TestEnvironment();
        env.CreateFiles(
            "a.txt",
            "b.cs",
            "keep.md",
            "bin/skip.dll",
            "obj/temp/x.tmp",
            "src/main.cs",
            "src/util/helpers.cs");

        var result = FileQueryRuntime.Create()
            .Execute(
                new(
                    env.Root,
                    new FileQueryOptions(
                            new FileQueryOptionsConfig(
                            PatternInput: new(Patterns: [
                                "**",
                                "!*.cs",
                                "bin/**",
                                "obj/**"
                                ]
                            ),
                            RecurseSubdirectories: true
                        )
                    )
                )
            )
            .Select(PathUtilities.Normalize)
            .ToList();

        TestAssertEx.AreEquivalent(result, [
            PathUtilities.Normalize(env.Abs("b.cs")),
            PathUtilities.Normalize(env.Abs("src", "main.cs")),
            PathUtilities.Normalize(env.Abs("src", "util", "helpers.cs"))
        ]);
        TestAssertEx.DoesNotContain(result, x => x.Contains("/bin/", StringComparison.Ordinal));
        TestAssertEx.DoesNotContain(result, x => x.Contains("/obj/", StringComparison.Ordinal));
    }

    /// <summary>Tests Complex_GitIgnoreScenario.</summary>
    [TestMethod]
    public void Complex_GitIgnoreScenario() {
        using var env = new TestEnvironment();
        env.CreateFiles(
            "a.txt",
            "b.cs",
            "keep.md",
            "bin/skip.dll",
            "obj/temp/x.tmp",
            "src/main.cs",
            "src/util/helpers.cs");

        var patterns = new[] { "**", "!**/*.cs", "src/util/**", "bin/**", "obj/**" };

        var result = FileQueryRuntime.Create()
            .Execute(
                new(
                    env.Root,
                    new FileQueryOptions(
                        new FileQueryOptionsConfig(
                            PatternInput: new(patterns),
                            RecurseSubdirectories: true
                        )
                    )
                )
            )
            .ToList();

        TestAssertEx.Contains(result, env.Abs("b.cs"));
        TestAssertEx.Contains(result, env.Abs("src", "main.cs"));
        TestAssertEx.DoesNotContain(result, x => x.EndsWith("helpers.cs", StringComparison.Ordinal));
        TestAssertEx.DoesNotContain(result, x => x.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal));
        TestAssertEx.DoesNotContain(result, x => x.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal));
        TestAssertEx.HasCount(result, 2);
    }

    /// <summary>Tests ShouldLimit_Depth.</summary>
    [TestMethod]
    public void ShouldLimit_Depth() {
        using var env = new TestEnvironment();
        env.CreateFiles(
            "a.txt",
            "b.cs",
            "keep.md",
            "bin/skip.dll",
            "obj/temp/x.tmp",
            "src/main.cs",
            "src/util/helpers.cs");

        var result = FileQueryRuntime.Create()
            .Execute(
                new(
                    env.Root,
                    new FileQueryOptions(
                        new FileQueryOptionsConfig(
                            PatternInput: new(Patterns: ["**", "!**/*.cs"]),
                            RecurseSubdirectories: true,
                            MaxRecursionDepth: 0
                        )
                    )
                )
            )
            .Select(PathUtilities.Normalize)
            .ToList();

        TestAssertEx.AreEquivalent(result, [PathUtilities.Normalize(env.Abs("b.cs"))]);
    }

    /// <summary>Tests DoubleStar_ShouldMatchNested.</summary>
    [TestMethod]
    public void DoubleStar_ShouldMatchNested() {
        using var env = new TestEnvironment();
        env.CreateFiles(
            "a.txt",
            "b.cs",
            "keep.md",
            "bin/skip.dll",
            "obj/temp/x.tmp",
            "src/main.cs",
            "src/util/helpers.cs");

        var result = FileQueryRuntime.Create()
            .Execute(
                new(
                    env.Root,
                    new FileQueryOptions(
                        new FileQueryOptionsConfig(
                            PatternInput: new(Patterns: ["**", "!**/*.txt"]),
                            RecurseSubdirectories: true
                        )
                    )
                )
            )
            .Select(PathUtilities.Normalize)
            .ToList();

        TestAssertEx.AreEquivalent(result, [PathUtilities.Normalize(env.Abs("a.txt"))]);
    }

    /// <summary>Tests DirectoryOnlyPattern_ShouldExcludeFilesCorrectly.</summary>
    [TestMethod]
    public void DirectoryOnlyPattern_ShouldExcludeFilesCorrectly() {
        using var env = new TestEnvironment();
        env.CreateFiles(
            "a.txt",
            "b.cs",
            "keep.md",
            "bin/skip.dll",
            "obj/temp/x.tmp",
            "src/main.cs",
            "src/util/helpers.cs");

        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(Patterns: ["**", "!**/*.cs", "src/util/**"]),
                RecurseSubdirectories: true
            )
        );

        var result = FileQueryRuntime.Create().Execute(new(env.Root, options)).ToList();

        TestAssertEx.DoesNotContain(result, x => x.Contains($"{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}util{Path.DirectorySeparatorChar}", StringComparison.Ordinal));
        TestAssertEx.Contains(result, env.Abs("src", "main.cs"));
    }

    /// <summary>Tests CaseSensitivity_ShouldFollow_Options.</summary>
    [TestMethod]
    public void CaseSensitivity_ShouldFollow_Options() {
        using var env = new TestEnvironment();
        env.CreateFiles(
            "a.txt",
            "b.cs",
            "a.txt",
            "FiLe.TxT",
            "keep.md",
            "bin/skip.dll",
            "obj/temp/x.tmp",
            "src/main.cs",
            "src/util/helpers.cs");

        var fileQueryEngine = FileQueryRuntime.Create();
        var insensitiveOptions = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(Patterns: ["**", "!a.txt", "!file.txt"]),
                CaseSensitivity: CaseSensitivity.Insensitive
            )
        );
        var sensitiveOptions = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(Patterns: ["**", "!a.txt", "!file.txt"]),
                CaseSensitivity: CaseSensitivity.Sensitive
            )
        );

        var insensitiveResult = fileQueryEngine.Execute(new(env.Root, insensitiveOptions)).ToList();
        var sensitiveResult = fileQueryEngine.Execute(new(env.Root, sensitiveOptions)).ToList();

        TestAssertEx.HasCount(insensitiveResult, 2);
        TestAssertEx.HasCount(sensitiveResult, 1);
        TestAssertEx.Contains(sensitiveResult, env.Abs("a.txt"));
    }
}
