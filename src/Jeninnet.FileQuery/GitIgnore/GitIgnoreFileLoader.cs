//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
using Jeninnet.FileQuery.Patterns.Exceptions;

namespace Jeninnet.FileQuery.GitIgnore;

/// <summary>
/// Loads Git ignore files and rewrites their rules into root-relative GitIgnore patterns.
/// </summary>
/// <remarks>
/// <para>
/// Precedence follows Git: global excludes, then <c>.git/info/exclude</c>, then the root
/// <c>.gitignore</c>, then nested <c>.gitignore</c> files in parent-before-child order. The
/// engine appends the caller's own patterns after the loaded rules, so explicit query patterns
/// always win.
/// </para>
/// <para>
/// A rule from <c>sub/.gitignore</c> is scoped to <c>sub</c>: rules containing a slash are
/// anchored to that directory, and rules without a slash are allowed at any depth below it.
/// Directory-only and negated rules keep their meaning.
/// </para>
/// </remarks>
internal static class GitIgnoreFileLoader {
    private const string GIT_IGNORE_FILE = ".gitignore";
    private const string GIT_DIRECTORY = ".git";
    private const string INFO_EXCLUDE_RELATIVE_PATH = "info/exclude";

    /// <summary>
    /// Loads all configured ignore files for a query root.
    /// </summary>
    /// <param name="options">The ignore-file loading settings.</param>
    /// <param name="fileSystem">The filesystem abstraction.</param>
    /// <param name="queryRoot">The absolute query root.</param>
    /// <returns>The ordered root-relative GitIgnore patterns.</returns>
    public static IReadOnlyList<string> Load(
        GitIgnoreOptions options,
        IFileSystem fileSystem,
        string queryRoot
    ) {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(fileSystem);
        ArgumentException.ThrowIfNullOrEmpty(queryRoot);

        var patterns = new List<string>();

        if(!string.IsNullOrWhiteSpace(options.GlobalExcludesFile)) {
            var globalPath = ResolvePath(options.GlobalExcludesFile, queryRoot, fileSystem);

            if(globalPath is not null) {
                AddFile(globalPath, baseRelativePath: string.Empty, fileSystem, patterns);
            }
        }

        if(options.IncludeInfoExclude) {
            AddRepositoryExcludes(fileSystem, queryRoot, patterns);
        }

        var rootIgnoreFile = Path.Combine(queryRoot, GIT_IGNORE_FILE);

        if(fileSystem.FileExists(rootIgnoreFile)) {
            AddFile(rootIgnoreFile, baseRelativePath: string.Empty, fileSystem, patterns);
        }

        if(options.IncludeNestedFiles) {
            AddNestedFiles(fileSystem, queryRoot, patterns);
        }

        return patterns;
    }

    /// <summary>
    /// Enumerates subdirectories breadth-first and loads each <c>.gitignore</c>.
    /// </summary>
    /// <param name="fileSystem">The filesystem abstraction.</param>
    /// <param name="queryRoot">The absolute query root.</param>
    /// <param name="patterns">The target pattern list.</param>
    private static void AddNestedFiles(IFileSystem fileSystem, string queryRoot, List<string> patterns) {
        var pending = new Queue<(string Directory, string RelativePath)>();
        var matcher = CompileMatcher(patterns);
        pending.Enqueue((queryRoot, string.Empty));

        while(pending.Count > 0) {
            var (directory, relativePath) = pending.Dequeue();

            foreach(var entry in fileSystem.Enumerate(
                directory,
                ignoreInaccessible: true,
                FileQueryErrorRecoveryOptions.Skip)) {
                if(!entry.IsDirectory || entry.IsReparsePoint) {
                    continue;
                }

                var name = Path.GetFileName(entry.FullPath);

                if(string.Equals(name, GIT_DIRECTORY, StringComparison.Ordinal)) {
                    continue;
                }

                var childRelativePath = relativePath.Length == 0
                    ? name
                    : $"{relativePath}/{name}";

                // Git does not read nested ignore files (and does not descend) inside an
                // ignored directory. Evaluating the directory before loading its own rules
                // reproduces that behavior.
                if(matcher?.IsMatch($"{childRelativePath}/") == false) {
                    continue;
                }

                var ignoreFile = Path.Combine(entry.FullPath, GIT_IGNORE_FILE);

                if(fileSystem.FileExists(ignoreFile)) {
                    var previousCount = patterns.Count;
                    AddFile(ignoreFile, childRelativePath, fileSystem, patterns);

                    if(patterns.Count != previousCount) {
                        matcher = CompileMatcher(patterns);
                    }
                }

                pending.Enqueue((entry.FullPath, childRelativePath));
            }
        }
    }

    /// <summary>
    /// Compiles the rules collected so far for directory pruning.
    /// </summary>
    /// <param name="patterns">The GitIgnore patterns collected so far.</param>
    /// <returns>The compiled matcher, or <see langword="null"/> when there are no rules.</returns>
    private static FileQueryPatternSet? CompileMatcher(List<string> patterns) {
        if(patterns.Count == 0) {
            return null;
        }

        var typedPatterns = new Dictionary<PatternKind, IEnumerable<string>> {
            [PatternKind.GitIgnore] = patterns
        };

        return FileQueryPatternSet.Compile(
            new PatternInput(TypedPatterns: typedPatterns),
            PatternMatchingMode.GitIgnore);
    }

    /// <summary>
    /// Loads <c>.git/info/exclude</c> from the containing repository, when the query root is
    /// the repository root. Rules are repository-root relative, so a query rooted deeper only
    /// receives the unanchored rules that can still apply below it.
    /// </summary>
    /// <param name="fileSystem">The filesystem abstraction.</param>
    /// <param name="queryRoot">The absolute query root.</param>
    /// <param name="patterns">The target pattern list.</param>
    private static void AddRepositoryExcludes(IFileSystem fileSystem, string queryRoot, List<string> patterns) {
        var repository = FindRepository(fileSystem, queryRoot);

        if(repository is null) {
            return;
        }

        var excludeFile = Path.Combine(repository.Value.GitDirectory, INFO_EXCLUDE_RELATIVE_PATH.Replace('/', Path.DirectorySeparatorChar));

        if(!fileSystem.FileExists(excludeFile)) {
            return;
        }

        var relativeRoot = Path.GetRelativePath(repository.Value.WorkingDirectory, queryRoot)
            .Replace('\\', '/');

        if(relativeRoot is ".") {
            relativeRoot = string.Empty;
        }

        if(Path.IsPathRooted(relativeRoot) || relativeRoot.StartsWith("../", StringComparison.Ordinal)) {
            return;
        }

        AddRepositoryFile(excludeFile, relativeRoot, fileSystem, patterns);
    }

    /// <summary>
    /// Finds the Git working tree that contains the query root.
    /// </summary>
    /// <param name="fileSystem">The filesystem abstraction.</param>
    /// <param name="queryRoot">The absolute query root.</param>
    /// <returns>The working directory and Git directory, or <see langword="null"/> when not in a repository.</returns>
    private static (string WorkingDirectory, string GitDirectory)? FindRepository(IFileSystem fileSystem, string queryRoot) {
        var current = queryRoot;

        while(!string.IsNullOrEmpty(current)) {
            var gitPath = Path.Combine(current, GIT_DIRECTORY);

            if(fileSystem.DirectoryExists(gitPath)) {
                return (current, gitPath);
            }

            if(fileSystem.FileExists(gitPath)) {
                var gitDirectory = ReadWorktreeGitDirectory(gameFile: gitPath, workingDirectory: current, fileSystem);

                if(gitDirectory is not null) {
                    return (current, gitDirectory);
                }
            }

            var parent = Path.GetDirectoryName(current);

            if(string.IsNullOrEmpty(parent) || string.Equals(parent, current, StringComparison.Ordinal)) {
                break;
            }

            current = parent;
        }

        return null;
    }

    /// <summary>
    /// Resolves the <c>gitdir:</c> pointer used by Git worktrees.
    /// </summary>
    /// <param name="gameFile">The path of the <c>.git</c> file.</param>
    /// <param name="workingDirectory">The directory containing the <c>.git</c> file.</param>
    /// <param name="fileSystem">The filesystem abstraction.</param>
    /// <returns>The absolute Git directory, or <see langword="null"/> when the pointer is missing.</returns>
    private static string? ReadWorktreeGitDirectory(string gameFile, string workingDirectory, IFileSystem fileSystem) {
        string[] lines;

        try {
            lines = fileSystem.ReadAllLines(gameFile);
        }
        catch(IOException) {
            return null;
        }
        catch(UnauthorizedAccessException) {
            return null;
        }

        if(lines.Length == 0) {
            return null;
        }

        const string prefix = "gitdir:";

        if(!lines[0].StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) {
            return null;
        }

        var target = lines[0][prefix.Length..].Trim();

        if(target.Length == 0) {
            return null;
        }

        try {
            return Path.IsPathRooted(target)
                ? target
                : fileSystem.GetFullPath(target, workingDirectory);
        }
        catch(ArgumentException) {
            return null;
        }
    }

    /// <summary>
    /// Adds a repository-root-relative ignore file, rebasing anchored rules onto the query root.
    /// </summary>
    /// <param name="path">The ignore file path.</param>
    /// <param name="relativeRoot">The query root relative to the repository root.</param>
    /// <param name="fileSystem">The filesystem abstraction.</param>
    /// <param name="patterns">The target pattern list.</param>
    private static void AddRepositoryFile(
        string path,
        string relativeRoot,
        IFileSystem fileSystem,
        List<string> patterns
    ) {
        foreach(var line in ReadPatternLines(path, fileSystem)) {
            var rebased = RewriteRepositoryRule(line, relativeRoot);

            if(rebased is not null) {
                patterns.Add(rebased);
            }
        }
    }

    /// <summary>
    /// Reads and normalizes the meaningful lines of an ignore file.
    /// </summary>
    /// <param name="path">The ignore file path.</param>
    /// <param name="fileSystem">The filesystem abstraction.</param>
    /// <returns>The pattern lines in source order.</returns>
    private static IEnumerable<string> ReadPatternLines(string path, IFileSystem fileSystem) {
        string[] lines;

        try {
            lines = fileSystem.ReadAllLines(path);
        }
        catch(IOException) {
            yield break;
        }
        catch(UnauthorizedAccessException) {
            yield break;
        }

        foreach(var rawLine in lines) {
            var pattern = ParseLine(rawLine);

            if(pattern is not null) {
                yield return pattern;
            }
        }
    }

    /// <summary>
    /// Adds a single ignore file, scoping its rules to <paramref name="baseRelativePath"/>.
    /// </summary>
    /// <param name="path">The ignore file path.</param>
    /// <param name="baseRelativePath">The directory containing the file, relative to the query root.</param>
    /// <param name="fileSystem">The filesystem abstraction.</param>
    /// <param name="patterns">The target pattern list.</param>
    private static void AddFile(
        string path,
        string baseRelativePath,
        IFileSystem fileSystem,
        List<string> patterns
    ) {
        foreach(var line in ReadPatternLines(path, fileSystem)) {
            var scoped = RewriteForBase(line, baseRelativePath);

            if(scoped is not null && IsSupportedRule(scoped)) {
                patterns.Add(scoped);
            }
        }
    }

    /// <summary>
    /// Determines whether a GitIgnore rule can be compiled by the engine.
    /// Ignore files in the wild sometimes contain patterns our stricter validator
    /// rejects; dropping the unsupported line is preferable to failing the whole query.
    /// </summary>
    /// <param name="rule">The root-relative rule.</param>
    /// <returns><see langword="true"/> when the rule compiles.</returns>
    private static bool IsSupportedRule(string rule) {
        try {
            _ = FileQueryPatternSet.Compile(
                new PatternInput(
                    TypedPatterns: new Dictionary<PatternKind, IEnumerable<string>> {
                        [PatternKind.GitIgnore] = [rule]
                    },
                    InterpretationMode: PatternInterpretationMode.Specific),
                PatternMatchingMode.GitIgnore);

            return true;
        }
        catch(PatternException) {
            return false;
        }
    }

    /// <summary>
    /// Removes comments and insignificant trailing whitespace from a raw ignore-file line.
    /// </summary>
    /// <param name="line">The raw line, without its line ending.</param>
    /// <returns>The pattern, or <see langword="null"/> when the line is blank or a comment.</returns>
    private static string? ParseLine(string line) {
        if(line.Length > 0 && line[0] == '\uFEFF') {
            line = line[1..];
        }

        line = line.TrimEnd('\r');

        if(line.Length == 0 || line[0] == '#') {
            return null;
        }

        var end = line.Length;

        while(end > 0 && line[end - 1] == ' ') {
            var backslashes = 0;

            for(var i = end - 2; i >= 0 && line[i] == '\\'; i--) {
                backslashes++;
            }

            if(backslashes % 2 != 0) {
                break;
            }

            end--;
        }

        return end == 0 ? null : line[..end];
    }

    /// <summary>
    /// Scopes a rule from a nested ignore file to the query root.
    /// </summary>
    /// <param name="pattern">The raw rule.</param>
    /// <param name="baseRelativePath">The directory containing the ignore file, relative to the query root.</param>
    /// <returns>The scoped rule, or <see langword="null"/> when the rule is empty.</returns>
    private static string? RewriteForBase(string pattern, string baseRelativePath) {
        if(baseRelativePath.Length == 0) {
            return pattern;
        }

        var negated = pattern[0] == '!';
        var body = negated ? pattern[1..] : pattern;

        if(body.Length == 0) {
            return null;
        }

        var isDirectoryOnly = body[^1] == '/';
        var wasRootAnchored = body[0] == '/';
        var content = wasRootAnchored ? body[1..] : body;

        if(content.Length == 0) {
            return null;
        }

        var contentWithoutTrailingSlash = isDirectoryOnly ? content[..^1] : content;
        var containsSlash = contentWithoutTrailingSlash.Contains('/', StringComparison.Ordinal);

        var unanchoredSuffix = string.Equals(content, "**", StringComparison.Ordinal)
            ? "**"
            : $"**/{content}";

        var scoped = wasRootAnchored || containsSlash
            ? $"/{baseRelativePath}/{content}"
            : $"/{baseRelativePath}/{unanchoredSuffix}";

        return negated ? $"!{scoped}" : scoped;
    }

    /// <summary>
    /// Determines whether a rule target is a proper ancestor directory of the query root.
    /// </summary>
    /// <param name="candidate">The rule target, without a trailing slash.</param>
    /// <param name="relativeRoot">The query root relative to the repository root.</param>
    /// <param name="comparison">The path comparison mode.</param>
    /// <returns><see langword="true"/> when the rule targets an ancestor of the query root.</returns>
    private static bool IsAncestorRule(
        string candidate,
        string relativeRoot,
        StringComparison comparison
    ) =>
        candidate.Length > 0 &&
        relativeRoot.Length > candidate.Length &&
        relativeRoot.StartsWith(candidate, comparison) &&
        relativeRoot[candidate.Length] == '/';

    /// <summary>
    /// Rebases a repository-root-relative rule onto a query root that may be deeper.
    /// </summary>
    /// <param name="pattern">The repository-relative rule.</param>
    /// <param name="relativeRoot">The query root relative to the repository root.</param>
    /// <returns>The rebased rule, or <see langword="null"/> when it cannot affect the query root.</returns>
    private static string? RewriteRepositoryRule(string pattern, string relativeRoot) {
        if(relativeRoot.Length == 0) {
            return pattern;
        }

        var negated = pattern[0] == '!';
        var body = negated ? pattern[1..] : pattern;

        if(body.Length == 0) {
            return null;
        }

        var wasRootAnchored = body[0] == '/';
        var content = wasRootAnchored ? body[1..] : body;
        var isDirectoryOnly = content.Length > 0 && content[^1] == '/';
        var contentWithoutTrailingSlash = isDirectoryOnly ? content[..^1] : content;
        var containsSlash = contentWithoutTrailingSlash.Contains('/', StringComparison.Ordinal);

        var comparison = OperatingSystem.IsLinux()
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;

        if(!wasRootAnchored && !containsSlash) {
            if(IsAncestorRule(contentWithoutTrailingSlash, relativeRoot, comparison)) {
                return negated ? null : "/**";
            }

            return pattern;
        }

        if(string.Equals(contentWithoutTrailingSlash, relativeRoot, comparison)) {
            // The rule targets the query root itself; every entry below it is affected.
            return negated ? "!/**" : "/**";
        }

        if(IsAncestorRule(contentWithoutTrailingSlash, relativeRoot, comparison)) {
            // The rule targets an ancestor directory of the query root. Git cannot
            // re-include below an excluded parent, so only a positive rule applies:
            // it excludes the entire query root.
            return negated ? null : "/**";
        }

        var prefix = $"{relativeRoot}/";

        if(!content.StartsWith(prefix, comparison)) {
            return null;
        }

        var remainder = content[prefix.Length..];
        return negated ? $"!/{remainder}" : $"/{remainder}";
    }

    /// <summary>
    /// Resolves an optional path against the query root.
    /// </summary>
    /// <param name="path">The configured path.</param>
    /// <param name="queryRoot">The absolute query root.</param>
    /// <param name="fileSystem">The filesystem abstraction.</param>
    /// <returns>The absolute path, or <see langword="null"/> when it cannot be resolved.</returns>
    private static string? ResolvePath(string path, string queryRoot, IFileSystem fileSystem) {
        try {
            return fileSystem.GetFullPath(path, queryRoot);
        }
        catch(ArgumentException) {
            return null;
        }
        catch(NotSupportedException) {
            return null;
        }
    }
}
