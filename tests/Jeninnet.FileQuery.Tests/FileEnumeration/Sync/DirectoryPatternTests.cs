//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.FileEnumeration.Sync;

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

    /// <summary>
    /// Verifies that directory-only patterns correctly exclude files matching a negated pattern
    /// while pruning subdirectories.
    /// </summary>
    [TestMethod]
    public void Should_ExcludeFilesCorrectly_When_DirectoryOnlyPatternUsed() {
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
        result.Should().Contain(env.Abs("src", "main.cs"));
        result.Should().Contain(env.Abs("b.cs"));
        // src/util/helpers.cs must be excluded due to pruning.
        result.Should().NotContain(static x => x.Contains(Path.Combine("src", "util")));
    }

    /// <summary>
    /// Verifies that directory patterns match and prune at multiple directory levels.
    /// </summary>
    [TestMethod]
    public void Should_MatchMultipleLevels_When_DirectoryPatternUsed() {
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
        result.Should().NotContain(env.Abs("src", "util", "helpers.cs"));
    }

    /// <summary>
    /// Verifies that multiple directory exclusion patterns stack correctly to prune
    /// multiple subtrees simultaneously.
    /// </summary>
    [TestMethod]
    public void Should_Stack_When_MultipleDirectoryPatternsUsed() {
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
        result.Should().Contain(env.Abs("b.cs"));
        result.Should().Contain(env.Abs("src", "main.cs"));
        // Check both excluded directories
        result.Should().NotContain(static x => x.Contains(Path.Combine("src", "util")));
        result.Should().NotContain(static x => x.Contains(Path.Combine("src", "other")));
    }
}
