namespace Jeninnet.FileQuery.Tests.Shared;

public static class PathExtensions {
    public static bool EndWithNormalized(this string path, string ending) {
        ending = ending.Replace('/', Path.DirectorySeparatorChar);
        path = path.Replace('/', Path.DirectorySeparatorChar);
        return path.EndsWith(ending);
    }

    public static bool EndWithPath(this string path, string relative) {
        relative = relative.Replace('/', Path.DirectorySeparatorChar);
        path = path.Replace('/', Path.DirectorySeparatorChar);
        return path.EndsWith(relative);
    }
}
