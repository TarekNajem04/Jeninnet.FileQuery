namespace Jeninnet.FileQuery;

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
/// <strong>Immutability contract:</strong> This type is physically immutable.
/// All properties are set during construction and cannot be modified.
/// The engine, pattern compiler, and matchers rely on this stability to ensure
/// deterministic behavior, caching safety, and thread safety.
/// </para>
/// </remarks>
public sealed record FileQueryOptions
{
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

    public FileQueryOptions(
        PatternInput patternInput,
        bool recurseSubdirectories = true,
        int maxRecursionDepth = UNLIMITED_RECURSION_DEPTH,
        bool ignoreInaccessible = true,
        PatternMatchingMode patternMatchingMode = PatternMatchingMode.GitIgnore,
        CaseSensitivity caseSensitivity = CaseSensitivity.PlatformDefault,
        TraversalOptions? traversal = null,
        bool auditMatches = false,
        IProgress<FileQueryDiagnostic>? diagnostics = null,
        FileQueryErrorRecoveryOptions? errorRecovery = null
)
    {
        PatternInput = patternInput;
        RecurseSubdirectories = recurseSubdirectories;
        MaxRecursionDepth = maxRecursionDepth;
        IgnoreInaccessible = ignoreInaccessible;
        PatternMatchingMode = patternMatchingMode;
        CaseSensitivity = caseSensitivity;
        Traversal = traversal ?? new();
        AuditMatches = auditMatches;
        Diagnostics = diagnostics;
        ErrorRecovery = errorRecovery ?? (ignoreInaccessible
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
    internal void Validate()
    {
        if(MaxRecursionDepth < UNLIMITED)
        {
#pragma warning disable S3928
            throw new ArgumentOutOfRangeException(nameof(MaxRecursionDepth), "Recursion depth cannot be less than -1.");
#pragma warning restore S3928
        }

        ErrorRecovery.Validate();
    }
}
