/*
 * Purpose: hello world of the library.
 * This sample demonstrates the most basic usage of the library.
 */
using System.Reflection;
using Jeninnet.FileQuery;

var root = @"C:\repo";

if(!Directory.Exists(root)) {
    Console.WriteLine($"Directory '{root}' does not exist. We will use the directory of the executing assembly as the root for our query.");
    root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
}

if(string.IsNullOrEmpty(root)) {
    Console.WriteLine("Unable to determine a valid root directory for the query.");
    return;
}

var engine = FileQueryRuntime.Create();
// Including all files by default,
// we can simply build a query with the root directory and no patterns.
// This will include all files under the root directory and its subdirectories.
var query = FileQuery.From(root)
                     .Build();

var results = engine.Execute(query).ToList();

if(results.Count == 0) {
    Console.WriteLine("No files matched the query.");
    return;
}

foreach(var file in results) {
    Console.WriteLine(file);
}
