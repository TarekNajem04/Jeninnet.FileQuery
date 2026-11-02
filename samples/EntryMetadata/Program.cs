//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
/*
 * Purpose: entry metadata and size/time/attribute filters.
 * ExecuteEntries() returns paths, size, timestamps, attributes, and directory state from the
 * same enumeration used for matching, so no extra file-system calls are performed.
 */

var root = SampleUtils.CreateDemoTree("EntryMetadata");
var smallPath = Path.Combine(root, "small.dat");
var largePath = Path.Combine(root, "large.dat");

await File.WriteAllTextAsync(smallPath, "small").ConfigureAwait(false);
await File.WriteAllTextAsync(largePath, new string('x', 2_048)).ConfigureAwait(false);

var oldTimestamp = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);
File.SetLastWriteTimeUtc(smallPath, oldTimestamp.UtcDateTime);
File.SetLastWriteTimeUtc(largePath, DateTime.UtcNow);

try {
    var entries = FileQuery.From(root)
        .UsingGlob()
        .Where(PatternKind.Glob, ["**/*.dat"])
        .MinSize(100)
        .MaxSize(4_096)
        .ModifiedAfter(oldTimestamp)
        .ModifiedBefore(DateTimeOffset.UtcNow.AddDays(1))
        .AttributesToSkip(FileAttributes.System)
        .ExecuteEntries()
        .OrderBy(static entry => entry.RelativePath, StringComparer.Ordinal)
        .ToList();

    await Console.Out.WriteLineAsync("Entry metadata and filters").ConfigureAwait(false);
    await Console.Out.WriteLineAsync("==========================").ConfigureAwait(false);

    foreach(var entry in entries) {
        await Console.Out.WriteLineAsync(entry.RelativePath).ConfigureAwait(false);
        await Console.Out.WriteLineAsync($"  full      : {entry.FullPath}").ConfigureAwait(false);
        await Console.Out.WriteLineAsync($"  size      : {entry.Length} bytes").ConfigureAwait(false);
        await Console.Out.WriteLineAsync($"  created   : {entry.CreationTimeUtc:O}").ConfigureAwait(false);
        await Console.Out.WriteLineAsync($"  modified  : {entry.LastWriteTimeUtc:O}").ConfigureAwait(false);
        await Console.Out.WriteLineAsync($"  attributes: {entry.Attributes}").ConfigureAwait(false);
        await Console.Out.WriteLineAsync($"  directory : {entry.IsDirectory}").ConfigureAwait(false);
    }

    await Console.Out.WriteLineAsync().ConfigureAwait(false);
    await Console.Out.WriteLineAsync($"Total entries: {entries.Count}").ConfigureAwait(false);
    await Console.Out.WriteLineAsync("Expected: only large.dat (small.dat is below MinSize and before ModifiedAfter).").ConfigureAwait(false);
}
finally {
    SampleUtils.Cleanup(root);
}
