//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
/*
 * Purpose: directory-only matching.
 * In GitIgnore-style rules, a pattern ending with '/' matches directories
 * themselves; re-including a directory restores access to its files.
 */

var root = SampleUtils.CreateDemoTree("DirectoryOnlyMatching");

try {
    var query = FileQuery.From(root)
                         .UsingGitIgnore()
                         .Where(
                             "**",       // Exclude every file and directory.
                             "!logs/"    // ...then re-include only the 'logs' directory (trailing '/' = directory-only rule).
                         )
                         .Build();

    SampleUtils.RunDemo(
        title: "Directory-Only Matching",
        description: "The trailing '/' makes 'logs/' match the 'logs' directory itself (not its files): " +
                     "'**' excludes everything, then '!logs/' re-includes that directory and restores access to its files.",
        queryText: "FileQuery.From(root).UsingGitIgnore().Where(\"**\", \"!logs/\").Build()",
        query: query,
        expected: "The 2 files inside 'logs': 'logs/app.log' and 'logs/error.log'."
    );
}
finally {
    SampleUtils.Cleanup(root);
}
