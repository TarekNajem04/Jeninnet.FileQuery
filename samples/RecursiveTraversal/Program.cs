/*
 * Purpose: recursive traversal.
 * A negated recursive pattern re-includes the whole 'src' subtree.
 */

var root = SampleUtils.CreateDemoTree("RecursiveTraversal");

try {
    var query = FileQuery.From(root)
                         .UsingGitIgnore()
                         .Where(
                             "**",              // Exclude every file.
                             "!src/**/*.cs"     // ...then re-include every .cs file under 'src', at any depth.
                         )
                         .Build();

    SampleUtils.RunDemo(
        title: "Recursive Traversal",
        description: "After '**' excludes all files, '!src/**/*.cs' re-includes the whole 'src' subtree: " +
                     "the recursive '**' walks every nested folder ('cli', 'test') down to its .cs files.",
        queryText: "FileQuery.From(root).UsingGitIgnore().Where(\"**\", \"!src/**/*.cs\").Build()",
        query: query,
        expected: "The 4 .cs files under 'src' — including 'src/cli' and 'src/test'."
    );
}
finally {
    SampleUtils.Cleanup(root);
}
