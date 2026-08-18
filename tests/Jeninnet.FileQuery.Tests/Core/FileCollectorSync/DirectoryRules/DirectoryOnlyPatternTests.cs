namespace Jeninnet.FileQuery.Tests.Core.FileCollectorSync.DirectoryRules;

/// <summary>
/// Tests directory-only GitIgnore patterns:
/// - "folder/" matches only directories
/// - Does not match files
/// - "*" and "**" interaction
/// </summary>
[TestClass]
public class DirectoryOnlyPatternTests {
    /// <summary>
    /// Directory-only pattern "sub/" should match the directory,
    /// but FileQueryEngine never *returns* directories — only filters by them.
    /// So root/sub/ should be included for recursion decisions,
    /// but no files should be returned unless deeper rules allow it.
    /// </summary>
    [TestMethod]
    public void DirectoryOnly_Inclusion_ShouldNotReturnFilesUnlessAllowed() {
        using var env = new TestEnvironment();

        env.CreateFile("sub/a.txt");
        env.CreateFile("sub/b.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "sub/"
                    ]
                )
            )
        );

        var results = fileQueryEngine.Execute(new(env.Root, options)).ToList();

        // Directory rule matches only the folder, not the files
        TestAssertEx.IsEmpty(results);
    }

    /// <summary>
    /// Directory-only pattern combined with deeper file rule.
    /// </summary>
    [TestMethod]
    public void DirectoryThenWildcard_ShouldReturnFilesInsideMatchedDirectory() {
        using var env = new TestEnvironment();

        env.CreateFile("sub/inside.txt");
        env.CreateFile("sub/other.log");
        env.CreateFile("other/x.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "sub/",
                        "!sub/*.txt"
                    ]
                )
            )
        );

        var results = fileQueryEngine.Execute(new(env.Root, options)).ToList();

        Assert.Contains(static x => x.EndsWith("inside.txt", StringComparison.Ordinal), results);   // Included by negation "!sub/*.txt" rule
        Assert.Contains(static x => x.EndsWith("other.log", StringComparison.Ordinal), results);    // Excluded by default
        TestAssertEx.ContainsSingle(results, static x => x.EndsWith("x.txt", StringComparison.Ordinal));       // Included by default (not in 'sub/')
    }

    /// <summary>
    /// Negated directory rule re-enables access.
    /// </summary>
    [TestMethod]
    public void NegatedDirectoryRule_ShouldRestoreFilesInsideFolder() {
        using var env = new TestEnvironment();

        env.CreateFile("sub/a.txt");
        env.CreateFile("sub/b.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "sub/*",       // exclude folder
                        "!sub/a.txt"    // re-include one file
                    ]
                )
            )
        );

        var results = fileQueryEngine.Execute(new(env.Root, options)).ToList();

        TestAssertEx.ContainsSingle(results, static x => x.EndsWith("a.txt", StringComparison.Ordinal));
    }

    /// <summary>Tests DirectoryOnly_Inclusion_ShouldNotReturnFilesInsideFolder.</summary>
    [TestMethod]
    public void DirectoryOnly_Inclusion_ShouldNotReturnFilesInsideFolder() {
        using var env = new TestEnvironment();

        /*
         * Root directory 'temp' must be excluded (IsNegated=false)
         * File inside root 'temp' must be excluded.
         * ================================================================================
         * Nested Must NOT Match
         * ================================================================================
         * Nested directory 'src/temp' must NOT match anchored /temp/ (Default Excluded).
         * File inside nested 'src/temp' must NOT match anchored /temp/ (Default Excluded).
         */
        env.CreateFile("temp/file.log");
        env.CreateFile("src/temp/file.log");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "/temp/"
                    ]
                )
            )
        );

        var results = fileQueryEngine.Execute(new(env.Root, options)).ToList();

        Assert.DoesNotContain(static x => x.StartsWith("temp", StringComparison.Ordinal), results);
        Assert.Contains(static x => x.EndsWith("file.log", StringComparison.Ordinal), results);
        TestAssertEx.HasCount(results, 1);
    }

    /// <summary>Tests DirectoryOnly_Inclusion_ShouldReturnFilesInsideSubdirectories.</summary>
    [TestMethod]
    public void DirectoryOnly_Inclusion_ShouldReturnFilesInsideSubdirectories() {
        using var env = new TestEnvironment();

        /*
         * Root directory 'temp' must be excluded (IsNegated=false)
         * File inside root 'temp' must be excluded.
         * ================================================================================
         * Nested Must BE Match
         * ================================================================================
         * Nested directory 'src/temp' must be included.
         * File inside nested 'src/temp' must be included.
         */
        env.CreateFile("root-file-1.log");
        env.CreateFile("root-file-2.log");
        env.CreateFile("temp/file.log");
        env.CreateFile("src/temp/file.log");
        env.CreateFile("src/temp/sub/file.log");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "/*",   // Exclude all files in root directory
                        "!*/",  // Re-include all directories (prevents pruning)
                        "!*/*" // Include all files in subdirectories
                    ]
                )
            )
        );

        var results = fileQueryEngine.Execute(new(env.Root, options)).ToList();

        TestAssertEx.Contains(results, env.Abs("temp", "file.log"), "File inside root 'temp' must be excluded.");
        TestAssertEx.Contains(results, env.Abs("src", "temp", "file.log"), "File inside nested 'src/temp' must be included.");
        TestAssertEx.Contains(results, env.Abs("src", "temp", "sub", "file.log"), "File inside nested 'src/temp' must be included.");
        TestAssertEx.HasCount(results, 3);
    }

    /// <summary>Tests DirectoryOnly_LastRuleWins_HierarchyAndSpecificNegation0.</summary>
    [TestMethod]
    public void DirectoryOnly_LastRuleWins_HierarchyAndSpecificNegation0() {
        using var env = new TestEnvironment();

        /*
         * Root directory 'temp' must be excluded (IsNegated=false)
         * File inside root 'temp' must be excluded.
         * ================================================================================
         * Nested Must BE Match
         * ================================================================================
         * Nested directory 'src/temp' must be included.
         * File inside nested 'src/temp' must be included.
         */
        env.CreateFile("sub/inside.txt");
        env.CreateFile("sub/other.log");
        env.CreateFile("sub/x.txt");
        env.CreateFile("tmp/sub/b.txt");

        var fileQueryEngine = FileQueryRuntime.Create();
        var options = new FileQueryOptions(
            new FileQueryOptionsConfig(
                PatternInput: new(
                    Patterns: [
                        "sub/*",        // Ignores all files within the `sub` folder located at the root
                        "!sub/x.txt",   // Excludes the `x.txt` file located within the `sub` folder at the root only
                        "*/sub/*",      // Ignores all files within any subfolder named `sub` (such as `tmp/sub/b.txt`)
                    ]
                )
            )
        );

        var results = fileQueryEngine.Execute(new(env.Root, options)).ToList();

        TestAssertEx.DoesNotContain(results, env.Abs("sub", "inside.txt"), "File 'inside.txt' inside root 'sub' must be excluded.");
        TestAssertEx.DoesNotContain(results, env.Abs("sub", "other.log"), "File 'other.log' inside 'sub' must be excluded.");
        TestAssertEx.Contains(results, env.Abs("sub", "x.txt"), "File 'x.txt' inside 'sub' must be included.");
        TestAssertEx.DoesNotContain(results, env.Abs("tmp", "sub", "x.txt"), "File 'b.txt' inside nested 'tmp/sub' must be excluded by '*/sub/*'.");
    }
}
