/*
 * Purpose: anchored regex patterns.
 * '^' anchors the regex to the root, so only files whose full relative path starts with 'src/' match.
 */

var root = SampleUtils.CreateDemoTree("RegexAnchoredMatching");

try {
    var query = FileQuery.From(root)
                         .UsingRegex()
                         .Where("r:^src/.*\\.cs$")
                         .Build();

    SampleUtils.RunDemo(
        title: "Regex — Anchored Matching",
        description: "The 'r:' prefix selects regex syntax; '^src/' anchors to the root, and '.*\\\\.cs$' " +
                     "requires the relative path to end in '.cs'.",
        queryText: "FileQuery.From(root).UsingRegex().Where(\"r:^src/.*\\\\.cs$\").Build()",
        query: query,
        expected: "The 4 .cs files under 'src' — and nothing outside it."
    );
}
finally {
    SampleUtils.Cleanup(root);
}
