//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Soak;

/// <summary>
/// Options for a soak run.
/// </summary>
/// <param name="DurationSeconds">Measured duration in seconds.</param>
/// <param name="WarmupSeconds">Warmup duration in seconds.</param>
/// <param name="Directories">Number of directories in the synthetic dataset.</param>
/// <param name="FilesPerDirectory">Number of files per directory.</param>
/// <param name="MaxGrowthMegabytes">Allowed managed heap growth over the run.</param>
/// <param name="SampleInterval">Iterations between heap samples.</param>
/// <param name="Query">The pattern applied on every iteration.</param>
/// <param name="OutputDirectory">Directory that receives the JSON and Markdown reports.</param>
internal sealed record SoakOptions(
    int DurationSeconds,
    int WarmupSeconds,
    int Directories,
    int FilesPerDirectory,
    double MaxGrowthMegabytes,
    int SampleInterval,
    string Query,
    string OutputDirectory
) {
    /// <summary>
    /// The default options: 20 seconds measured, 3 seconds warmup, 2,500 files.
    /// </summary>
    public static SoakOptions Default { get; } = new(
        DurationSeconds: 20,
        WarmupSeconds: 3,
        Directories: 50,
        FilesPerDirectory: 50,
        MaxGrowthMegabytes: 4,
        SampleInterval: 10,
        Query: "**;!*.cs",
        OutputDirectory: Path.Combine("artifacts", "soak"));

    /// <summary>
    /// The command-line help text.
    /// </summary>
    public const string USAGE =
        """
        Jeninnet.FileQuery soak harness

        Usage:
          dotnet run --project src/Jeninnet.FileQuery.Soak -c Release -- [options]

        Options:
          --duration-seconds <n>    Measured duration (default 20)
          --warmup-seconds <n>      Warmup duration (default 3)
          --directories <n>         Dataset directories (default 50)
          --files-per-directory <n> Files per directory (default 50)
          --max-growth-mb <n>       Allowed managed heap growth (default 4)
          --sample-interval <n>     Iterations between heap samples (default 10)
          --query <pattern>         Query pattern (default "**;!*.cs")
          --output <dir>            Report directory (default artifacts/soak)
          --help                    Show this help
        """;

    /// <summary>
    /// Parses command-line arguments.
    /// </summary>
    /// <param name="args">The raw arguments.</param>
    /// <param name="options">The parsed options.</param>
    /// <param name="error">The parse error, when any.</param>
    /// <returns><see langword="true"/> when parsing succeeded.</returns>
    public static bool TryParse(string[] args, out SoakOptions? options, out string? error) {
        ArgumentNullException.ThrowIfNull(args);

        options = Default;
        error = null;
        var index = 0;

        while(index < args.Length) {
            var argument = args[index++];

            if(argument is "--help" or "-h") {
                error = USAGE;
                return false;
            }

            if(index >= args.Length) {
                error = $"Missing value for '{argument}'.";
                return false;
            }

            var value = args[index++];

            switch(argument) {
                case "--duration-seconds":
                    if(!TryParseInt(value, 1, out var duration)) {
                        error = $"Invalid --duration-seconds value '{value}'.";
                        return false;
                    }

                    options = options with { DurationSeconds = duration };
                    break;
                case "--warmup-seconds":
                    if(!TryParseInt(value, 0, out var warmup)) {
                        error = $"Invalid --warmup-seconds value '{value}'.";
                        return false;
                    }

                    options = options with { WarmupSeconds = warmup };
                    break;
                case "--directories":
                    if(!TryParseInt(value, 1, out var directories)) {
                        error = $"Invalid --directories value '{value}'.";
                        return false;
                    }

                    options = options with { Directories = directories };
                    break;
                case "--files-per-directory":
                    if(!TryParseInt(value, 1, out var files)) {
                        error = $"Invalid --files-per-directory value '{value}'.";
                        return false;
                    }

                    options = options with { FilesPerDirectory = files };
                    break;
                case "--max-growth-mb":
                    if(!double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var growth) || growth < 0) {
                        error = $"Invalid --max-growth-mb value '{value}'.";
                        return false;
                    }

                    options = options with { MaxGrowthMegabytes = growth };
                    break;
                case "--sample-interval":
                    if(!TryParseInt(value, 1, out var interval)) {
                        error = $"Invalid --sample-interval value '{value}'.";
                        return false;
                    }

                    options = options with { SampleInterval = interval };
                    break;
                case "--query":
                    if(string.IsNullOrWhiteSpace(value)) {
                        error = "--query must not be empty.";
                        return false;
                    }

                    options = options with { Query = value };
                    break;
                case "--output":
                    if(string.IsNullOrWhiteSpace(value)) {
                        error = "--output must not be empty.";
                        return false;
                    }

                    options = options with { OutputDirectory = value };
                    break;
                default:
                    error = $"Unknown option '{argument}'.";
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Parses a non-negative integer with a lower bound.
    /// </summary>
    /// <param name="value">The raw value.</param>
    /// <param name="minimum">The inclusive minimum.</param>
    /// <param name="result">The parsed value.</param>
    /// <returns><see langword="true"/> when parsing succeeded.</returns>
    private static bool TryParseInt(string value, int minimum, out int result) =>
        int.TryParse(value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out result) &&
        result >= minimum;
}
