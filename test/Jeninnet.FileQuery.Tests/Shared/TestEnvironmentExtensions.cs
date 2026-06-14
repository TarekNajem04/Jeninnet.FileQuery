namespace Jeninnet.FileQuery.Tests.Shared;

public static class TestEnvironmentExtensions
{
    /// <summary>
    /// Creates a nested directory structure like a/b/c/d/... up to the specified number of levels.
    /// </summary>
    /// <param name="env">The test environment.</param>
    /// <param name="levels">The number of directory levels to create.</param>
    /// <param name="fileName">The base file name.</param>
    /// <param name="fileExt">The file extension.</param>
    /// <param name="fileCount">The number of files to create in each directory.</param>
    public static void CreateDeepDirectoryTree(
        this TestEnvironment env,
        int levels,
        string fileName = "file",
        string fileExt = "txt",
        int fileCount = 1
        )
    {
        ArgumentNullException.ThrowIfNull(env);

        if(levels <= 0)
        {
            return;
        }

        if(fileCount <= 0)
        {
            return;
        }

        var current = "";
        for(var i = 0; i < levels; i++)
        {
            current = Path.Combine(current, $"dir{i}");
            env.CreateDirectory(current);
        }

        if(fileCount > 1)
        {
            for(var i = 0; i < fileCount; i++)
            {
                var fullFileName = $"{fileName}_{i}.{fileExt}";
                env.CreateFiles(Path.Combine(current, fullFileName));
            }
        } else
        {
            // Optionally, put a file in the deepest directory
            var fullFileName = $"{fileName}.{fileExt}";
            env.CreateFiles(Path.Combine(current, fullFileName));
        }
    }
}
