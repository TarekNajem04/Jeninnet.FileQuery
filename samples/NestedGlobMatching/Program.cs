using Jeninnet.FileQuery;

// If you don't see any results, change the folder.
var query = FileQueryBuilder.From(".")
                            .Where("**/*.{cs,csproj}")
                            .Execute();

foreach(var file in query)
{
    Console.WriteLine(file);
}
