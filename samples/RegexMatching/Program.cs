/*
 * Purpose: demonstrate regex pattern support.
 * This sample demonstrates how to use the pattern language to demonstrate regex pattern support.
 * We will include only .cs files under the "src" directory and its subdirectories using a regex pattern.
 * - regex matcher
 * - PatternClassifier
 * - RegexPathMatcher
 */

using System.Reflection;
using Jeninnet.FileQuery;

var root = @"C:\repo";

if(!Directory.Exists(root))
{
    Console.WriteLine($"Directory '{root}' does not exist. We will use the directory of the executing assembly as the root for our query.");
    root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
}

if(string.IsNullOrEmpty(root))
{
    Console.WriteLine("Unable to determine a valid root directory for the query.");
    return;
}

var engine = FileQueryRuntime.Create();

/*
 * Notes:
 * - The order of the patterns matters. The first pattern that matches a file will determine
 * - whether it is included or excluded. In the this example, we first exclude all files with "**" and then include only .cs files under the "src" directory and its subdirectories. 
 *   If we reversed the order, we would include all "src//**//*.cs" files and then exclude everything else, resulting in no files being included.
 * - The patterns are evaluated in a depth-first manner, meaning that the engine will first evaluate the patterns for the current directory before moving on to subdirectories.
 *   This allows for more granular control over which files are included or excluded based on their location in the directory structure.
 * - We use last-role-wins semantics, which means that if a file matches multiple patterns, the last pattern that matches will determine whether the file is included or excluded.
 */
var query = FileQuery.From(root)
                     .UsingRegex()                  // Use the regex pattern matcher
                     /*
                      * Because we use the regex matcher, we need to use the "r:" prefix to indicate that the pattern is a regex pattern.
                      * The regex pattern "src/.*\\.cs$" matches any .cs file under the "src" directory and its subdirectories.
                      */
                     .Where("r:^src/.*\\.cs$")    // Then include only .cs files under the "src" directory and its subdirectories
                     .Build();

var results = engine.Execute(query).ToList();

if(results.Count == 0)
{
    Console.WriteLine("No files matched the query.");
    return;
}

foreach(var file in results)
{
    Console.WriteLine(file);
}
