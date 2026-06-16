namespace Jeninnet.FileQuery.Extensions;

/// <summary>
/// Provides extension methods and resolution logic for the <see cref="CaseSensitivity"/> enumeration.
/// </summary>
public static class CaseSensitivityExtensions
{
    /// <summary>
    /// Resolves a <see cref="CaseSensitivity"/> value into a concrete mode,
    /// handling platform-specific defaults when specified.
    /// </summary>
    /// <param name="caseSensitivity">The case sensitivity mode to resolve.</param>
    /// <returns>
    /// The final resolved <see cref="CaseSensitivity"/> value.
    /// </returns>
    public static CaseSensitivity Resolve(this CaseSensitivity caseSensitivity)
        => caseSensitivity switch
        {
            CaseSensitivity.PlatformDefault => DetectOsCaseSensitivity(),
            _ => caseSensitivity
        };

    /// <summary>
    /// Converts the case-sensitivity mode into a <see cref="StringComparison"/>
    /// suitable for span-based comparisons.
    /// </summary>
    /// <param name="caseSensitivity">The case-sensitivity mode to convert.</param>
    /// <returns>A <see cref="StringComparison"/> corresponding to the resolved case sensitivity.</returns>
    public static StringComparison GetStringComparison(this CaseSensitivity caseSensitivity) =>
        Resolve(caseSensitivity) == CaseSensitivity.Insensitive
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

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
