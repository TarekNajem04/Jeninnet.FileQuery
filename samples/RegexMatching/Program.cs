/*
 * Purpose: regex pattern support.
 * An unanchored regex matches anywhere in the relative path, so '.*\.log$'
 * finds every '.log' file regardless of its directory.
 */

var root = SampleUtils.CreateDemoTree("RegexMatching");

try {
    var query = FileQuery.From(root)
                         .UsingRegex()
                         .Where("r:.*\\.log$")
                         .Build();

    SampleUtils.RunDemo(
        title: "Regex — Unanchored Matching",
        description: "Without '^', the regex can match at any position: '.*\\\\.log$' finds every '.log' file " +
                     "at any depth, unlike an anchored pattern.",
        queryText: "FileQuery.From(root).UsingRegex().Where(\"r:.*\\\\.log$\").Build()",
        query: query,
        expected: "The 3 log files: 'app.log', 'logs/app.log', 'logs/error.log'."
    );
}
finally {
    SampleUtils.Cleanup(root);
}
