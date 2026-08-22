//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Core.FileCollectorSync;

/// <summary>
/// Provides additional synchronous tests for the <see cref="IFileQueryEngine"/>.
/// </summary>
[TestClass]
public class FileCollectorSyncTests_Additional {

    /// <summary>
    /// Creates a temporary directory with the given subpaths and returns its path.
    /// Used to simplify test setup.
    /// </summary>
    /// <param name="files">A dictionary mapping file paths to their content, or null for directories.</param>
    /// <returns>The path to the created temporary directory.</returns>
    private static string CreateTree(Dictionary<string, string?> files) {
        var root = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(root);

        foreach(var pair in files) {
            var full = Path.Combine(root, pair.Key);

            var dir = Path.GetDirectoryName(full)!;
            Directory.CreateDirectory(dir);

            if(pair.Value is not null) {
                File.WriteAllText(full, pair.Value);
            } else {
                Directory.CreateDirectory(full);
            }
        }

        return root;
    }

    /// <summary>
    /// Verifies that a pattern ending with '/' matches directories ONLY,
    /// allowing files *inside* matched directories to pass through.
    /// </summary>
    [TestMethod]
    public void DirectoryPattern_ShouldMatchDirectoriesOnly() {
        var root = CreateTree(new() {
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
        var result = fc.Execute(new(root, opts)).ToList();

        // only logs directory included ? includes file inside it
        var onlyResult = TestAssertEx.ContainsSingle(result);
        TestAssertEx.EndsWith(onlyResult, Path.Combine("logs", "file1.txt"));
    }

    /// <summary>
    /// Verifies that negation restores a previously-excluded directory.
    /// </summary>
    [TestMethod]
    public void DirectoryNegation_ShouldRestoreFiles() {
        var root = CreateTree(new() {
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
        var result = fc.Execute(new(root, opts)).ToList();

        var onlyResult = TestAssertEx.ContainsSingle(result);
        TestAssertEx.EndsWith(onlyResult, Path.Combine("logs", "b.txt"));
    }

    /// <summary>
    /// Verifies that a leading slash matches only at the root level.
    /// </summary>
    [TestMethod]
    public void AnchoredPattern_ShouldMatchOnlyAtRoot() {
        var root = CreateTree(new() {
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
        var result = fc.Execute(new(root, opts)).ToList();

        var onlyResult = TestAssertEx.ContainsSingle(result);
        TestAssertEx.EndsWith(onlyResult, "a.txt");
    }

    /// <summary>
    /// Verifies that unanchored patterns match anywhere.
    /// </summary>
    [TestMethod]
    public void UnanchoredPattern_ShouldMatchAnywhere() {
        var root = CreateTree(new() {
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
        var result = fc.Execute(new(root, opts)).ToList();

        TestAssertEx.HasCount(result, 2);
    }

    /// <summary>
    /// Verifies that negation overrides previous exclusion patterns.
    /// </summary>
    [TestMethod]
    public void Negation_ShouldOverridePrevious() {
        var root = CreateTree(new() {
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

        var result = FileQueryRuntime.Create().Execute(new(root, opts)).ToList();

        var onlyResult = TestAssertEx.ContainsSingle(result);
        TestAssertEx.EndsWith(onlyResult, "other.log");
    }

    /// <summary>
    /// Verifies that negation allows files inside an ignored parent directory.
    /// </summary>
    [TestMethod]
    public void NegationInsideIgnoredDirectory_ShouldBeIncluded() {
        var root = CreateTree(new() {
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

        var result = FileQueryRuntime.Create().Execute(new(root, opts)).ToList();

        var onlyResult = TestAssertEx.ContainsSingle(result);
        TestAssertEx.EndsWith(onlyResult, Path.Combine("logs", "hidden", "keep.txt"));
    }

    /// <summary>
    /// Verifies that character classes are matched correctly.
    /// </summary>
    [TestMethod]
    public void CharacterClass_ShouldMatchCorrectly() {
        var root = CreateTree(new() {
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

        var result = FileQueryRuntime.Create().Execute(new(root, opts)).ToList();
        TestAssertEx.HasCount(result, 2);
    }

    /// <summary>
    /// Verifies that negated character classes are matched correctly.
    /// </summary>
    [TestMethod]
    public void NegatedCharacterClass_ShouldMatchCorrectly() {
        var root = CreateTree(new() {
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

        var result = FileQueryRuntime.Create().Execute(new(root, opts)).ToList();
        var onlyResult = TestAssertEx.ContainsSingle(result);
        TestAssertEx.EndsWith(onlyResult, "file1.txt");
    }

    /// <summary>
    /// Verifies that multi-wildcards match complex file layouts.
    /// </summary>
    [TestMethod]
    public void MultiWildcard_ShouldMatchComplexLayout() {
        var root = CreateTree(new() {
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

        var result = FileQueryRuntime.Create().Execute(new(root, opts)).ToList();
        TestAssertEx.HasCount(result, 3);
    }

    /// <summary>
    /// Verifies that dotfiles do not match with a standard star pattern.
    /// </summary>
    [TestMethod]
    public void Dotfile_ShouldNotMatchWithStar() {
        var root = CreateTree(new() {
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

        var result = FileQueryRuntime.Create().Execute(new(root, opts)).ToList();
        TestAssertEx.IsNotEmpty(result);
    }

    /// <summary>
    /// Verifies that dotfiles match when explicitly included.
    /// </summary>
    [TestMethod]
    public void Dotfile_ShouldMatchWhenExplicit() {
        var root = CreateTree(new() {
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

        var result = FileQueryRuntime.Create().Execute(new(root, opts)).ToList();
        TestAssertEx.ContainsSingle(result);
    }

    /// <summary>
    /// Verifies that the recursion depth limit correctly restricts traversal.
    /// </summary>
    [TestMethod]
    public void MaxDepth_ShouldLimitTraversal() {
        var root = CreateTree(new() {
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

        var result = FileQueryRuntime.Create().Execute(new(root, opts)).ToList();
        TestAssertEx.IsEmpty(result);
    }

    /// <summary>
    /// Verifies that returned paths use the OS-specific separator.
    /// </summary>
    [TestMethod]
    public void ReturnedPath_ShouldUseOSSeparator() {
        var root = CreateTree(new() {
            ["sub/file.txt"] = "x"
        });

        var result = FileQueryRuntime.Create()
                                           .Execute(
                                               new(
                                                   root,
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
        TestAssertEx.Contains(result.Single(), Path.DirectorySeparatorChar.ToString());
    }
}
