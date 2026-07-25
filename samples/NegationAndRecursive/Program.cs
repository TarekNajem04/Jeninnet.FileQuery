using Jeninnet.FileQuery;
using Jeninnet.FileQuery.Enums;

// If you don't see any results, change the folder.
var query = FileQueryBuilder.From(".")
                            .Where("**/bin/**")
                            .Where(PatternKind.GitIgnore, ["!**/bin/debug/**"])
                            .Execute();

foreach(var file in query) {
    Console.WriteLine(file);
}
