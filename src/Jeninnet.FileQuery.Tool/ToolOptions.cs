//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
using System.CommandLine;
using Jeninnet.FileQuery.CommandLine;

namespace Jeninnet.FileQuery.Tool;

/// <summary>
/// Defines the command-line surface of the <c>dotnet-filequery</c> tool.
/// </summary>
internal sealed class ToolOptions : CommandLinePatternOptions {
    private const string OPTION_NAME_ROOT = "--root";
    private const string OPTION_ALIAS_ROOT = "-r";
    private const string OPTION_NAME_DIALECT = "--dialect";
    private const string OPTION_NAME_NO_RECURSIVE = "--no-recursive";
    private const string OPTION_NAME_DEPTH = "--depth";
    private const string OPTION_NAME_IGNORE_CASE = "--ignore-case";
    private const string OPTION_NAME_SYMLINKS = "--symlinks";
    private const string OPTION_NAME_RELATIVE = "--relative";
    private const string OPTION_NAME_NULL = "--null";
    private const string OPTION_ALIAS_NULL = "-0";
    private const string OPTION_NAME_COUNT = "--count";
    private const string OPTION_NAME_CHANGED_SINCE = "--changed-since";
    private const string OPTION_NAME_CHANGED_SINCE_DATE = "--changed-since-date";
    private const string OPTION_NAME_NO_UNTRACKED = "--no-untracked";
    private const string OPTION_NAME_EXPLAIN = "--explain";
    private const string OPTION_NAME_IGNORE_FILES = "--ignore-files";
    private const string OPTION_NAME_GLOBAL_IGNORE = "--global-ignore";
    private const string OPTION_NAME_JSON = "--json";

    /// <summary>
    /// Gets the positional patterns to match. Each value may contain multiple semicolon-separated patterns.
    /// </summary>
    public Argument<string[]> PositionalPatterns { get; }

    /// <summary>
    /// Gets the root directory option.
    /// </summary>
    public Option<string> Root { get; }

    /// <summary>
    /// Gets the dialect option that forces a single pattern language.
    /// </summary>
    public Option<string> Dialect { get; }

    /// <summary>
    /// Gets the option that disables recursion into subdirectories.
    /// </summary>
    public Option<bool> NoRecursive { get; }

    /// <summary>
    /// Gets the maximum recursion depth option.
    /// </summary>
    public Option<int?> Depth { get; }

    /// <summary>
    /// Gets the case-insensitive matching option.
    /// </summary>
    public Option<bool> IgnoreCase { get; }

    /// <summary>
    /// Gets the symbolic link policy option.
    /// </summary>
    public Option<string> Symlinks { get; }

    /// <summary>
    /// Gets the option that emits paths relative to the root directory.
    /// </summary>
    public Option<bool> Relative { get; }

    /// <summary>
    /// Gets the option that separates paths with a NUL character instead of a newline.
    /// </summary>
    public Option<bool> NullSeparated { get; }

    /// <summary>
    /// Gets the option that prints only the number of matching files.
    /// </summary>
    public Option<bool> Count { get; }

    /// <summary>
    /// Gets the option that limits the query to files changed since a Git revision.
    /// </summary>
    public Option<string?> ChangedSince { get; }

    /// <summary>
    /// Gets the option that limits the query to files changed since a timestamp.
    /// </summary>
    public Option<string?> ChangedSinceDate { get; }

    /// <summary>
    /// Gets the option that excludes untracked files from a change-set query.
    /// </summary>
    public Option<bool> NoUntracked { get; }

    /// <summary>
    /// Gets the option that writes per-file include/exclude explanations instead of matches.
    /// </summary>
    public Option<bool> Explain { get; }

    /// <summary>
    /// Gets the option that loads Git ignore files into the query.
    /// </summary>
    public Option<bool> IgnoreFiles { get; }

    /// <summary>
    /// Gets the option that supplies an additional global Git ignore file.
    /// </summary>
    public Option<string?> GlobalIgnore { get; }

    /// <summary>
    /// Gets the option that writes newline-delimited JSON objects with entry metadata.
    /// </summary>
    public Option<bool> JsonOutput { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ToolOptions"/> class.
    /// </summary>
    public ToolOptions() {
        PositionalPatterns = new Argument<string[]>("patterns") {
            Description =
                """
                Patterns to match. Each value may contain multiple patterns separated by ';'.
                Defaults to every file when omitted.
                """,
            Arity = ArgumentArity.ZeroOrMore
        };

        Root = new Option<string>(OPTION_NAME_ROOT, OPTION_ALIAS_ROOT) {
            Description = "Root directory to search. Defaults to the current directory.",
            DefaultValueFactory = static _ => ".",
            Arity = ArgumentArity.ExactlyOne
        };

        Dialect = new Option<string>(OPTION_NAME_DIALECT) {
            Description = "Force one pattern dialect: hybrid (default), gitignore, glob, or regex.",
            DefaultValueFactory = static _ => "hybrid",
            Arity = ArgumentArity.ExactlyOne
        };

        NoRecursive = new Option<bool>(OPTION_NAME_NO_RECURSIVE) {
            Description = "Search only the root directory; do not descend into subdirectories.",
            Arity = ArgumentArity.Zero
        };

        Depth = new Option<int?>(OPTION_NAME_DEPTH) {
            Description = "Maximum recursion depth. 0 searches the root directory only.",
            Arity = ArgumentArity.ExactlyOne
        };

        IgnoreCase = new Option<bool>(OPTION_NAME_IGNORE_CASE) {
            Description = "Match pattern characters without case sensitivity.",
            Arity = ArgumentArity.Zero
        };

        Symlinks = new Option<string>(OPTION_NAME_SYMLINKS) {
            Description = "Symbolic link policy: ignore (default), follow, or follow-with-cycle-detection.",
            DefaultValueFactory = static _ => "ignore",
            Arity = ArgumentArity.ExactlyOne
        };

        Relative = new Option<bool>(OPTION_NAME_RELATIVE) {
            Description = "Write paths relative to the root directory instead of absolute paths.",
            Arity = ArgumentArity.Zero
        };

        NullSeparated = new Option<bool>(OPTION_NAME_NULL, OPTION_ALIAS_NULL) {
            Description = "Separate output paths with a NUL character (for xargs -0 and similar tools).",
            Arity = ArgumentArity.Zero
        };

        Count = new Option<bool>(OPTION_NAME_COUNT) {
            Description = "Write only the number of matching files.",
            Arity = ArgumentArity.Zero
        };

        ChangedSince = new Option<string?>(OPTION_NAME_CHANGED_SINCE) {
            Description = "Limit the query to files changed since a commit, tag, or branch (includes untracked files).",
            DefaultValueFactory = static _ => default,
            Arity = ArgumentArity.ExactlyOne
        };

        ChangedSinceDate = new Option<string?>(OPTION_NAME_CHANGED_SINCE_DATE) {
            Description = "Limit the query to files changed since an ISO 8601 timestamp (uses the last commit before it).",
            DefaultValueFactory = static _ => default,
            Arity = ArgumentArity.ExactlyOne
        };

        NoUntracked = new Option<bool>(OPTION_NAME_NO_UNTRACKED) {
            Description = "Exclude untracked files when a --changed-since baseline is used.",
            Arity = ArgumentArity.Zero
        };

        Explain = new Option<bool>(OPTION_NAME_EXPLAIN) {
            Description = "Write one line per file with its include/exclude outcome and reason instead of matching paths.",
            Arity = ArgumentArity.Zero
        };

        IgnoreFiles = new Option<bool>(OPTION_NAME_IGNORE_FILES) {
            Description = "Load .gitignore files (root and nested), .git/info/exclude, and the optional --global-ignore file.",
            Arity = ArgumentArity.Zero
        };

        GlobalIgnore = new Option<string?>(OPTION_NAME_GLOBAL_IGNORE) {
            Description = "Additional global Git ignore file to load with --ignore-files. Relative paths are resolved against the root.",
            DefaultValueFactory = static _ => default,
            Arity = ArgumentArity.ExactlyOne
        };

        JsonOutput = new Option<bool>(OPTION_NAME_JSON) {
            Description = "Write one JSON object per line with path and metadata; combine with --explain for outcomes and reasons.",
            Arity = ArgumentArity.Zero
        };
    }

    /// <summary>
    /// Returns all supported command-line options, including the inherited pattern options.
    /// </summary>
    /// <returns>The complete list of options accepted by the root command.</returns>
    public override List<Option> GetCommandOptions() =>
        [
            .. base.GetCommandOptions(),
            Root,
            Dialect,
            NoRecursive,
            Depth,
            IgnoreCase,
            Symlinks,
            Relative,
            NullSeparated,
            Count,
            ChangedSince,
            ChangedSinceDate,
            NoUntracked,
            Explain,
            IgnoreFiles,
            GlobalIgnore,
            JsonOutput,
        ];
}
