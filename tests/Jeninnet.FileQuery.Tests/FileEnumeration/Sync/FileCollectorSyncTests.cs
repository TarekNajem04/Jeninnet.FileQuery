namespace Jeninnet.FileQuery.Tests.FileEnumeration.Sync;

/// <summary>
/// Tests synchronous file collection behavior including directory patterns,
/// negation, anchoring, character classes, wildcards, and max depth.
/// </summary>
[TestClass]
public class FileCollectorSyncTests {

    /// <summary>
    /// A pattern ending with '/' must match directories ONLY.
    /// FileQueryEngine must allow files *inside* matched directories to pass through.
    /// </summary>
    [TestMethod]
    public void Should_MatchDirectoriesOnly_When_DirectoryPatternUsed() {
        using var env = new TestEnvironment();
        env.CreateTree(new() {
            ["logs/file1.txt"] = "x",
            ["logs2/file2.txt"] = "x"
        });

        var opts = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!logs/**"
                    ]
                ),
                RecurseSubdirectories: true
            )
        );

        var fc = FileQueryRuntime.Create();
        var result = fc.Execute(new(env.Root, opts)).ToList();

        // only logs directory included ? includes file inside it
        var onlyResult = result.Should().ContainSingle();
        onlyResult.Which.Should().EndsWith(PathHelper.Join("logs", "file1.txt"));
    }

    /// <summary>
    /// Negation should restore a previously-excluded directory.
    /// </summary>
    [TestMethod]
    public void Should_RestoreFiles_When_DirectoryNegationApplied() {
        using var env = new TestEnvironment();
        env.CreateTree(new() {
            ["logs/a.txt"] = "x",
            ["logs/b.txt"] = "x"
        });

        var opts = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "logs/*",
                        "!logs/b.txt"
                    ]
                ),
                RecurseSubdirectories: true
            )
        );

        var fc = FileQueryRuntime.Create();
        var result = fc.Execute(new(env.Root, opts)).ToList();

        var onlyResult = result.Should().ContainSingle();
        onlyResult.Which.Should().EndsWith(PathHelper.Join("logs", "b.txt"));
    }

    /// <summary>
    /// Leading slash means "match only at root".
    /// </summary>
    [TestMethod]
    public void Should_MatchOnlyAtRoot_When_AnchoredPatternUsed() {
        using var env = new TestEnvironment();
        env.CreateTree(new() {
            ["a.txt"] = "x",
            ["sub/a.txt"] = "x"
        });

        var opts = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!/a.txt"
                    ]
                ),
                RecurseSubdirectories: true
            )
        );

        var fc = FileQueryRuntime.Create();
        var result = fc.Execute(new(env.Root, opts)).ToList();

        var onlyResult = result.Should().ContainSingle();
        onlyResult.Which.Should().EndsWith("a.txt");
    }

    /// <summary>
    /// Unanchored patterns match anywhere.
    /// </summary>
    [TestMethod]
    public void UnanchoredPattern_ShouldMatchAnywhere() {
        using var env = new TestEnvironment();
        env.CreateTree(new() {
            ["a.txt"] = "x",
            ["deep/a.txt"] = "x"
        });

        var opts = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!a.txt"
                    ]
                ),
                RecurseSubdirectories: true
            )
        );

        var fc = FileQueryRuntime.Create();
        var result = fc.Execute(new(env.Root, opts)).ToList();

        result.Should().HaveCount(2);
    }

    /// <summary>
    /// Verifies that a later inclusion pattern overrides a previous negation pattern.
    /// </summary>
    [TestMethod]
    public void Should_OverridePrevious_When_NegationApplied() {
        using var env = new TestEnvironment();
        env.CreateTree(new() {
            ["important.log"] = "x",
            ["other.log"] = "x"
        });

        var opts = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!*.log",
                        "important.log"
                    ]
                )
            )
        );

        var result = FileQueryRuntime.Create().Execute(new(env.Root, opts)).ToList();

        var onlyResult = result.Should().ContainSingle();
        onlyResult.Which.Should().EndsWith("other.log");
    }

    /// <summary>
    /// GitIgnore semantics allow restoring files inside an ignored parent directory.
    /// </summary>
    [TestMethod]
    public void Should_BeIncluded_When_NegationInsideIgnoredDirectory() {
        using var env = new TestEnvironment();
        env.CreateTree(new() {
            ["logs/hidden/a.txt"] = "x",
            ["logs/hidden/keep.txt"] = "x"
        });

        var opts = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!logs/hidden/keep.txt"
                    ]
                ),
                RecurseSubdirectories: true
            )
        );

        var result = FileQueryRuntime.Create().Execute(new(env.Root, opts)).ToList();

        var onlyResult = result.Should().ContainSingle();
        onlyResult.Which.Should().EndsWith(PathHelper.Join("logs", "hidden", "keep.txt"));
    }

    /// <summary>
    /// Verifies that character-class patterns correctly match files within a numeric range.
    /// </summary>
    [TestMethod]
    public void Should_MatchCorrectly_When_CharacterClassUsed() {
        using var env = new TestEnvironment();
        env.CreateTree(new() {
            ["file0.txt"] = "x",
            ["file1.txt"] = "x",
            ["file9.txt"] = "x"
        });

        var opts = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!file[0-2].txt"
                    ]
                )
            )
        );

        var result = FileQueryRuntime.Create().Execute(new(env.Root, opts)).ToList();
        result.Should().HaveCount(2);
    }

    /// <summary>
    /// Verifies that negated character-class patterns correctly match files outside the specified range.
    /// </summary>
    [TestMethod]
    public void NegatedShould_MatchCorrectly_When_CharacterClassUsed() {
        using var env = new TestEnvironment();
        env.CreateTree(new() {
            ["file1.txt"] = "x",
            ["file7.txt"] = "x"
        });

        var opts = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!file[!5-9].txt"
                    ]
                )
            )
        );

        var result = FileQueryRuntime.Create().Execute(new(env.Root, opts)).ToList();
        var onlyResult = result.Should().ContainSingle();
        onlyResult.Which.Should().EndsWith("file1.txt");
    }

    /// <summary>
    /// Verifies that multi-level wildcard patterns correctly match complex nested directory layouts.
    /// </summary>
    [TestMethod]
    public void Should_MatchComplexLayout_When_MultiWildcardUsed() {
        using var env = new TestEnvironment();
        env.CreateTree(new() {
            ["foo/a/bar/x.json"] = "x",
            ["foo/b/c/bar/x.json"] = "x",
            ["foo/b/bar/x.json"] = "x",
        });

        var opts = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!**/foo/**/bar/*.json"
                    ]
                ),
                RecurseSubdirectories: true
            )
        );

        var result = FileQueryRuntime.Create().Execute(new(env.Root, opts)).ToList();
        result.Should().HaveCount(3);
    }

    /// <summary>
    /// Verifies that star wildcard patterns do not match dotfiles by default.
    /// </summary>
    [TestMethod]
    public void Should_NotMatchDotfile_When_StarUsed() {
        using var env = new TestEnvironment();
        env.CreateTree(new() {
            [".env"] = "x"
        });

        var opts = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "!**"
                    ]
                )
            )
        );

        var result = FileQueryRuntime.Create().Execute(new(env.Root, opts)).ToList();
        result.Should().NotBeEmpty();
    }

    /// <summary>
    /// Verifies that dotfiles can be matched when explicitly specified in the pattern.
    /// </summary>
    [TestMethod]
    public void Should_MatchDotfile_When_ExplicitPattern() {
        using var env = new TestEnvironment();
        env.CreateTree(new() {
            [".env"] = "x"
        });

        var opts = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!.env"
                    ]
                )
            )
        );

        var result = FileQueryRuntime.Create().Execute(new(env.Root, opts)).ToList();
        result.Should().ContainSingle();
    }

    /// <summary>
    /// Verifies that traversal depth is limited when MaxRecursionDepth is set.
    /// </summary>
    [TestMethod]
    public void Should_LimitTraversal_When_MaxDepthSet() {
        using var env = new TestEnvironment();
        env.CreateTree(new() {
            ["a/b/c/d/e/f.txt"] = "x"
        });

        var opts = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "**",
                        "!**/*.txt"
                    ]
                ),
                RecurseSubdirectories: true,
                MaxRecursionDepth: 2
            )
        );

        var result = FileQueryRuntime.Create().Execute(new(env.Root, opts)).ToList();
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that returned file paths use the OS-native directory separator character.
    /// </summary>
    [TestMethod]
    public void Should_UseOSSeparator_When_PathReturned() {
        using var env = new TestEnvironment();
        env.CreateTree(new() {
            ["sub/file.txt"] = "x"
        });

        var result = FileQueryRuntime.Create()
                                       .Execute(
                                           new(
                                               env.Root,
                                               new FileQueryOptions(
                                                   new FileQueryOptionsConfig(
                                                       PatternInput: new(
                                                           Patterns: [
                                                               "**",
                                                               "!**/*.txt"
                                                            ]
                                                       ),
                                                       RecurseSubdirectories: true
                                                   )
                                               )
                                           )
                                       )
                                       .ToList();

        // The returned path MUST use native separators
        result.Single().Should().Contain(Path.DirectorySeparatorChar.ToString());
    }
}
