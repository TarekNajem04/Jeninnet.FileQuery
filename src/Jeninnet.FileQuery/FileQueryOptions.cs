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
    FileQueryErrorRecoveryOptions? ErrorRecovery = null
);

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
    /// Represents an unlimited value for numeric limits such as
    /// <see cref="MaxRecursionDepth"/>.
    /// </summary>
    public const int UNLIMITED = -1;

    /// <summary>
    /// Represents an unlimited recursion depth.
    /// Pass this value to <see cref="MaxRecursionDepth"/> to allow unbounded traversal.
    /// </summary>
    public const int UNLIMITED_RECURSION_DEPTH = UNLIMITED;

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
    /// Validates this options instance for internal consistency.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <see cref="MaxRecursionDepth"/> is less than
    /// <see cref="UNLIMITED_RECURSION_DEPTH"/>.
    /// </exception>
    internal void Validate() {
        if(MaxRecursionDepth < UNLIMITED) {
#pragma warning disable S3928
            throw new ArgumentOutOfRangeException(nameof(MaxRecursionDepth), "Recursion depth cannot be less than -1.");
#pragma warning restore S3928
        }

        ErrorRecovery.Validate();
    }
}
