namespace Jeninnet.FileQuery.Internal;

/// <summary>
/// Provides logic for resolving <see cref="CaseSensitivity"/> preferences into concrete execution modes.
/// </summary>
internal static class CaseSensitivityResolver
{
    /// <summary>
    /// Determines the effective case-sensitivity mode based on the provided options.
    /// </summary>
    /// <param name="options">The query options containing the case sensitivity preference.</param>
    /// <returns>
    /// The resolved <see cref="CaseSensitivity"/> value.
    /// </returns>
    public static CaseSensitivity Resolve(FileQueryOptions options) => Resolve(options.CaseSensitivity);

    /// <summary>
    /// Resolves a <see cref="CaseSensitivity"/> value into a concrete mode,
    /// handling platform-specific defaults when specified.
    /// </summary>
    /// <param name="caseSensitivity">The case sensitivity mode to resolve.</param>
    /// <returns>
    /// The final <see cref="CaseSensitivity"/> value to be used by the matcher.
    /// </returns>
    public static CaseSensitivity Resolve(this CaseSensitivity caseSensitivity)
        => caseSensitivity switch
        {
            CaseSensitivity.PlatformDefault => DetectOsCaseSensitivity(),
            _ => caseSensitivity
        };

    /// <summary>
    /// Maps a <see cref="CaseSensitivity"/> value to the corresponding .NET <see cref="StringComparison"/> enumeration.
    /// </summary>
    /// <param name="caseSensitivity">The case sensitivity mode to convert.</param>
    /// <returns>
    /// <see cref="StringComparison.Ordinal"/> for <see cref="CaseSensitivity.Sensitive"/>
    /// and <see cref="StringComparison.OrdinalIgnoreCase"/> for <see cref="CaseSensitivity.Insensitive"/>.
    /// </returns>
    public static StringComparison ToStringComparison(this CaseSensitivity caseSensitivity)
        => Resolve(caseSensitivity) switch
        {
            CaseSensitivity.Sensitive => StringComparison.Ordinal,
            CaseSensitivity.Insensitive => StringComparison.OrdinalIgnoreCase,
            _ => StringComparison.Ordinal
        };

    /// <summary>
    /// Detects the default case sensitivity of the current operating system.
    /// </summary>
    /// <returns>
    /// <see cref="CaseSensitivity.Sensitive"/> on Linux, and <see cref="CaseSensitivity.Insensitive"/> on Windows or macOS.
    /// </returns>
    private static CaseSensitivity DetectOsCaseSensitivity() =>
       OperatingSystem.IsLinux()
           ? CaseSensitivity.Sensitive
           : CaseSensitivity.Insensitive;
}
