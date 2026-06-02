namespace Jeninnet.FileQuery.Patterns.Invariants.Definition;

/// <summary>
/// Result of an invariant validation.
/// </summary>
internal readonly record struct PatternInvariantResult(bool IsSuccess, string? Message) {
    public static PatternInvariantResult Success => new(true, null);

    public static PatternInvariantResult Fail(string message) => new(false, message);
}
