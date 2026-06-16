using Jeninnet.FileQuery;
using Jeninnet.FileQuery.Enums;

// If you don't see any results, change the folder.
var query = FileQueryBuilder.From(".")
                            .Where(PatternKind.Regex, ["r:^src/.*\\.cs$"])
                            .Execute();

foreach(var file in query)
{
    Console.WriteLine(file);
}
