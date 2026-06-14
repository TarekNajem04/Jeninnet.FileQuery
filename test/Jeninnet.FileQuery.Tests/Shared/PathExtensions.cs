namespace Jeninnet.FileQuery.Tests.Shared;

public static class PathExtensions
{
    public static bool EndWithNormalized(this string path, string ending)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(ending);

        ending = ending.Replace('/', Path.DirectorySeparatorChar);
        path = path.Replace('/', Path.DirectorySeparatorChar);
        return path.EndsWith(ending, StringComparison.Ordinal);
    }

    public static bool EndWithPath(this string path, string relative)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(relative);

        relative = relative.Replace('/', Path.DirectorySeparatorChar);
        path = path.Replace('/', Path.DirectorySeparatorChar);
        return path.EndsWith(relative, StringComparison.Ordinal);
    }
}
