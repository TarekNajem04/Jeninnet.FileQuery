namespace Jeninnet.FileQuery.Patterns.Invariants.Definition;

/// <summary>
/// Result of an invariant validation.
/// </summary>
/// <param name="IsSuccess">Whether the validation succeeded.</param>
/// <param name="Message">Optional error message if validation failed.</param>
internal readonly record struct PatternInvariantResult(bool IsSuccess, string? Message)
{
    public static PatternInvariantResult Success => new(true, null);

    public static PatternInvariantResult Fail(string message) => new(false, message);
}
