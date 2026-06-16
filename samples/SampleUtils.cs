using System.Reflection;

namespace Jeninnet.FileQuery.Samples;

public static class SampleUtils
{
    public static string GetRootDirectory()
    {
        var root = @"C:\repo";

        if (!Directory.Exists(root))
        {
            Console.WriteLine($"Directory '{root}' does not exist. We will use the directory of the executing assembly as the root for our query.");
            root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        }

        return root;
    }

    public static void ExecuteAndPrint(FileQuery query)
    {
        var engine = FileQueryRuntime.Create();
        var results = engine.Execute(query).ToList();

        if (results.Count == 0)
        {
            Console.WriteLine("No files matched the query.");
            return;
        }

        foreach (var file in results)
        {
            Console.WriteLine(file);
        }
    }
}
