namespace Jeninnet.FileQuery.Tests.Core.FileCollectorSync.DirectoryRules;

/// <summary>
/// Tests directory patterns ("dir/", "dir/**") in synchronous file enumeration.
/// Verifies that when a directory is excluded, its entire subtree is skipped
/// during traversal (Efficiency check).
/// </summary>
[TestClass]
public class DirectoryPatternTests {
    // --- Setup Helpers ---

    private static void CreateStandardStructure(TestEnvironment env) =>
        env.CreateFiles(
            "a.txt",
            "b.cs",
            "keep.md",
            "bin/skip.dll",
            "obj/temp/x.tmp",
            "src/main.cs",
            "src/util/helpers.cs"
        );

    // --- Test Methods ---

    [TestMethod]
    public void DirectoryOnlyPattern_ShouldExcludeFilesCorrectly() {
        // ARRANGE
        using var env = new TestEnvironment();
        CreateStandardStructure(env);
        var fileQueryEngine = FileQueryRuntime.Create();

        // ACT
        var result = fileQueryEngine.Execute(
            new FileQuery(
                env.Root,
                new FileQueryOptions(
                    new FileQueryOptionsConfig(
                        PatternInput: new(
                            Patterns: [
                                "**",
                                "!**/*.cs",
                                "src/util/**", // Exclusion: Prunes src/util/ and its contents
                            ]
                        ),
                        RecurseSubdirectories: true
                    )
                )
            )
        )
        .Select(Path.GetFullPath)
        .ToList();

        // ASSERT
        // Should include all .cs files EXCEPT those under src/util/
        TestAssertEx.Contains(result, env.Abs("src", "main.cs"));
        TestAssertEx.Contains(result, env.Abs("b.cs"));
        // src/util/helpers.cs must be excluded due to pruning.
        TestAssertEx.DoesNotContain(result, x => x.Contains(Path.Combine("src", "util")));
    }

    [TestMethod]
    public void DirectoryPattern_ShouldMatchMultipleLevels() {
        // ARRANGE
        using var env = new TestEnvironment();
        CreateStandardStructure(env);
        // Note: The original test asserts against 'src\util\sub', which does not exist in the setup.
        // Assuming the intent was to ensure src/util/ is pruned.

        var fileQueryEngine = FileQueryRuntime.Create();

        // ACT
        var result = fileQueryEngine
            .Execute(
                new FileQuery(
                    env.Root,
                    new FileQueryOptions(
                        new FileQueryOptionsConfig(
                            PatternInput: new(
                                Patterns: [
                                    "**",
                                    "!**/*.cs",
                                    "src/util/**",    // Exclusion: Prunes src/util/ directory subtree
                                ]
                            ),
                            RecurseSubdirectories: true
                        )
                    )
                )
            )
            .ToList();

        // ASSERT
        // Ensure that src/util/helpers.cs is NOT included.
        TestAssertEx.DoesNotContain(result, env.Abs("src", "util", "helpers.cs"));
    }

    [TestMethod]
    public void MultipleDirectoryPatterns_ShouldStack() {
        // ARRANGE
        using var env = new TestEnvironment();
        CreateStandardStructure(env);

        // Add the 'src/other' directory which the original patterns try to exclude,
        // even though it was not created in the original setup. We create it now.
        env.CreateDirectory("src/other");
        env.CreateFile("src/other/test.file");

        var fileQueryEngine = FileQueryRuntime.Create();

        // ACT
        var result = fileQueryEngine
            .Execute(
                new FileQuery(
                    env.Root,
                    new FileQueryOptions(
                        new FileQueryOptionsConfig(
                            PatternInput: new(
                                Patterns: [
                                    "**",
                                    "!**/*.cs",
                                    "src/util/**",   // Exclusion 1: Prunes src/util/ subtree
                                    "src/other/**", // Exclusion 2: Prunes src/other/ subtree
                                ]
                            )
                        )
                    )
                )
            )
            .ToList();

        // ASSERT
        TestAssertEx.Contains(result, env.Abs("b.cs"));
        TestAssertEx.Contains(result, env.Abs("src", "main.cs"));
        // Check both excluded directories
        TestAssertEx.DoesNotContain(result, x => x.Contains(Path.Combine("src", "util")));
        TestAssertEx.DoesNotContain(result, x => x.Contains(Path.Combine("src", "other")));
    }
}
