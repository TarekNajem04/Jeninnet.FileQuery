namespace Jeninnet.FileQuery.Extensions;

public static class CaseSensitivityExtensions {
    /// <summary>
    /// Converts the case-sensitivity mode into a <see cref="StringComparison"/>
    /// suitable for span-based comparisons.
    /// </summary>
    public static StringComparison GetStringComparison(this CaseSensitivity caseSensitivity) =>
        caseSensitivity == CaseSensitivity.Insensitive
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
}
