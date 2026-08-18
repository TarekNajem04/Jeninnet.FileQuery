//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
/*
 * Purpose: hello world of the library.
 * A query without patterns includes every file under the root and its subdirectories.
 */

var root = SampleUtils.CreateDemoTree("BasicMatching");

try {
    var query = FileQuery.From(root)
                         .Build();

    SampleUtils.RunDemo(
        title: "Basic Matching — hello world",
        description: "The most basic usage: a query with no patterns matches every file in the tree.",
        queryText: "FileQuery.From(root).Build()",
        query: query,
        expected: "All 12 files of the demo tree."
    );
}
finally {
    SampleUtils.Cleanup(root);
}
