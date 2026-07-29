namespace Jeninnet.FileQuery.Tests.Shared;

/// <summary>
/// Provides helper methods for creating pattern dictionaries used to associate pattern types with collections of pattern strings.
/// </summary>
/// <remarks>
/// This class is intended for use in scenarios where pattern types need to be mapped to lists of string patterns,
/// such as configuration or pattern matching operations.
/// All members are static and thread-safe.
/// </remarks>
public static class PatternHelpers {
    /// <summary>
    /// Creates a read-only dictionary that maps the specified pattern type to a read-only list of pattern strings.
    /// </summary>
    /// <param name="patternKind">The pattern type to associate with the provided pattern strings in the resulting dictionary.</param>
    /// <param name="patterns">An array of pattern strings to be grouped under the specified pattern type. Cannot be null.</param>
    /// <returns>A read-only dictionary containing a single entry that maps the specified pattern type to a read-only list of the
    /// provided pattern strings.</returns>
    public static IReadOnlyDictionary<PatternKind, IEnumerable<string>> Create(
        PatternKind patternKind,
        params string[] patterns
    ) => new Dictionary<PatternKind, IEnumerable<string>> {
        [patternKind] = patterns.Where(static p => !string.IsNullOrWhiteSpace(p))
    };
}

