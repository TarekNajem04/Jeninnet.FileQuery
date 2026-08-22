//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
/*
 * Purpose: matcher composition.
 * Demonstrates the hybrid pattern matcher: GitIgnore-style patterns, negation,
 * and Regex patterns combined in a single query.
 */

var root = SampleUtils.CreateDemoTree("HybridMatcher");

try {
    var query = FileQuery.From(root)
                         .UsingHybrid()
                         .Where(
                             "**",                    // Exclude every file (GitIgnore-style glob).
                             "!**/*.cs",              // ...then re-include every .cs file at any depth (negation).
                             "r:.*\\.(png|jpg)$"     // Regex pattern: include image files as well.
                         )
                         .IgnoreCase()
                         .Build();

    SampleUtils.RunDemo(
        title: "Hybrid Matcher — composition",
        description: "One query mixes GitIgnore-style globs, negation ('!'), and regex ('r:') patterns: " +
                     "'**' excludes everything, '!**/*.cs' re-includes C# files at any depth, and the " +
                     "bare regex 'r:' pattern re-includes image files.",
        queryText: "FileQuery.From(root).UsingHybrid().Where(\"**\", \"!**/*.cs\", \"r:.*\\\\.(png|jpg)$\").Build()",
        query: query,
        expected: "The 4 .cs files plus the 2 images (6 files)."
    );
}
finally {
    SampleUtils.Cleanup(root);
}
