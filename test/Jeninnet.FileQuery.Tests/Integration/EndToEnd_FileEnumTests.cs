namespace Jeninnet.FileQuery.Tests.Integration;

[TestClass]
public class EndToEnd_FileEnumTests {
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
            patternInput: new(patterns: ["**", "!**/*.cs"]),
            recurseSubdirectories: true
        );

        var result = fileQueryEngine.Execute(new(env.Root, options)).ToList();

        TestAssertEx.AreEquivalent(result, [
            env.Abs("b.cs"),
            env.Abs("src", "main.cs"),
            env.Abs("src", "util", "helpers.cs")
        ]);
    }

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
            .Execute(new(
                env.Root,
                new FileQueryOptions(
                    patternInput: new(patterns: ["**", "!*.cs", "bin/**", "obj/**"]),
                    recurseSubdirectories: true
                )))
            .Select(PathUtilities.Normalize)
            .ToList();

        TestAssertEx.AreEquivalent(result, [
            PathUtilities.Normalize(env.Abs("b.cs")),
            PathUtilities.Normalize(env.Abs("src", "main.cs")),
            PathUtilities.Normalize(env.Abs("src", "util", "helpers.cs"))
        ]);
        TestAssertEx.DoesNotContain(result, x => x.Contains("/bin/"));
        TestAssertEx.DoesNotContain(result, x => x.Contains("/obj/"));
    }

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
            .Execute(new(
                env.Root,
                new FileQueryOptions(
                    patternInput: new(patterns),
                    recurseSubdirectories: true
                )))
            .ToList();

        TestAssertEx.Contains(result, env.Abs("b.cs"));
        TestAssertEx.Contains(result, env.Abs("src", "main.cs"));
        TestAssertEx.DoesNotContain(result, x => x.EndsWith("helpers.cs"));
        TestAssertEx.DoesNotContain(result, x => x.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"));
        TestAssertEx.DoesNotContain(result, x => x.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"));
        TestAssertEx.HasCount(result, 2);
    }

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
            .Execute(new(
                env.Root,
                new FileQueryOptions(
                    patternInput: new(patterns: ["**", "!**/*.cs"]),
                    recurseSubdirectories: true,
                    maxRecursionDepth: 0
                )))
            .Select(PathUtilities.Normalize)
            .ToList();

        TestAssertEx.AreEquivalent(result, [PathUtilities.Normalize(env.Abs("b.cs"))]);
    }

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
            .Execute(new(
                env.Root,
                new FileQueryOptions(
                    patternInput: new(patterns: ["**", "!**/*.txt"]),
                    recurseSubdirectories: true
                )))
            .Select(PathUtilities.Normalize)
            .ToList();

        TestAssertEx.AreEquivalent(result, [PathUtilities.Normalize(env.Abs("a.txt"))]);
    }

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
            patternInput: new(patterns: ["**", "!**/*.cs", "src/util/**"]),
            recurseSubdirectories: true
        );

        var result = FileQueryRuntime.Create().Execute(new(env.Root, options)).ToList();

        TestAssertEx.DoesNotContain(result, x => x.Contains($"{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}util{Path.DirectorySeparatorChar}"));
        TestAssertEx.Contains(result, env.Abs("src", "main.cs"));
    }

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
            patternInput: new(patterns: ["**", "!a.txt", "!file.txt"]),
            caseSensitivity: CaseSensitivity.Insensitive
        );
        var sensitiveOptions = new FileQueryOptions(
            patternInput: new(patterns: ["**", "!a.txt", "!file.txt"]),
            caseSensitivity: CaseSensitivity.Sensitive
        );

        var insensitiveResult = fileQueryEngine.Execute(new(env.Root, insensitiveOptions)).ToList();
        var sensitiveResult = fileQueryEngine.Execute(new(env.Root, sensitiveOptions)).ToList();

        TestAssertEx.HasCount(insensitiveResult, 2);
        TestAssertEx.HasCount(sensitiveResult, 1);
        TestAssertEx.Contains(sensitiveResult, env.Abs("a.txt"));
    }
}
