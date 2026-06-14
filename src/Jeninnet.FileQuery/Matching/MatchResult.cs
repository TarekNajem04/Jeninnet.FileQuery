namespace Jeninnet.FileQuery.Matching;

/// <summary>
/// Represents the final outcome of a path matching operation performed by a matcher,
/// including both “did anything match?” and “what is the final inclusion state?”.
/// </summary>
/// <remarks>
/// This struct is essential for correctly implementing GitIgnore-style semantics.
/// It separates two distinct states required for complex rule evaluation:
/// <list type="bullet">
/// <item><term><see cref="IsMatched"/></term><description>Indicates whether the path matched <em>any</em> of the rules provided.</description></item>
/// <item><term><see cref="IsIncluded"/></term><description>Indicates the final inclusion/exclusion decision based on the “last rule wins” principle.</description></item>
/// </list>
/// For directories, these properties determine whether a directory should be traversed (pruned).
/// </remarks>
internal ref struct MatchResult
{
    /// <summary>
    /// Gets a value indicating whether the path is ultimately included in the final collection.
    /// </summary>
    /// <remarks>
    /// This property holds the final state dictated by the <em>last</em> matching pattern
    /// (negated for inclusion, non-negated for exclusion). This is the property checked by
    /// the consumer (e.g., <c>FileQueryEngine</c>) to yield the final file or directory.
    /// </remarks>
    public bool IsIncluded { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the path matched at least one pattern in the entire list.
    /// </summary>
    /// <remarks>
    /// This is crucial for handling default behavior, especially for “pure inclusion lists”
    /// (where all patterns are negated, e.g., <c>!*.txt</c>). If a path does not match
    /// <em>any</em> pattern in such a list, it must be excluded, even if <see cref="IsIncluded"/>
    /// would otherwise default to <see langword="true"/>.
    /// </remarks>
    public bool IsMatched { get; private set; }

    private MatchResult(bool isIncluded, bool isMatched)
    {
        IsIncluded = isIncluded;
        IsMatched = isMatched;
    }

    /// <summary>
    /// Checks if the path should be accepted by the query engine (i.e., <see cref="IsIncluded"/> is <see langword="true"/>).
    /// </summary>
    /// <returns><see langword="true"/> if the path should be included; otherwise, <see langword="false"/>.</returns>
    public readonly bool Successed() => IsIncluded;

    public readonly MatchOutcome ToOutcome() =>
        (IsIncluded, IsMatched) switch
        {
            (true, _) => MatchOutcome.Include,
            (false, true) => MatchOutcome.Exclude,
            _ => MatchOutcome.NoMatch
        };

    /// <summary>
    /// Sets <see cref="IsIncluded"/> to <see langword="true"/> (included).
    /// </summary>
    /// <returns>The current <see cref="MatchResult"/> instance for method chaining.</returns>
    public MatchResult Include()
    {
        IsIncluded = true;
        return this;
    }

    /// <summary>
    /// Sets <see cref="IsIncluded"/> based on the result of the provided delegate.
    /// </summary>
    /// <param name="func">A function that computes the inclusion state.</param>
    /// <returns>The current <see cref="MatchResult"/> instance for method chaining.</returns>
    /// <remarks>
    /// This overload is useful when inclusion depends on a more complex condition that should
    /// only be evaluated when needed.
    /// </remarks>
    public MatchResult Include(Func<bool> func)
    {
        ArgumentNullException.ThrowIfNull(func);
        IsIncluded = func();
        return this;
    }

    /// <summary>
    /// Sets <see cref="IsIncluded"/> to <see langword="false"/> (excluded).
    /// </summary>
    /// <returns>The current <see cref="MatchResult"/> instance for method chaining.</returns>
    public MatchResult Exclude()
    {
        IsIncluded = false;
        return this;
    }

    /// <summary>
    /// Sets <see cref="IsMatched"/> to <see langword="true"/>.
    /// </summary>
    /// <returns>The current <see cref="MatchResult"/> instance for method chaining.</returns>
    public MatchResult Match()
    {
        IsMatched = true;
        return this;
    }

    /// <summary>
    /// Sets <see cref="IsMatched"/> based on the result of the provided delegate.
    /// </summary>
    /// <param name="func">A function that computes the match state.</param>
    /// <returns>The current <see cref="MatchResult"/> instance for method chaining.</returns>
    /// <remarks>
    /// This overload allows deferring potentially expensive match checks until they are needed,
    /// while still preserving fluent usage.
    /// </remarks>
    public MatchResult Match(Func<bool> func)
    {
        ArgumentNullException.ThrowIfNull(func);
        IsMatched = func();
        return this;
    }

    /// <summary>
    /// Sets <see cref="IsMatched"/> to <see langword="false"/>.
    /// </summary>
    /// <returns>The current <see cref="MatchResult"/> instance for method chaining.</returns>
    public MatchResult NotMatch()
    {
        IsMatched = false;
        return this;
    }

    /// <summary>
    /// Creates a default failure result (not matched, not included).
    /// </summary>
    public static MatchResult Fail() => new(isIncluded: false, isMatched: false);

    /// <summary>
    /// Creates a successful match (matched and included).
    /// </summary>
    public static MatchResult Success() => new(isIncluded: true, isMatched: true);

    /// <summary>
    /// Creates an unmatched result that is still included.
    /// </summary>
    public static MatchResult Included() => new(isIncluded: true, isMatched: false);

    /// <summary>
    /// Creates a matched result that is excluded.
    /// </summary>
    public static MatchResult Matched() => new(isIncluded: false, isMatched: true);

    /// <summary>
    /// Implicit conversion to <see cref="bool"/> using <see cref="Successed"/>.
    /// </summary>
    /// <param name="result">The match result.</param>
    public static implicit operator bool([DisallowNull] MatchResult result) => result.Successed();
}
