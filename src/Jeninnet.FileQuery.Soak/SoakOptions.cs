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

        var current = Default;
        error = null;
        var index = 0;

        while(index < args.Length) {
            var argument = args[index++];

            if(argument is "--help" or "-h") {
                options = current;
                error = USAGE;
                return false;
            }

            if(index >= args.Length) {
                options = current;
                error = $"Missing value for '{argument}'.";
                return false;
            }

            var value = args[index++];

            if(!TryApplyOption(argument, value, current, out var updated, out error)) {
                options = current;
                return false;
            }

            current = updated;
        }

        options = current;
        return true;
    }

    /// <summary>
    /// Applies one command-line option to the current options.
    /// </summary>
    /// <param name="argument">The option name.</param>
    /// <param name="value">The option value.</param>
    /// <param name="options">The current options.</param>
    /// <param name="updated">The updated options.</param>
    /// <param name="error">The parse error, when any.</param>
    /// <returns><see langword="true"/> when the option was valid.</returns>
    private static bool TryApplyOption(
        string argument,
        string value,
        SoakOptions options,
        out SoakOptions updated,
        out string? error
    ) {
        switch(argument) {
            case "--duration-seconds":
                return TryApplyIntOption(value, 1, argument, options, static (current, parsed) => current with { DurationSeconds = parsed }, out updated, out error);
            case "--warmup-seconds":
                return TryApplyIntOption(value, 0, argument, options, static (current, parsed) => current with { WarmupSeconds = parsed }, out updated, out error);
            case "--directories":
                return TryApplyIntOption(value, 1, argument, options, static (current, parsed) => current with { Directories = parsed }, out updated, out error);
            case "--files-per-directory":
                return TryApplyIntOption(value, 1, argument, options, static (current, parsed) => current with { FilesPerDirectory = parsed }, out updated, out error);
            case "--max-growth-mb":
                return TryApplyGrowthOption(value, options, out updated, out error);
            case "--sample-interval":
                return TryApplyIntOption(value, 1, argument, options, static (current, parsed) => current with { SampleInterval = parsed }, out updated, out error);
            case "--query":
                return TryApplyStringOption(value, argument, options, static (current, parsed) => current with { Query = parsed }, out updated, out error);
            case "--output":
                return TryApplyStringOption(value, argument, options, static (current, parsed) => current with { OutputDirectory = parsed }, out updated, out error);
            default:
                updated = options;
                error = $"Unknown option '{argument}'.";
                return false;
        }
    }

    /// <summary>
    /// Applies an integer option with a lower bound.
    /// </summary>
    /// <param name="value">The raw value.</param>
    /// <param name="minimum">The inclusive minimum.</param>
    /// <param name="option">The option name.</param>
    /// <param name="options">The current options.</param>
    /// <param name="update">The update operation.</param>
    /// <param name="updated">The updated options.</param>
    /// <param name="error">The parse error, when any.</param>
    /// <returns><see langword="true"/> when the value was valid.</returns>
    private static bool TryApplyIntOption(
        string value,
        int minimum,
        string option,
        SoakOptions options,
        Func<SoakOptions, int, SoakOptions> update,
        out SoakOptions updated,
        out string? error
    ) {
        updated = options;

        if(!TryParseInt(value, minimum, out var parsed)) {
            error = $"Invalid {option} value '{value}'.";
            return false;
        }

        updated = update(options, parsed);
        error = null;
        return true;
    }

    /// <summary>
    /// Applies the managed-heap growth option.
    /// </summary>
    /// <param name="value">The raw value.</param>
    /// <param name="options">The current options.</param>
    /// <param name="updated">The updated options.</param>
    /// <param name="error">The parse error, when any.</param>
    /// <returns><see langword="true"/> when the value was valid.</returns>
    private static bool TryApplyGrowthOption(
        string value,
        SoakOptions options,
        out SoakOptions updated,
        out string? error
    ) {
        updated = options;

        if(!double.TryParse(
            value,
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture,
            out var growth) || growth < 0) {
            error = $"Invalid --max-growth-mb value '{value}'.";
            return false;
        }

        updated = options with { MaxGrowthMegabytes = growth };
        error = null;
        return true;
    }

    /// <summary>
    /// Applies a non-empty string option.
    /// </summary>
    /// <param name="value">The raw value.</param>
    /// <param name="option">The option name.</param>
    /// <param name="options">The current options.</param>
    /// <param name="update">The update operation.</param>
    /// <param name="updated">The updated options.</param>
    /// <param name="error">The parse error, when any.</param>
    /// <returns><see langword="true"/> when the value was valid.</returns>
    private static bool TryApplyStringOption(
        string value,
        string option,
        SoakOptions options,
        Func<SoakOptions, string, SoakOptions> update,
        out SoakOptions updated,
        out string? error
    ) {
        updated = options;

        if(string.IsNullOrWhiteSpace(value)) {
            error = $"{option} must not be empty.";
            return false;
        }

        updated = update(options, value);
        error = null;
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
