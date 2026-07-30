namespace Jeninnet.FileQuery.Enums;

/// <summary>
/// Specifies how textual comparisons should treat character casing.
/// </summary>
public enum CaseSensitivity {
    /// <summary>
    /// Use the default case-sensitivity of the underlying operating system.
    /// </summary>
    PlatformDefault = 0,

    /// <summary>
    /// Comparisons must match character casing exactly.
    /// </summary>
    Sensitive = 1,

    /// <summary>
    /// Comparisons ignore character casing and treat uppercase and lowercase letters as equivalent.
    /// </summary>
    Insensitive = 2
}
