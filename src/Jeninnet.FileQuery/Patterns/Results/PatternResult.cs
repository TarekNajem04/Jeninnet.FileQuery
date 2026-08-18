//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Patterns.Results;

/// <summary>
/// Represents the result of a pattern classification or compilation operation.
/// </summary>
/// <typeparam name="T">The result type.</typeparam>
internal readonly record struct PatternResult<T> {
    /// <summary>
    /// Gets the value of the result. Throws <see cref="InvalidOperationException"/> if the operation failed.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="IsSuccess"/> is <see langword="false"/>.</exception>
    [AllowNull]
    public T Value => IsSuccess ? field! : throw new InvalidOperationException($"Cannot access Value of a failed result: {Error}");

    /// <summary>
    /// Gets the error message, if any.
    /// </summary>
    public string? Error { get; }

    /// <summary>
    /// Indicates whether the operation was successful (i.e., no error occurred).
    /// </summary>
    public bool IsSuccess => Error is null;

    private PatternResult(T? value, string? error) {
        Value = value;
        Error = error;
    }

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
