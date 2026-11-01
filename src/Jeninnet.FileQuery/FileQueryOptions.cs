//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery;

/// <summary>
/// Configuration for creating a <see cref="FileQueryOptions"/>.
/// </summary>
/// <param name="PatternInput">The pattern input that defines which files to include or exclude.</param>
/// <param name="RecurseSubdirectories">Whether to recurse into subdirectories.</param>
/// <param name="MaxRecursionDepth">The maximum depth for recursion.</param>
/// <param name="IgnoreInaccessible">Whether to ignore inaccessible files and directories.</param>
/// <param name="PatternMatchingMode">The mode for pattern matching.</param>
/// <param name="CaseSensitivity">The case sensitivity for pattern matching.</param>
/// <param name="Traversal">The traversal options.</param>
/// <param name="AuditMatches">Whether to audit matches.</param>
/// <param name="Diagnostics">The diagnostics progress reporter.</param>
/// <param name="ErrorRecovery">The error recovery options.</param>
/// <param name="MinSize">The inclusive minimum entry size in bytes, or <see langword="null"/> for no lower bound.</param>
/// <param name="MaxSize">The inclusive maximum entry size in bytes, or <see langword="null"/> for no upper bound.</param>
/// <param name="ModifiedAfter">The exclusive lower last-write boundary, or <see langword="null"/> for no lower bound.</param>
/// <param name="ModifiedBefore">The exclusive upper last-write boundary, or <see langword="null"/> for no upper bound.</param>
/// <param name="AttributesToSkip">Attribute flags that exclude an entry (and prune a matching directory).</param>
/// <param name="GitIgnoreFiles">The optional Git ignore-file loading settings.</param>
/// <param name="IncludeDirectories">Whether included directories are yielded alongside files.</param>
public record FileQueryOptionsConfig(
    PatternInput PatternInput,
    bool RecurseSubdirectories = true,
    int MaxRecursionDepth = FileQueryOptions.UNLIMITED_RECURSION_DEPTH,
    bool IgnoreInaccessible = true,
    PatternMatchingMode PatternMatchingMode = PatternMatchingMode.GitIgnore,
    CaseSensitivity CaseSensitivity = CaseSensitivity.PlatformDefault,
    TraversalOptions? Traversal = null,
    bool AuditMatches = false,
    IProgress<FileQueryDiagnostic>? Diagnostics = null,
    FileQueryErrorRecoveryOptions? ErrorRecovery = null,
    long? MinSize = null,
    long? MaxSize = null,
    DateTimeOffset? ModifiedAfter = null,
    DateTimeOffset? ModifiedBefore = null,
    FileAttributes AttributesToSkip = FileAttributes.None,
    GitIgnoreOptions? GitIgnoreFiles = null,
    bool IncludeDirectories = false
) {
    /// <summary>
    /// Gets the optional set of root-relative file paths that limits the query to an explicit
    /// allow-list (for example, the files changed in a diff).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Paths use forward slashes and are relative to the query root. Absolute paths, parent
    /// segments (<c>..</c>), empty entries, and directory entries are rejected when the query
    /// is built.
    /// </para>
    /// <para>
    /// When the collection is <see langword="null"/>, traversal is not restricted. An empty
    /// collection matches no files. Directories that cannot contain an allowed path are pruned;
    /// files that are not listed are not matched at all.
    /// </para>
    /// </remarks>
    public IReadOnlyCollection<string>? IncludedPaths { get; init; }
}

/// <summary>
/// Represents the complete, immutable configuration for a single file query execution.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="FileQueryOptions"/> is a <strong>configuration snapshot</strong>:
/// once passed to the engine, its values are assumed to remain stable for the
/// lifetime of the query.
/// </para>
/// <para>
/// <strong>Immutability contract:</strong> This type is designed to be <em>logically immutable</em>.
/// All properties should be set during construction (typically via <see cref="FileQueryBuilder"/>)
/// and must not be mutated during traversal or matching. The engine, pattern compiler, and
/// matchers rely on this stability to ensure deterministic behavior, caching safety, and
/// thread safety.
/// </para>
/// <para>
/// <strong>Mutation boundary:</strong> Mutation is expected to occur only in a higher-level
/// construction mechanism such as <see cref="FileQueryBuilder"/>. Once execution begins,
/// this instance acts as a read-only execution contract.
/// </para>
/// </remarks>
public sealed record FileQueryOptions {
    /// <summary>
    /// Represents an unlimited recursion depth.
    /// Pass this value to <see cref="MaxRecursionDepth"/> to allow unbounded traversal.
    /// </summary>
    public const int UNLIMITED_RECURSION_DEPTH = -1;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileQueryOptions"/> class with the specified settings.
    /// </summary>
    /// <param name="config">The configuration object.</param>
    public FileQueryOptions(FileQueryOptionsConfig config) {
        ArgumentNullException.ThrowIfNull(config);

        PatternInput = config.PatternInput;
        RecurseSubdirectories = config.RecurseSubdirectories;
        MaxRecursionDepth = config.MaxRecursionDepth;
        IgnoreInaccessible = config.IgnoreInaccessible;
        PatternMatchingMode = config.PatternMatchingMode;
        CaseSensitivity = config.CaseSensitivity;
        Traversal = config.Traversal ?? new();
        AuditMatches = config.AuditMatches;
        Diagnostics = config.Diagnostics;
        ErrorRecovery = config.ErrorRecovery ?? (config.IgnoreInaccessible
            ? FileQueryErrorRecoveryOptions.Skip
            : FileQueryErrorRecoveryOptions.Abort);
        IncludedPaths = config.IncludedPaths;
        MinSize = config.MinSize;
        MaxSize = config.MaxSize;
        ModifiedAfter = config.ModifiedAfter;
        ModifiedBefore = config.ModifiedBefore;
        AttributesToSkip = config.AttributesToSkip;
        GitIgnoreFiles = config.GitIgnoreFiles;
        IncludeDirectories = config.IncludeDirectories;
    }

    /// <summary>
    /// Gets the pattern configuration that governs which files are included or excluded during traversal.
    /// </summary>
    public PatternInput PatternInput { get; }

    /// <summary>
    /// Gets a value indicating whether subdirectories are traversed.
    /// </summary>
    public bool RecurseSubdirectories { get; }

    /// <summary>
    /// Gets the maximum allowed recursion depth.
    /// </summary>
    public int MaxRecursionDepth { get; }

    /// <summary>
    /// Gets a value indicating whether inaccessible directories are silently skipped.
    /// </summary>
    public bool IgnoreInaccessible { get; }

    /// <summary>
    /// Gets the matching mode used when the
    /// <see cref="PatternInput.InterpretationMode"/> is
    /// <see cref="PatternInterpretationMode.Specific"/>.
    /// </summary>
    public PatternMatchingMode PatternMatchingMode { get; }

    /// <summary>
    /// Gets the case-sensitivity behavior for path comparisons.
    /// </summary>
    public CaseSensitivity CaseSensitivity { get; }

    /// <summary>
    /// Gets traversal-specific configuration options such as traversal order
    /// and symbolic link handling.
    /// </summary>
    public TraversalOptions Traversal { get; }

    /// <summary>
    /// Gets a value indicating whether match diagnostics are emitted during traversal.
    /// </summary>
    public bool AuditMatches { get; }

    /// <summary>
    /// Gets the diagnostic sink used when <see cref="AuditMatches"/> is enabled.
    /// </summary>
    public IProgress<FileQueryDiagnostic>? Diagnostics { get; }

    /// <summary>
    /// Gets the configured IO error recovery policy.
    /// </summary>
    public FileQueryErrorRecoveryOptions ErrorRecovery { get; }

    /// <summary>
    /// Gets the optional root-relative allow-list of files to query, or <see langword="null"/>
    /// when traversal is unrestricted.
    /// </summary>
    /// <remarks>
    /// This is the execution form of <see cref="FileQueryOptionsConfig.IncludedPaths"/>. When it
    /// is set, matching is only attempted for the listed files, and directories that cannot
    /// contain a listed file are pruned.
    /// </remarks>
    public IReadOnlyCollection<string>? IncludedPaths { get; }

    /// <summary>
    /// Gets the inclusive minimum entry size in bytes, or <see langword="null"/> when no lower bound is configured.
    /// </summary>
    /// <remarks>Size filters apply to files only; directories are never rejected based on size.</remarks>
    public long? MinSize { get; }

    /// <summary>
    /// Gets the inclusive maximum entry size in bytes, or <see langword="null"/> when no upper bound is configured.
    /// </summary>
    /// <remarks>Size filters apply to files only; directories are never rejected based on size.</remarks>
    public long? MaxSize { get; }

    /// <summary>
    /// Gets the exclusive lower last-write boundary, or <see langword="null"/> when no lower bound is configured.
    /// </summary>
    /// <remarks>Time filters apply to files only; directories are never rejected based on time.</remarks>
    public DateTimeOffset? ModifiedAfter { get; }

    /// <summary>
    /// Gets the exclusive upper last-write boundary, or <see langword="null"/> when no upper bound is configured.
    /// </summary>
    /// <remarks>Time filters apply to files only; directories are never rejected based on time.</remarks>
    public DateTimeOffset? ModifiedBefore { get; }

    /// <summary>
    /// Gets the attribute flags that exclude an entry, or <see cref="FileAttributes.None"/> when no
    /// attribute filter is configured.
    /// </summary>
    /// <remarks>
    /// An entry is skipped when any configured flag is set. Directories matching the filter are
    /// pruned, so their contents are not traversed.
    /// </remarks>
    public FileAttributes AttributesToSkip { get; }

    /// <summary>
    /// Gets the Git ignore-file loading settings, or <see langword="null"/> when ignore files are not loaded.
    /// </summary>
    public GitIgnoreOptions? GitIgnoreFiles { get; }

    /// <summary>
    /// Gets a value indicating whether included directories are yielded as results.
    /// </summary>
    /// <remarks>
    /// Disabled by default, so path-only queries keep returning files only. When enabled, a
    /// directory is yielded when the matcher includes it; directory-only rules and negation
    /// therefore decide directory results using the same last-match-wins semantics as files.
    /// </remarks>
    public bool IncludeDirectories { get; }

    /// <summary>
    /// Validates this options instance for internal consistency.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <see cref="MaxRecursionDepth"/> is less than
    /// <see cref="UNLIMITED_RECURSION_DEPTH"/>.
    /// </exception>
    internal void Validate() {
        ArgumentOutOfRangeException.ThrowIfLessThan(MaxRecursionDepth, UNLIMITED_RECURSION_DEPTH);
        ErrorRecovery.Validate();
        ValidateSizeRange(MinSize, MaxSize);
        ValidateModifiedRange(ModifiedAfter, ModifiedBefore);
    }

    /// <summary>
    /// Validates the optional size boundaries.
    /// </summary>
    /// <param name="minSize">The inclusive minimum size.</param>
    /// <param name="maxSize">The inclusive maximum size.</param>
    private static void ValidateSizeRange(long? minSize, long? maxSize) {
        if(minSize is < 0) {
            throw new ArgumentOutOfRangeException(nameof(minSize), minSize, "The minimum size cannot be negative.");
        }

        if(maxSize is < 0) {
            throw new ArgumentOutOfRangeException(nameof(maxSize), maxSize, "The maximum size cannot be negative.");
        }

        if(minSize is { } min && maxSize is { } max && min > max) {
            throw new ArgumentException("The minimum size cannot exceed the maximum size.", nameof(minSize));
        }
    }

    /// <summary>
    /// Validates the optional last-write boundaries.
    /// </summary>
    /// <param name="after">The exclusive lower boundary.</param>
    /// <param name="before">The exclusive upper boundary.</param>
    private static void ValidateModifiedRange(DateTimeOffset? after, DateTimeOffset? before) {
        if(after is { } afterValue && before is { } beforeValue && afterValue >= beforeValue) {
            throw new ArgumentException(
                "The 'ModifiedAfter' boundary must be earlier than the 'ModifiedBefore' boundary.",
                nameof(after));
        }
    }
}
