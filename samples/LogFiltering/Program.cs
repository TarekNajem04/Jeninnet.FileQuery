using Jeninnet.FileQuery;

Console.WriteLine("=== Jeninnet.FileQuery — Real-time Log Filtering Sample ===");

// 1. Setup a dummy log directory for the demo
var root = Path.Combine(Path.GetTempPath(), "Jeninnet.LogSample");
Directory.CreateDirectory(root);
Directory.CreateDirectory(Path.Combine(root, "app1"));
Directory.CreateDirectory(Path.Combine(root, "app2"));

await File.WriteAllTextAsync(Path.Combine(root, "app1", "service.log"), "log content");
await File.WriteAllTextAsync(Path.Combine(root, "app2", "error.log"), "error content");
await File.WriteAllTextAsync(Path.Combine(root, "old.txt"), "not a log");

Console.WriteLine($"Scanning directory: {root}");
Console.WriteLine("Looking for: **/!*.log (All .log files in any subdirectory)");
Console.WriteLine("Press Ctrl+C to cancel.");

// 2. Configure the query
var query = FileQuery.From(root)
                     .Where("**")           // exclude everything else
                     .Where("!**/*.log")    // include .log files anywhere
                     .IgnoreCase()
                     .Build();

var engine = FileQueryRuntime.Create();

// 3. Execute asynchronously with cancellation support
using var cts = new CancellationTokenSource();

// For demo purposes, we'll cancel after 5 seconds if not finished (though this is instant)
cts.CancelAfter(TimeSpan.FromSeconds(5));

try {
    var count = 0;
    await foreach(var filePath in engine.ExecuteAsync(query, cts.Token)) {
        count++;
        Console.WriteLine($"[FOUND {count:D2}] {Path.GetFileName(filePath)} -> {filePath}");

        // Simulate some async processing per file
        await Task.Delay(100, cts.Token);
    }

    Console.WriteLine($"\nScan complete. Total logs found: {count}");
}
catch(OperationCanceledException) {
    Console.WriteLine("\nScan was canceled.");
}
finally {
    // Cleanup
    try { Directory.Delete(root, true); } catch { /* ignore */ }
}
