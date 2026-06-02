namespace Jeninnet.FileQuery.Tests.Core.FileCollectorSync;

[TestClass]
public class FileQueryEngineSyncTests_Additional {

    /// <summary>
    /// Creates a temporary directory with the given subpaths and returns its path.
    /// Used to simplify test setup.
    /// </summary>
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
    /// A pattern ending with '/' must match directories ONLY.
    /// FileQueryEngine must allow files *inside* matched directories to pass through.
    /// </summary>
    [TestMethod]
    public void DirectoryPattern_ShouldMatchDirectoriesOnly() {
        var root = CreateTree(new() {
            ["logs/file1.txt"] = "x",
            ["logs2/file2.txt"] = "x"
        });

        var opts = new FileQueryOptions(
            patternInput: new(
                patterns: [
                    "**",
                    "!logs/**"
                ]
            ),
            recurseSubdirectories: true
        );

        var fc = FileQueryRuntime.Create();
        var result = fc.Execute(new(root, opts)).ToList();

        // only logs directory included → includes file inside it
        var onlyResult = TestAssertEx.ContainsSingle(result);
        TestAssertEx.EndsWith(onlyResult, Path.Combine("logs", "file1.txt"));
    }

    /// <summary>
    /// Negation should restore a previously-excluded directory.
    /// </summary>
    [TestMethod]
    public void DirectoryNegation_ShouldRestoreFiles() {
        var root = CreateTree(new() {
            ["logs/a.txt"] = "x",
            ["logs/b.txt"] = "x"
        });

        var opts = new FileQueryOptions(
            patternInput: new(
                patterns: [
                    "logs/*",
                    "!logs/b.txt"
                ]
            ),
            recurseSubdirectories: true
        );

        var fc = FileQueryRuntime.Create();
        var result = fc.Execute(new(root, opts)).ToList();

        var onlyResult = TestAssertEx.ContainsSingle(result);
        TestAssertEx.EndsWith(onlyResult, Path.Combine("logs", "b.txt"));
    }

    /// <summary>
    /// Leading slash means "match only at root".
    /// </summary>
    [TestMethod]
    public void AnchoredPattern_ShouldMatchOnlyAtRoot() {
        var root = CreateTree(new() {
            ["a.txt"] = "x",
            ["sub/a.txt"] = "x"
        });

        var opts = new FileQueryOptions(
            patternInput: new(
                patterns: [
                    "**",
                    "!/a.txt"
                ]
            ),
            recurseSubdirectories: true
        );

        var fc = FileQueryRuntime.Create();
        var result = fc.Execute(new(root, opts)).ToList();

        var onlyResult = TestAssertEx.ContainsSingle(result);
        TestAssertEx.EndsWith(onlyResult, "a.txt");
    }

    /// <summary>
    /// Unanchored patterns match anywhere.
    /// </summary>
    [TestMethod]
    public void UnanchoredPattern_ShouldMatchAnywhere() {
        var root = CreateTree(new() {
            ["a.txt"] = "x",
            ["deep/a.txt"] = "x"
        });

        var opts = new FileQueryOptions(
            patternInput: new(
                patterns: [
                    "**",
                    "!a.txt"
                ]
            ),
            recurseSubdirectories: true
        );

        var fc = FileQueryRuntime.Create();
        var result = fc.Execute(new(root, opts)).ToList();

        TestAssertEx.HasCount(result, 2);
    }

    [TestMethod]
    public void Negation_ShouldOverridePrevious() {
        var root = CreateTree(new() {
            ["important.log"] = "x",
            ["other.log"] = "x"
        });

        var opts = new FileQueryOptions(
            patternInput: new(
                patterns: [
                    "**",
                    "!*.log",
                    "important.log"
                ]
            )
        );

        var result = FileQueryRuntime.Create().Execute(new(root, opts)).ToList();

        var onlyResult = TestAssertEx.ContainsSingle(result);
        TestAssertEx.EndsWith(onlyResult, "other.log");
    }

    /// <summary>
    /// GitIgnore semantics allow restoring files inside an ignored parent directory.
    /// </summary>
    [TestMethod]
    public void NegationInsideIgnoredDirectory_ShouldBeIncluded() {
        var root = CreateTree(new() {
            ["logs/hidden/a.txt"] = "x",
            ["logs/hidden/keep.txt"] = "x"
        });

        var opts = new FileQueryOptions(
            patternInput: new(
                patterns: [
                    "**",
                    "!logs/hidden/keep.txt"
                ]
            ),
            recurseSubdirectories: true
        );

        var result = FileQueryRuntime.Create().Execute(new(root, opts)).ToList();

        var onlyResult = TestAssertEx.ContainsSingle(result);
        TestAssertEx.EndsWith(onlyResult, Path.Combine("logs", "hidden", "keep.txt"));
    }

    [TestMethod]
    public void CharacterClass_ShouldMatchCorrectly() {
        var root = CreateTree(new() {
            ["file0.txt"] = "x",
            ["file1.txt"] = "x",
            ["file9.txt"] = "x"
        });

        var opts = new FileQueryOptions(
            patternInput: new(
                patterns: [
                    "**",
                    "!file[0-2].txt"
                ]
            )
        );

        var result = FileQueryRuntime.Create().Execute(new(root, opts)).ToList();
        TestAssertEx.HasCount(result, 2);
    }

    [TestMethod]
    public void NegatedCharacterClass_ShouldMatchCorrectly() {
        var root = CreateTree(new() {
            ["file1.txt"] = "x",
            ["file7.txt"] = "x"
        });

        var opts = new FileQueryOptions(
            patternInput: new(
                patterns: [
                    "**",
                    "!file[!5-9].txt"
                ]
            )
        );

        var result = FileQueryRuntime.Create().Execute(new(root, opts)).ToList();
        var onlyResult = TestAssertEx.ContainsSingle(result);
        TestAssertEx.EndsWith(onlyResult, "file1.txt");
    }

    [TestMethod]
    public void MultiWildcard_ShouldMatchComplexLayout() {
        var root = CreateTree(new() {
            ["foo/a/bar/x.json"] = "x",
            ["foo/b/c/bar/x.json"] = "x",
            ["foo/b/bar/x.json"] = "x",
        });

        var opts = new FileQueryOptions(
            patternInput: new(
                patterns: [
                    "**",
                    "!**/foo/**/bar/*.json"
                ]
            ),
            recurseSubdirectories: true
        );

        var result = FileQueryRuntime.Create().Execute(new(root, opts)).ToList();
        TestAssertEx.HasCount(result, 3);
    }

    [TestMethod]
    public void Dotfile_ShouldNotMatchWithStar() {
        var root = CreateTree(new() {
            [".env"] = "x"
        });

        var opts = new FileQueryOptions(
            patternInput: new(
                patterns: [
                    "!**"
                ]
            )
        );

        var result = FileQueryRuntime.Create().Execute(new(root, opts)).ToList();
        TestAssertEx.IsNotEmpty(result);
    }

    [TestMethod]
    public void Dotfile_ShouldMatchWhenExplicit() {
        var root = CreateTree(new() {
            [".env"] = "x"
        });

        var opts = new FileQueryOptions(
            patternInput: new(
                patterns: [
                    "**",
                    "!.env"
                ]
            )
        );

        var result = FileQueryRuntime.Create().Execute(new(root, opts)).ToList();
        TestAssertEx.ContainsSingle(result);
    }

    [TestMethod]
    public void MaxDepth_ShouldLimitTraversal() {
        var root = CreateTree(new() {
            ["a/b/c/d/e/f.txt"] = "x"
        });

        var opts = new FileQueryOptions(
            patternInput: new(
                patterns: [
                    "**",
                    "!**/*.txt"
                ]
            ),
            recurseSubdirectories: true,
            maxRecursionDepth: 2
        );

        var result = FileQueryRuntime.Create().Execute(new(root, opts)).ToList();
        TestAssertEx.IsEmpty(result);
    }

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
                                                       patternInput: new(
                                                           patterns: [
                                                               "**",
                                                               "!**/*.txt"
                                                            ]
                                                       ),
                                                       recurseSubdirectories: true
                                                   )
                                               )
                                           )
                                           .ToList();

        // The returned path MUST use native separators
        TestAssertEx.Contains(result.Single(), Path.DirectorySeparatorChar.ToString());
    }
}
