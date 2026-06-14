namespace Jeninnet.FileQuery;

/// <summary>
/// Provides a fluent API for configuring file system queries.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="FileQueryBuilder"/> represents a mutable <strong>configuration pipeline</strong>.
/// Each method updates and returns the same builder instance.
/// Call <see cref="Build"/> to produce the immutable <see cref="FileQuery"/> descriptor.
/// </para>
/// <para>
/// <strong>Thread safety:</strong> A single builder instance must not be shared across threads.
/// </para>
/// </remarks>
public sealed class FileQueryBuilder
{
    private IFileQueryEngine? _engine;
    private readonly IFileSystem _fileSystem;
    private readonly string _rootPath;
    private readonly Dictionary<PatternKind, HashSet<string>> _patternStorage = [];
    private PatternInterpretationMode _interpretationMode = PatternInterpretationMode.Hybrid;
    private PatternMatchingMode _patternMatchingMode = PatternMatchingMode.GitIgnore;
    private CaseSensitivity _caseSensitivity = CaseSensitivity.PlatformDefault;
    private bool _recurse = true;
    private bool _auditMatches;
    private IProgress<FileQueryDiagnostic>? _diagnostics;
    private FileQueryErrorRecoveryOptions? _errorRecovery;

    /// <summary>
    /// Initializes a new <see cref="FileQueryBuilder"/> rooted at the specified directory.
    /// </summary>
    /// <param name="rootPath">
    /// The root directory from which traversal begins. Must not be null or empty.
    /// </param>
    /// <param name="fileSystem">
    /// The file system abstraction.
    /// </param>
    /// <param name="engine">
    /// An optional engine instance. When <see langword="null"/>, the default engine is
    /// created lazily by <see cref="Execute()"/> or <see cref="ExecuteAsync(CancellationToken)"/>.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="rootPath"/> is null or empty.
    /// </exception>
    internal FileQueryBuilder(string rootPath, IFileSystem fileSystem, IFileQueryEngine? engine = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(rootPath);
        ArgumentNullException.ThrowIfNull(fileSystem);

        _rootPath = rootPath;
        _fileSystem = fileSystem;
        _engine = engine;
    }

    /// <summary>
    /// Creates a new <see cref="FileQueryBuilder"/> rooted at the specified directory using
    /// the default <see cref="IFileSystem"/>.
    /// </summary>
    /// <param name="rootPath">The absolute path of the root directory.</param>
    /// <returns>A builder instance used to configure the query.</returns>
    public static FileQueryBuilder From(string rootPath) => new(rootPath, FileSystem.Instance);

    /// <summary>
    /// Adds one or more untyped patterns to the query.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Patterns are classified automatically using <see cref="PatternClassifier"/>
    /// when the query is executed in <see cref="PatternInterpretationMode.Hybrid"/> mode.
    /// </para>
    /// <para>
    /// Duplicate patterns (those already present in the builder) are ignored.
    /// </para>
    /// </remarks>
    /// <param name="patterns">
    /// A sequence of pattern strings. May include GitIgnore, Glob, or Regex prefixed patterns.
    /// </param>
    /// <returns>The current <see cref="FileQueryBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="patterns"/> is <see langword="null"/>.
    /// </exception>
    public FileQueryBuilder Where(params IEnumerable<string> patterns)
    {
        ArgumentNullException.ThrowIfNull(patterns);

        foreach(var pattern in patterns)
        {
            var kind = PatternClassifier.Classify(pattern);
            MergeIntoTypedBucket(kind, [pattern]);
        }

        return this;
    }

    /// <summary>
    /// Adds one or more patterns explicitly associated with a specific <see cref="PatternKind"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This overload bypasses automatic pattern classification and forces explicit semantics.
    /// Use this when you know the pattern dialect ahead of time and want to avoid classification overhead.
    /// </para>
    /// <para>
    /// Duplicate patterns within the same <paramref name="patternKind"/> bucket are ignored.
    /// </para>
    /// </remarks>
    /// <param name="patternKind">The semantic type of the supplied patterns. Must not be <see cref="PatternKind.Unknown"/>.</param>
    /// <param name="patterns">A sequence of pattern strings.</param>
    /// <returns>The current <see cref="FileQueryBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="patterns"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="patternKind"/> is <see cref="PatternKind.Unknown"/>.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <paramref name="patternKind"/> is incompatible with the current <see cref="PatternMatchingMode"/>.
    /// </exception>
    public FileQueryBuilder Where(PatternKind patternKind, IEnumerable<string> patterns)
    {
        ArgumentNullException.ThrowIfNull(patterns);

        if(patternKind is PatternKind.Unknown)
        {
            throw new ArgumentException(
                "Pattern type cannot be Unknown when adding typed patterns.",
                nameof(patternKind));
        }

        ValidatePatternType(patternKind);
        MergeIntoTypedBucket(patternKind, patterns);
        return this;
    }

    /// <summary>
    /// Adds multiple patterns grouped by pattern type to the query.
    /// </summary>
    /// <remarks>
    /// <para>
    /// All entries in <paramref name="typedPattern"/> are processed.
    /// For each key that already exists in the builder, new patterns are appended
    /// (duplicates within that bucket are skipped).
    /// </para>
    /// <para>
    /// Every dictionary entry is processed. Duplicate patterns within an existing bucket
    /// are skipped by the builder's merge logic.
    /// </para>
    /// </remarks>
    /// <param name="typedPattern">
    /// A dictionary mapping each <see cref="PatternKind"/> to the patterns for that dialect.
    /// No entry may use <see cref="PatternKind.Unknown"/> as a key.
    /// </param>
    /// <returns>The current <see cref="FileQueryBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="typedPattern"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when any key is <see cref="PatternKind.Unknown"/>.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when any key is incompatible with the current <see cref="PatternMatchingMode"/>.
    /// </exception>
    public FileQueryBuilder Where(Dictionary<PatternKind, List<string>> typedPattern)
    {
        ArgumentNullException.ThrowIfNull(typedPattern);

        foreach(var (patternKind, patterns) in typedPattern)
        {
            if(patternKind is PatternKind.Unknown)
            {
                throw new ArgumentException(
                    "Pattern type cannot be Unknown when adding typed patterns.",
                    nameof(typedPattern));
            }

            ValidatePatternType(patternKind);

            // Add only missing items
            MergeIntoTypedBucket(patternKind, patterns);
        }

        return this;
    }

    /// <summary>
    /// Parses and adds multiple patterns from a single delimited string.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This overload is intended for simple scenarios such as CLI argument parsing.
    /// Splitting uses a fixed separator and applies <see cref="StringSplitOptions.RemoveEmptyEntries"/>.
    /// </para>
    /// <para>
    /// For advanced parsing (escaping, quoting, user input), split the string externally
    /// and call <see cref="Where(IEnumerable{string})"/> directly.
    /// </para>
    /// </remarks>
    /// <param name="delimitedPatterns">A semicolon-delimited (or custom-separated) pattern string. Must not be null or empty.</param>
    /// <param name="separator">The separator character. Defaults to <c>';'</c>.</param>
    /// <returns>The current <see cref="FileQueryBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="delimitedPatterns"/> is null or empty.</exception>
    public FileQueryBuilder Where(string delimitedPatterns, char separator = ';')
    {
        ArgumentException.ThrowIfNullOrEmpty(delimitedPatterns);
        return Where(
            delimitedPatterns
                .Split(separator, StringSplitOptions.RemoveEmptyEntries)
                .Select(static p => p.Trim())
        );
    }

    /// <summary>
    /// Configures the query to interpret patterns using <c>Hybrid</c> semantics
    /// (auto-detect GitIgnore, Glob, and Regex patterns).
    /// </summary>
    /// <returns>The current <see cref="FileQueryBuilder"/> instance for method chaining.</returns>
    public FileQueryBuilder UsingHybrid()
    {
        _interpretationMode = PatternInterpretationMode.Hybrid;
        return this;
    }

    /// <summary>
    /// Configures the query to interpret all patterns using <c>GitIgnore</c> semantics.
    /// </summary>
    /// <returns>The current <see cref="FileQueryBuilder"/> instance for method chaining.</returns>
    public FileQueryBuilder UsingGitIgnore()
    {
        _interpretationMode = PatternInterpretationMode.Specific;
        _patternMatchingMode = PatternMatchingMode.GitIgnore;
        return this;
    }

    /// <summary>
    /// Configures the query to interpret all patterns using <c>Glob</c> semantics.
    /// </summary>
    /// <returns>The current <see cref="FileQueryBuilder"/> instance for method chaining.</returns>
    public FileQueryBuilder UsingGlob()
    {
        _interpretationMode = PatternInterpretationMode.Specific;
        _patternMatchingMode = PatternMatchingMode.Glob;
        return this;
    }

    /// <summary>
    /// Configures the query to interpret all patterns using <c>Regex</c> semantics.
    /// </summary>
    /// <returns>The current <see cref="FileQueryBuilder"/> instance for method chaining.</returns>
    public FileQueryBuilder UsingRegex()
    {
        _interpretationMode = PatternInterpretationMode.Specific;
        _patternMatchingMode = PatternMatchingMode.Regex;
        return this;
    }

    // =====Traversal configuration =====

    /// <summary>
    /// Controls whether subdirectories are traversed during execution.
    /// </summary>
    /// <param name="recurse">
    /// <see langword="true"/> to recurse into subdirectories (default); <see langword="false"/> to restrict to the root only.
    /// </param>
    /// <returns>The current <see cref="FileQueryBuilder"/> instance for method chaining.</returns>
    public FileQueryBuilder WithRecursion(bool recurse = true)
    {
        _recurse = recurse;
        return this;
    }

    /// <summary>
    /// Restricts traversal to the root directory only. Equivalent to <c>WithRecursion(false)</c>.
    /// </summary>
    /// <returns>The current <see cref="FileQueryBuilder"/> instance for method chaining.</returns>
    public FileQueryBuilder WithoutRecursion()
    {
        _recurse = false;
        return this;
    }

    /// <summary>
    /// Controls how character casing is handled during pattern matching.
    /// </summary>
    /// <param name="ignoreCase">
    /// <see langword="true"/> to use case-insensitive matching;
    /// <see langword="false"/> for case-sensitive matching.
    /// </param>
    /// <returns>The current <see cref="FileQueryBuilder"/> instance for method chaining.</returns>
    public FileQueryBuilder IgnoreCase(bool ignoreCase = true)
    {
        _caseSensitivity = ignoreCase
            ? CaseSensitivity.Insensitive
            : CaseSensitivity.Sensitive;
        return this;
    }

    /// <summary>
    /// Enables optional match diagnostics for each evaluated filesystem entry.
    /// </summary>
    /// <param name="diagnostics">The diagnostic sink that receives audit entries.</param>
    /// <returns>The current <see cref="FileQueryBuilder"/> instance for method chaining.</returns>
    public FileQueryBuilder WithDiagnostics(IProgress<FileQueryDiagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);

        _auditMatches = true;
        _diagnostics = diagnostics;
        return this;
    }

    /// <summary>
    /// Configures how traversal recovers from IO errors.
    /// </summary>
    /// <param name="errorRecovery">The IO error recovery policy.</param>
    /// <returns>The current <see cref="FileQueryBuilder"/> instance for method chaining.</returns>
    public FileQueryBuilder WithErrorRecovery(FileQueryErrorRecoveryOptions errorRecovery)
    {
        ArgumentNullException.ThrowIfNull(errorRecovery);

        _errorRecovery = errorRecovery;
        return this;
    }

    // =====Terminal operations =====

    /// <summary>
    /// Executes the query synchronously using the configured or default engine.
    /// </summary>
    /// <returns>A lazy enumerable of matching absolute file paths.</returns>
    public IEnumerable<string> Execute()
        => GetEngine().Execute(Build());

    /// <summary>
    /// Executes the query asynchronously using the configured or default engine.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the async enumeration.</param>
    /// <returns>An async enumerable of matching absolute file paths.</returns>
    public IAsyncEnumerable<string> ExecuteAsync(CancellationToken cancellationToken = default)
        => GetEngine().ExecuteAsync(Build(), cancellationToken);

    /// <summary>
    /// Executes the query asynchronously using the configured or default engine and reports traversal progress.
    /// </summary>
    /// <param name="progress">The progress sink that receives traversal snapshots.</param>
    /// <param name="cancellationToken">A token to cancel the async enumeration.</param>
    /// <returns>An async enumerable of matching absolute file paths.</returns>
    public IAsyncEnumerable<string> ExecuteAsync(
        IProgress<FileQueryProgress> progress,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(progress);

        return GetEngine().ExecuteAsync(Build(), progress, cancellationToken);
    }

    /// <summary>
    /// Builds the immutable <see cref="FileQuery"/> descriptor from the current configuration.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method validates the configuration and throws if it is inconsistent.
    /// The returned <see cref="FileQuery"/> instance contains no execution logic — it
    /// is a pure data object passed to <see cref="IFileQueryEngine.Execute"/>.
    /// </para>
    /// </remarks>
    /// <returns>An immutable <see cref="FileQuery"/> ready for execution.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the root path is null or whitespace.
    /// </exception>
    public FileQuery Build()
    {
        if(string.IsNullOrWhiteSpace(_rootPath))
        {
            throw new InvalidOperationException("Root path must be specified.");
        }

        if(!_fileSystem.DirectoryExists(_rootPath))
        {
            throw new DirectoryNotFoundException($"The specified root path does not exist: '{_rootPath}'");
        }

        var patternInput = new PatternInput(
            patterns: [],
            typedPatterns: _patternStorage.ToDictionary(
                kvp => kvp.Key,
                kvp => (IEnumerable<string>)kvp.Value),
            interpretationMode: _interpretationMode
        );

        var options = new FileQueryOptions(
            patternInput: patternInput,
            recurseSubdirectories: _recurse,
            ignoreInaccessible: true,
            patternMatchingMode: _patternMatchingMode,
            caseSensitivity: _caseSensitivity,
            auditMatches: _auditMatches,
            diagnostics: _diagnostics,
            errorRecovery: _errorRecovery
        );

        options.Validate();

        return new FileQuery(_rootPath, options);
    }

    // ===== Private helpers =====

    private IFileQueryEngine GetEngine()
        => _engine ??= FileQueryRuntime.Create();

    /// <summary>
    /// Merges <paramref name="patterns"/> into the typed bucket for <paramref name="patternKind"/>,
    /// skipping duplicates that are already present.
    /// </summary>
    /// <param name="patternKind">The semantic type of the pattern.</param>
    /// <param name="patterns">A sequence of patterns to merge.</param>
    private void MergeIntoTypedBucket(PatternKind patternKind, IEnumerable<string> patterns)
    {
        if(!_patternStorage.TryGetValue(patternKind, out var bucket))
        {
            _patternStorage[patternKind] = [.. patterns];
            return;
        }

        foreach(var pattern in patterns)
        {
            bucket.Add(pattern);
        }
    }

    /// <summary>
    /// Validates that <paramref name="patternKind"/> is compatible with the current
    /// <see cref="PatternMatchingMode"/> when <see cref="PatternInterpretationMode.Specific"/> is active.
    /// </summary>
    /// <param name="patternKind">The pattern type to validate.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the combination of <paramref name="patternKind"/> and
    /// <see cref="_patternMatchingMode"/> is invalid.
    /// </exception>
    private void ValidatePatternType(PatternKind patternKind)
    {
        if(_interpretationMode != PatternInterpretationMode.Specific)
        {
            return;
        }

        var conflict = (_patternMatchingMode, patternKind) switch
        {
            (PatternMatchingMode.Glob, PatternKind.GitIgnore) => true,
            (PatternMatchingMode.Glob, PatternKind.Regex) => true,
            (PatternMatchingMode.GitIgnore, PatternKind.Glob) => true,
            (PatternMatchingMode.GitIgnore, PatternKind.Regex) => true,
            (PatternMatchingMode.Regex, PatternKind.GitIgnore) => true,
            (PatternMatchingMode.Regex, PatternKind.Glob) => true,
            _ => false
        };

        if(conflict)
        {
            throw new InvalidOperationException(
                $"Cannot add '{patternKind}' patterns when 'PatternMatchingMode' is set to '{_patternMatchingMode}'.");
        }
    }
}
