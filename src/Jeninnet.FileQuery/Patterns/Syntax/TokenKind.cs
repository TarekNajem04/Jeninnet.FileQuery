namespace Jeninnet.FileQuery.Patterns.Syntax;

/// <summary>
/// Specifies the types of tokens that can appear in a pattern expression, such as those used for file path or string
/// matching.
/// </summary>
/// <remarks>
/// Use this enumeration to identify and process different pattern elements, including literals, wildcards, character sets, and regular expressions.
/// The specific meaning of each token type depends on the pattern syntax supported by the consuming API.
/// </remarks>
public enum TokenKind {
    /// <summary>
    /// Represents a constant value in an expression or syntax tree.
    /// </summary>
    Literal,

    /// <summary>
    /// Represents a wildcard pattern that can be used to match multiple values or strings according to specific matching rules.
    /// </summary>
    /// <remarks>
    /// Use this type to define patterns that match a range of values, such as file names or identifiers, where exact matches are not required.
    /// The specific matching behavior depends on the implementation and may support characters like '*'.
    /// </remarks>
    Wildcard,           // *

    /// <summary>
    /// Indicates whether the operation or process should be performed recursively.
    /// </summary>
    RecursiveWildcard,  // **

    /// <summary>
    /// Represents a value that encapsulates a single character.
    /// </summary>
    SingleChar,         // ?

    /// <summary>
    /// Represents a set of characters used for encoding or validation operations.
    /// </summary>
    CharacterSet,       // [a-z]

    /// <summary>
    /// Represents a regular expression pattern used for matching and validating text.
    /// </summary>
    RegularExpression,  // r:regex

    /// <summary>
    /// Represents a directory separator (e.g. forward slash or backslash).
    /// </summary>
    DirectorySeparator,  // / or \

    /// <summary>
    /// Represents a segment separator character.
    /// </summary>
    Separator,         // /

    /// <summary>
    /// Represents an escape character prefix.
    /// </summary>
    Escape,

    /// <summary>
    /// Represents the starting bracket of a character class range.
    /// </summary>
    RangeStart,        // [

    /// <summary>
    /// Represents the ending bracket of a character class range.
    /// </summary>
    RangeEnd,          // ]

    /// <summary>
    /// Represents a pattern negation character (e.g. <c>!</c>).
    /// </summary>
    Negation,          // !

    /// <summary>
    /// Represents a pattern comment symbol (e.g. <c>#</c>).
    /// </summary>
    Comment,           // #
}
