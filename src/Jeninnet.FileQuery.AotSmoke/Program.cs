//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
using Jeninnet.FileQuery;
using Jeninnet.FileQuery.Enums;

// Native AOT smoke test: create a controlled tree, run a real query through the
// trimmed/AOT-compiled library, and fail the process when the result is wrong.
var root = Path.Combine(Path.GetTempPath(), "jeninnet-aot-smoke-" + Guid.NewGuid().ToString("N"));

try {
    Directory.CreateDirectory(Path.Combine(root, "src"));
    Directory.CreateDirectory(Path.Combine(root, "bin"));
    Directory.CreateDirectory(Path.Combine(root, "docs"));

    await File.WriteAllTextAsync(Path.Combine(root, "src", "a.cs"), "class A { }").ConfigureAwait(false);
    await File.WriteAllTextAsync(Path.Combine(root, "src", "b.txt"), "text").ConfigureAwait(false);
    await File.WriteAllTextAsync(Path.Combine(root, "bin", "c.cs"), "class C { }").ConfigureAwait(false);
    await File.WriteAllTextAsync(Path.Combine(root, "docs", "readme.md"), "# docs").ConfigureAwait(false);

    var query = FileQuery.From(root)
        .UsingGlob()
        .Where(PatternKind.Glob, ["**/*.cs"])
        .Build();

    var engine = FileQueryRuntime.Create();
    var matches = engine.Execute(query)
        .Select(path => Path.GetRelativePath(root, path).Replace('\\', '/'))
        .OrderBy(static path => path, StringComparer.Ordinal)
        .ToArray();

    string[] expected = ["bin/c.cs", "src/a.cs"];

    if(!matches.SequenceEqual(expected, StringComparer.Ordinal)) {
        await Console.Error.WriteLineAsync($"Unexpected matches: [{string.Join(", ", matches)}]").ConfigureAwait(false);
        return 1;
    }

    var entries = engine.ExecuteEntries(query)
        .OrderBy(static entry => entry.RelativePath, StringComparer.Ordinal)
        .ToArray();

    if(entries.Length != 2 || entries.Any(static entry => entry.Length <= 0)) {
        await Console.Error.WriteLineAsync("Entry metadata was not populated by the AOT-compiled host.").ConfigureAwait(false);
        return 1;
    }

    // Regex semantics, including unprefixed typed patterns, must survive trimming/AOT.
    var regexQuery = FileQuery.From(root)
        .UsingRegex()
        .Where(PatternKind.Regex, [@"^src/.*\.cs$"])
        .Build();

    var regexMatches = engine.Execute(regexQuery)
        .Select(path => Path.GetRelativePath(root, path).Replace('\\', '/'))
        .OrderBy(static path => path, StringComparer.Ordinal)
        .ToArray();

    string[] expectedRegex = ["src/a.cs"];

    if(!regexMatches.SequenceEqual(expectedRegex, StringComparer.Ordinal)) {
        await Console.Error.WriteLineAsync($"Unexpected regex matches: [{string.Join(", ", regexMatches)}]").ConfigureAwait(false);
        return 1;
    }

    // GitIgnore last-match-wins and negation must also survive trimming/AOT.
    var gitIgnoreQuery = FileQuery.From(root)
        .UsingGitIgnore()
        .Where(PatternKind.GitIgnore, ["**", "!**/*.cs"])
        .Build();

    var gitIgnoreMatches = engine.Execute(gitIgnoreQuery)
        .Select(path => Path.GetRelativePath(root, path).Replace('\\', '/'))
        .OrderBy(static path => path, StringComparer.Ordinal)
        .ToArray();

    if(!gitIgnoreMatches.SequenceEqual(expected, StringComparer.Ordinal)) {
        await Console.Error.WriteLineAsync($"Unexpected GitIgnore matches: [{string.Join(", ", gitIgnoreMatches)}]").ConfigureAwait(false);
        return 1;
    }

    await Console.Out.WriteLineAsync($"AOT smoke passed: {string.Join(", ", matches)}").ConfigureAwait(false);
    return 0;
}
finally {
    Directory.Delete(root, recursive: true);
}
