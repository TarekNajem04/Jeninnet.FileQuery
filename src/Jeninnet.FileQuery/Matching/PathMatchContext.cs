namespace Jeninnet.FileQuery.Matching;

/// <summary>
/// Represents a fully normalized path input used during pattern evaluation.
/// </summary>
/// <remarks>
/// This <see langword="ref struct"/> bundles all contextual information required for matching,
/// including the normalized path, its kind (file or directory), and the case-sensitivity mode.
/// Being a <see langword="ref struct"/> ensures zero-allocation, span-based matching.
/// </remarks>
internal readonly ref struct PathMatchContext
{
    /// <summary>
    /// Initializes a new <see cref="PathMatchContext"/> instance.
    /// </summary>
    /// <param name="path">The normalized absolute path to evaluate.</param>
    /// <param name="pathKind">Indicates whether the path represents a directory.</param>
    /// <param name="caseSensitivity">Specifies how character casing should be interpreted.</param>
    public PathMatchContext(
        ReadOnlySpan<char> path,
        PathKind pathKind,
        CaseSensitivity caseSensitivity = CaseSensitivity.Sensitive
    )
    {
        Path = path;
        PathKind = pathKind;
        CaseSensitivity = caseSensitivity;
    }

    /// <summary>
    /// Gets the normalized absolute path being evaluated.
    /// </summary>
    public ReadOnlySpan<char> Path { get; }

    /// <summary>
    /// Gets whether the path represents a directory.
    /// </summary>
    public PathKind PathKind { get; }

    /// <summary>
    /// Gets the case-sensitivity mode used during matching.
    /// </summary>
    public CaseSensitivity CaseSensitivity { get; }

    /// <summary>
    /// Converts the case-sensitivity mode into a <see cref="StringComparison"/>
    /// suitable for span-based comparisons.
    /// </summary>
    public StringComparison GetStringComparison() =>
        CaseSensitivity == CaseSensitivity.Insensitive
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

    /// <summary>
    /// Creates a <see cref="PathMatchContext"/> for a file path.
    /// </summary>
    /// <param name="path">The normalized absolute path of the file.</param>
    /// <param name="caseSensitivity">Specifies how character casing should be interpreted.</param>
    public static PathMatchContext CreateFileContext(
        ReadOnlySpan<char> path,
        CaseSensitivity caseSensitivity = CaseSensitivity.Sensitive) =>
        new(path, PathKind.File, caseSensitivity);

    /// <summary>
    /// Creates a <see cref="PathMatchContext"/> for a directory path.
    /// </summary>
    /// <param name="path">The normalized absolute path of the directory.</param>
    /// <param name="caseSensitivity">Specifies how character casing should be interpreted.</param>
    public static PathMatchContext CreateDirectoryContext(
        ReadOnlySpan<char> path,
        CaseSensitivity caseSensitivity = CaseSensitivity.Sensitive) =>
        new(path, PathKind.Directory, caseSensitivity);
}
