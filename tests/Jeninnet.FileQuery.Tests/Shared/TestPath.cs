namespace Jeninnet.FileQuery.Tests.Shared;

/// <summary>
/// Helper factory for building path match contexts.
/// </summary>
internal static class TestPath {
    public static PathMatchContext File(string path) => new(path, pathKind: PathKind.File, caseSensitivity: CaseSensitivity.Sensitive);

    public static PathMatchContext Directory(string path) => new(path, pathKind: PathKind.Directory, caseSensitivity: CaseSensitivity.Sensitive);
}

