//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
using Jeninnet.FileQuery.IO;

namespace Jeninnet.FileQuery.Tests.Core;

/// <summary>
/// Tests for the filesystem retry/skip guard.
/// </summary>
[TestClass]
public sealed class FileSystemGuardsTests {
    /// <summary>
    /// Retry is attempted even when inaccessible entries are ignored; the final attempt is skipped.
    /// </summary>
    [TestMethod]
    public void ShouldSkip_RetryWithDefaultIgnore_RetriesBeforeSkipping() {
        var retry = FileQueryErrorRecoveryOptions.Retry(2);

        Assert.IsFalse(FileSystemGuards.ShouldSkip(ignoreInaccessible: true, errorRecovery: retry, attempt: 0, attempts: 3));
        Assert.IsFalse(FileSystemGuards.ShouldSkip(ignoreInaccessible: true, errorRecovery: retry, attempt: 1, attempts: 3));
        Assert.IsTrue(FileSystemGuards.ShouldSkip(ignoreInaccessible: true, errorRecovery: retry, attempt: 2, attempts: 3));
        Assert.IsFalse(FileSystemGuards.ShouldSkip(ignoreInaccessible: false, errorRecovery: retry, attempt: 2, attempts: 3));
    }
}
