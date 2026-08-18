//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
/*
 * Purpose: nested glob matching.
 * Recursive '**' patterns combined in one query select files with different
 * extensions at any depth.
 */

var root = SampleUtils.CreateDemoTree("NestedGlobMatching");

try {
    var query = FileQuery.From(root)
                         .UsingGlob()
                         .Where(
                             "**/*.cs",   // Every .cs file at any depth.
                             "**/*.md"    // Every .md file at any depth.
                         )
                         .Build();

    SampleUtils.RunDemo(
        title: "Nested Glob Matching",
        description: "Each recursive '**' pattern reaches one extension at any depth; combining both globs " +
                     "in a single query selects .cs and .md files together.",
        queryText: "FileQuery.From(root).UsingGlob().Where(\"**/*.cs\", \"**/*.md\").Build()",
        query: query,
        expected: "The 4 .cs files plus the 2 .md files (6 files)."
    );
}
finally {
    SampleUtils.Cleanup(root);
}
