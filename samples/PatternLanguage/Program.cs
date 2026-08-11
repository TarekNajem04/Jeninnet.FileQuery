/*
 * Purpose: the pattern language.
 * Demonstrates GitIgnore-style rules and last-role-wins evaluation:
 * each matching rule flips the role of the previous one.
 */

var root = SampleUtils.CreateDemoTree("PatternLanguage");

try {
    var query = FileQuery.From(root)
                         .UsingGitIgnore()
                         .Where(
                             "**",           // Exclude every file.
                             "!**/*.cs",     // ...then re-include every .cs file at any depth.
                             "**/cli/**"     // ...and finally exclude the 'cli' subtree again.
                         )
                         .Build();

    SampleUtils.RunDemo(
        title: "Pattern Language — GitIgnore-style rules",
        description: "Patterns are evaluated in order with last-role-wins semantics: '**' excludes all files, " +
                     "'!**/*.cs' re-includes C# files, and '**/cli/**' excludes the 'cli' subtree again — " +
                     "the last matching rule always decides.",
        queryText: "FileQuery.From(root).UsingGitIgnore().Where(\"**\", \"!**/*.cs\", \"**/cli/**\").Build()",
        query: query,
        expected: "The 3 .cs files outside 'cli': 'src/FileQuery.cs', 'src/Program.cs', 'src/test/helpers.cs'."
    );
}
finally {
    SampleUtils.Cleanup(root);
}
