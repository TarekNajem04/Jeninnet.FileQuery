//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
/*
 * Purpose: POSIX character classes in glob patterns.
 * '[[:digit:]]' matches any decimal digit inside a glob.
 */

var root = SampleUtils.CreateDemoTree("POSIXClassMatching");

try {
    var query = FileQuery.From(root)
                         .UsingGlob()
                         .Where("**/*[[:digit:]].*")
                         .Build();

    SampleUtils.RunDemo(
        title: "POSIX Class Matching",
        description: "The POSIX class '[[:digit:]]' matches any digit, so only files whose name contains a digit match.",
        queryText: "FileQuery.From(root).UsingGlob().Where(\"**/*[[:digit:]].*\").Build()",
        query: query,
        expected: "The 2 images with digits in their names: 'logo1.png', 'logo2.png'."
    );
}
finally {
    SampleUtils.Cleanup(root);
}
