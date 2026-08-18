//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Tests.Shared;

/// <summary>
/// Helper factory for building path match contexts.
/// </summary>
internal static class TestPath {
    public static PathMatchContext File(string path) => new(path, pathKind: PathKind.File, caseSensitivity: CaseSensitivity.Sensitive);

    public static PathMatchContext Directory(string path) => new(path, pathKind: PathKind.Directory, caseSensitivity: CaseSensitivity.Sensitive);
}
