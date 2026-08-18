//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
/*
 * Purpose: asynchronous streaming with cancellation.
 * Demonstrates ExecuteAsync: results are streamed as they are discovered, with
 * cooperative cancellation and real-time per-file processing.
 */

Console.WriteLine("=== Jeninnet.FileQuery — Real-time Log Filtering Sample ===");

var root = SampleUtils.CreateDemoTree("LogFiltering");

try {
    var query = FileQuery.From(root)
                         .UsingGlob()
                         .Where("**/*.log")
                         .IgnoreCase()
                         .Build();

    Console.WriteLine($"Scanning directory: {root}");
    Console.WriteLine("Looking for: **/*.log (every .log file at any depth)");
    Console.WriteLine();

    // For demo purposes, cancel after 5 seconds (the scan itself is instant).
    using var cts = new CancellationTokenSource();
    cts.CancelAfter(TimeSpan.FromSeconds(5));

    var engine = FileQueryRuntime.Create();
    var count = 0;

    try {
        await foreach(var filePath in engine.ExecuteAsync(query, cts.Token)) {
            count++;
            Console.WriteLine($"[FOUND {count:D2}] {Path.GetFileName(filePath)} -> {filePath}");

            // Simulate real-time processing per file.
            await Task.Delay(100, cts.Token);
        }

        Console.WriteLine($"\nScan complete. Total logs found: {count}");
    }
    catch(OperationCanceledException) {
        Console.WriteLine("\nScan was canceled.");
    }
}
finally {
    SampleUtils.Cleanup(root);
}
