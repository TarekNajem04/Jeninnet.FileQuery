namespace Jeninnet.FileQuery.Patterns.Results;

/// <summary>
/// Represents the result of a pattern classification or compilation operation.
/// </summary>
/// <typeparam name="T">The result type.</typeparam>
/// <param name="Value">The value of the result.</param>
/// <param name="Error">The error message, if any. </param>
internal readonly record struct PatternResult<T>(T? Value, string? Error)
{
    /// <summary>
    /// Indicates whether the operation was successful (i.e., no error occurred).
    /// </summary>
    public bool IsSuccess => Error is null;

    /// <summary>
    /// Creates a successful PatternResult with the specified value.
    /// </summary>
    /// <param name="value">The value of the result.</param>
    /// <returns>The successful PatternResult.</returns>
    public static PatternResult<T> Success(T value) => new(value, null);

    /// <summary>
    /// Creates a failed PatternResult with the specified error message.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <returns>The failed PatternResult.</returns>
    public static PatternResult<T> Fail(string error) => new(default, error);
}
