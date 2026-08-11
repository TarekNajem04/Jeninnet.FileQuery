/*
 * Purpose: negation and recursive patterns.
 * The '!' prefix re-includes; combined with a recursive '**' pattern it
 * brings back a whole class of files at any depth.
 */

var root = SampleUtils.CreateDemoTree("NegationAndRecursive");

try {
    var query = FileQuery.From(root)
                         .UsingGitIgnore()
                         .Where(
                             "**",          // Exclude every file.
                             "!**/*.log"    // ...then re-include every .log file at any depth.
                         )
                         .Build();

    SampleUtils.RunDemo(
        title: "Negation and Recursive Matching",
        description: "The negated recursive pattern '!**/*.log' re-includes every .log file at any depth: " +
                     "'**' excludes all files, then the negation restores exactly the 3 log files.",
        queryText: "FileQuery.From(root).UsingGitIgnore().Where(\"**\", \"!**/*.log\").Build()",
        query: query,
        expected: "The 3 log files: 'app.log', 'logs/app.log', 'logs/error.log'."
    );
}
finally {
    SampleUtils.Cleanup(root);
}
