//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Samples;

/// <summary>
/// Provides shared helpers for the FileQuery sample applications:
/// a deterministic demo file tree, an explanatory demo runner, and best-effort cleanup.
/// </summary>
public static class SampleUtils {
    /// <summary>
    /// Gets the relative layout of the deterministic demo tree shared by every sample.
    /// </summary>
    private static readonly string[] _demoTreeFiles =
    [
        "app.log",
        "ReadMe.md",
        "assets/logo1.png",
        "assets/logo2.png",
        "backup/old.txt",
        "docs/guide.md",
        "logs/app.log",
        "logs/error.log",
        "src/FileQuery.cs",
        "src/Program.cs",
        "src/cli/Program.cs",
        "src/test/helpers.cs",
    ];

    /// <summary>
    /// Creates a fresh, deterministic demo tree for the sample with the specified name and returns its root directory.
    /// </summary>
    /// <param name="sampleName">The name of the sample; used to isolate its demo directory.</param>
    /// <returns>The absolute path of the sample's demo root directory.</returns>
    public static string CreateDemoTree(string sampleName) {
        var root = Path.Combine(Path.GetTempPath(), "Jeninnet.FileQuery.Samples", sampleName);

        if(Directory.Exists(root)) {
            Directory.Delete(root, recursive: true);
        }

        Directory.CreateDirectory(root);

        foreach(var relative in _demoTreeFiles) {
            var fullPath = Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            File.WriteAllText(fullPath, $"{relative} — sample fixture file.");
        }

        return root;
    }

    /// <summary>
    /// Runs an explanatory demo: prints a header, executes the query, lists every match, and prints a summary.
    /// </summary>
    /// <param name="title">The title of the demo.</param>
    /// <param name="description">What the demo demonstrates.</param>
    /// <param name="queryText">The query expressed as code, shown in the header.</param>
    /// <param name="query">The built query to execute.</param>
    /// <param name="expected">What the demo is expected to produce, shown in the header.</param>
    public static void RunDemo(string title, string description, string queryText, FileQuery query, string expected) {
        Console.WriteLine(new string('=', 72));
        Console.WriteLine($"Demo   : {title}");
        Console.WriteLine($"About  : {description}");
        Console.WriteLine($"Query  : {queryText}");
        Console.WriteLine($"Expect : {expected}");
        Console.WriteLine(new string('=', 72));

        var results = FileQueryRuntime.Create()
                                      .Execute(query)
                                      .ToList();

        if(results.Count == 0) {
            Console.WriteLine("  (no entries matched)");
        } else {
            for(var index = 0; index < results.Count; index++) {
                Console.WriteLine($"  [{index + 1:D2}] {results[index]}");
            }
        }

        Console.WriteLine(new string('-', 72));
        Console.WriteLine($"Total matches: {results.Count}");
        Console.WriteLine();
    }

    /// <summary>
    /// Deletes the demo tree created by <see cref="CreateDemoTree"/>.
    /// </summary>
    /// <param name="root">The demo root directory to delete.</param>
    public static void Cleanup(string root) {
        try {
            if(Directory.Exists(root)) {
                Directory.Delete(root, recursive: true);
            }
        }
        catch {
            // Best-effort cleanup only; a leftover demo directory is harmless.
        }
    }
}
