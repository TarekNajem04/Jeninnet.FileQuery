namespace Jeninnet.FileQuery.Benchmarks;

/*
 * Purpose:
 *  Measure the combined matcher pipeline performance when processing a mix of GitIgnore, Glob, and Regex patterns.
 */

[MemoryDiagnoser]
public class FileQueryBenchmark
{
    private IFileQueryEngine _engine = default!;
    private string _rootPath = default!;
    private FileQueryOptions _options = default!;

    [GlobalSetup]
    public void Setup()
    {
        _engine = FileQueryRuntime.Create();
        _rootPath = Directory.GetCurrentDirectory();
        _options = new FileQueryOptions(
            patternInput: new(
                patterns: [
                    "**",
                    "!**/*.txt"
                ]
            ),
            recurseSubdirectories: true
        );
    }

    [Benchmark]
    public void Match()
        => _ = _engine.Execute(new(_rootPath, _options)).ToList();
}
