//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery;

/// <summary>
/// Configures recoverable IO error handling during traversal.
/// </summary>
/// <param name="Action">The recovery action to apply when a recoverable IO exception occurs.</param>
/// <param name="MaxRetryAttempts">The maximum retry count used when <paramref name="Action"/> is <see cref="FileQueryErrorAction.Retry"/>.</param>
public sealed record FileQueryErrorRecoveryOptions(
    FileQueryErrorAction Action = FileQueryErrorAction.Skip,
    int MaxRetryAttempts = 0
) {
    /// <summary>
    /// Gets the default skip policy.
    /// </summary>
    public static FileQueryErrorRecoveryOptions Skip { get; } = new();

    /// <summary>
    /// Gets the default abort policy.
    /// </summary>
    public static FileQueryErrorRecoveryOptions Abort { get; } = new(FileQueryErrorAction.Abort);

    /// <summary>
    /// Creates a retry policy with the specified retry attempt count.
    /// </summary>
    /// <param name="maxRetryAttempts">The maximum retry attempts before the error is propagated.</param>
    /// <returns>A retry recovery policy.</returns>
    public static FileQueryErrorRecoveryOptions Retry(int maxRetryAttempts) => new(FileQueryErrorAction.Retry, maxRetryAttempts);

    internal void Validate() {
        if(MaxRetryAttempts < 0) {
            throw new ArgumentOutOfRangeException(nameof(MaxRetryAttempts), "Retry attempts cannot be negative.");
        }
    }
}
