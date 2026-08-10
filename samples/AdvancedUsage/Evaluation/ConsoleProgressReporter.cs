namespace AdvancedUsage.Evaluation;

/// <summary>
/// Renders dataset-generation progress to the console: wall-clock-timestamped
/// phase events, time-throttled single-line progress updates, and a phase-
/// duration summary. Timestamps are wall-clock and for display only; all
/// duration and throttle measurements use a monotonic stopwatch.
/// </summary>
public sealed class ConsoleProgressReporter : IProgress<GenerationProgress>, IDisposable {
    private const string RESET_CODE = "\u001b[0m";
    private const string RED_CODE = "\u001b[31m";
    private const string YELLOW_CODE = "\u001b[33m";
    private const string GREEN_CODE = "\u001b[32m";
    private const string CYAN_CODE = "\u001b[36m";
    private const string ERASE_LINE_CODE = "\u001b[2K";
    private const int PROGRESS_LINE_WIDTH = 100;
    private const int DETAIL_INDENT = 4;

    private static readonly TimeSpan _minimumRenderInterval = TimeSpan.FromSeconds(1);

    private readonly Lock _gate = new();
    private readonly bool _useColor;
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private readonly Stopwatch _filePhaseElapsed = new();

    private bool _filePhaseActive;
    private bool _progressLineActive;
    private bool _cursorHidden;
    private long _phaseStartTicks;
    private TimeSpan _lastRenderElapsed;
    private int _lastRenderedPercent = -1;
    private DateTimeOffset _generationStartedAt;
    private DateTimeOffset _filePhaseStartedAt;
    private DateTimeOffset _filePhaseCompletedAt;
    private TimeSpan? _directoryTreeDuration;
    private TimeSpan? _fileGenerationDuration;
    private TimeSpan? _manifestWriteDuration;
    private TimeSpan? _validationDuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleProgressReporter"/> class.
    /// Color is disabled automatically when output is redirected or when the
    /// NO_COLOR environment variable is set.
    /// </summary>
    public ConsoleProgressReporter() =>
        _useColor = !Console.IsOutputRedirected &&
                    string.IsNullOrEmpty(Environment.GetEnvironmentVariable("NO_COLOR"));

    /// <summary>
    /// Reports a dataset-generation progress notification.
    /// </summary>
    /// <param name="value">The progress notification to render.</param>
    public void Report(GenerationProgress value) {
        lock(_gate) {
            if(value.Phase == DatasetGenerationPhase.GeneratingFiles && value.Message.Length == 0) {
                RenderFileProgress(value);
                return;
            }

            RecordPhase(value);
            WriteEventLine(value);

            if(value.Phase == DatasetGenerationPhase.GeneratingFiles &&
               value.Severity == GeneratorProgressSeverity.Success) {
                WriteFileCompletionSummary(value);
            }

            if(value.Phase == DatasetGenerationPhase.Completed) {
                WriteSummary(value);
            }
        }
    }

    /// <summary>
    /// Renders a failure in red and clears any active progress line.
    /// </summary>
    /// <param name="message">The error message to display.</param>
    public void ReportError(string message) {
        lock(_gate) {
            ClearActiveProgressLine();
            Write(_useColor ? $"{RED_CODE}[✗] {message}{RESET_CODE}" : $"[✗] {message}");
        }
    }

    /// <summary>
    /// Restores the console state after progress rendering.
    /// </summary>
    public void Dispose() {
        lock(_gate) {
            ClearActiveProgressLine();
            RestoreCursor();
        }
    }

    private void RecordPhase(GenerationProgress progress) {
        switch(progress.Phase, progress.Severity) {
            case (DatasetGenerationPhase.ConfigurationValidated, GeneratorProgressSeverity.Success):
                _generationStartedAt = DateTimeOffset.Now;
                _clock.Restart();
                break;

            case (DatasetGenerationPhase.ExtensionDistributionCalculated, _):
                MarkPhaseStart();
                break;

            case (DatasetGenerationPhase.DirectoryTreeGenerated, _):
                _directoryTreeDuration = MeasurePhase();
                break;

            case (DatasetGenerationPhase.GeneratingFiles, GeneratorProgressSeverity.Info):
                _filePhaseStartedAt = DateTimeOffset.Now;
                _filePhaseElapsed.Restart();
                MarkPhaseStart();
                break;

            case (DatasetGenerationPhase.GeneratingFiles, GeneratorProgressSeverity.Success):
                _filePhaseCompletedAt = DateTimeOffset.Now;
                _fileGenerationDuration = _filePhaseElapsed.Elapsed;
                break;

            case (DatasetGenerationPhase.WritingManifest, GeneratorProgressSeverity.Info):
                MarkPhaseStart();
                break;

            case (DatasetGenerationPhase.WritingManifest, GeneratorProgressSeverity.Success):
                _manifestWriteDuration = MeasurePhase();
                break;

            case (DatasetGenerationPhase.ValidatingDataset, GeneratorProgressSeverity.Info):
                MarkPhaseStart();
                break;

            case (DatasetGenerationPhase.ValidatingDataset, GeneratorProgressSeverity.Success):
                _validationDuration = MeasurePhase();
                break;
        }
    }

    private void MarkPhaseStart() => _phaseStartTicks = _clock.ElapsedTicks;

    private TimeSpan? MeasurePhase() {
        var elapsed = TimeSpan.FromTicks(_clock.ElapsedTicks - _phaseStartTicks);
        return TimeSpan.Zero > elapsed ? TimeSpan.Zero : elapsed;
    }

    private void RenderFileProgress(GenerationProgress progress) {
        var generated = progress.GeneratedFileCount;
        var target = progress.TargetFileCount;

        if(target <= 0 || generated < 0 || generated > target) {
            return;
        }

        if(!_filePhaseActive) {
            _filePhaseActive = true;
            _filePhaseElapsed.Restart();
        }

        var percent = 100.0 * generated / target;
        var now = _clock.Elapsed;

        if((int)percent == _lastRenderedPercent && now - _lastRenderElapsed < _minimumRenderInterval) {
            return;
        }

        _lastRenderedPercent = (int)percent;
        _lastRenderElapsed = now;

        var line = BuildProgressLine(progress, percent, _filePhaseElapsed.Elapsed);

        if(_useColor) {
            if(!_progressLineActive) {
                HideCursor();
                _progressLineActive = true;
            }

            line = line.PadRight(PROGRESS_LINE_WIDTH);
            Console.Out.Write($"\r{CYAN_CODE}{line}{RESET_CODE}");
        } else {
            Console.Out.WriteLine(line);
        }

        Console.Out.Flush();
    }

    private static string BuildProgressLine(GenerationProgress progress, double percent, TimeSpan elapsed) {
        var generated = progress.GeneratedFileCount;
        var target = progress.TargetFileCount;

        var rate = elapsed.TotalSeconds > 0 ? generated / elapsed.TotalSeconds : 0;
        var remaining = target - generated;
        var eta = rate > 0 ? remaining / rate : 0;

        var hasRate = elapsed >= TimeSpan.FromSeconds(1);
        var hasEta = hasRate && eta >= 1;

        var rateSegment = hasRate ? $"  {rate:N0} files/sec" : string.Empty;
        var etaSegment = hasEta ? $"  ETA {FormatEta(TimeSpan.FromSeconds(eta))}" : string.Empty;

        return $"{Stamp()}     {generated:N0} / {target:N0} files ({percent:0.0}%){rateSegment}{etaSegment}";
    }

    private void WriteEventLine(GenerationProgress progress) => Write($"{Stamp()} {RenderHeading(progress)}");

    private void WriteSummary(GenerationProgress progress) {
        var files = progress.GeneratedFileCount;

        WriteDetail("Started at", $"{_generationStartedAt:HH\\:mm\\:ss}");

        if(_directoryTreeDuration is { } directoryTree) {
            WriteDetail("Directory tree", FormatDuration(directoryTree));
        }

        WriteDetail("Files generated", $"{files:N0}");

        if(_fileGenerationDuration is { } fileGeneration) {
            WriteDetail("File generation", FormatDuration(fileGeneration));
            WriteDetail("Average rate", $"{files / fileGeneration.TotalSeconds:0.0} files/sec");
        }

        if(_manifestWriteDuration is { } manifestWrite) {
            WriteDetail("Manifest write", FormatDuration(manifestWrite));
        }

        if(_validationDuration is { } validation) {
            WriteDetail("Validation", FormatDuration(validation));
        }

        WriteDetail("Total duration", FormatDuration(_clock.Elapsed));
    }

    private void WriteFileCompletionSummary(GenerationProgress progress) {
        var files = progress.GeneratedFileCount;

        WriteDetail("Started at", $"{_filePhaseStartedAt:HH\\:mm\\:ss}");
        WriteDetail("Completed at", $"{_filePhaseCompletedAt:HH\\:mm\\:ss}");
        WriteDetail("Files generated", $"{files:N0}");

        if(_fileGenerationDuration is { } duration) {
            WriteDetail("Duration", FormatDuration(duration));
            WriteDetail("Average rate", $"{files / duration.TotalSeconds:0.0} files/sec");
        }
    }

    private void WriteDetail(string label, string value) =>
        Write($"{Stamp()} {new string(' ', DETAIL_INDENT)}{label,-15}: {value}");

    private string RenderHeading(GenerationProgress progress) {
        var (symbol, code) = progress.Severity switch {
            GeneratorProgressSeverity.Success => ("✓", GREEN_CODE),
            GeneratorProgressSeverity.Warning => ("!", YELLOW_CODE),
            GeneratorProgressSeverity.Error => ("✗", RED_CODE),
            _ => ("→", CYAN_CODE)
        };

        var line = $"[{symbol}] {progress.Message}";

        if(!_useColor) {
            return line;
        }

        return progress.Severity is GeneratorProgressSeverity.Error or GeneratorProgressSeverity.Warning
            ? code + line + RESET_CODE
            : code + line[..3] + RESET_CODE + line[3..];
    }

    private void Write(string text) {
        ClearActiveProgressLine();
        Console.Out.Write(text);
        Console.Out.Write(Environment.NewLine);
        Console.Out.Flush();
    }

    private void ClearActiveProgressLine() {
        if(!_progressLineActive) {
            return;
        }

        _progressLineActive = false;
        RestoreCursor();

        if(_useColor) {
            Console.Out.Write($"\r{ERASE_LINE_CODE}");
        } else {
            Console.Out.Write($"\r{new string(' ', PROGRESS_LINE_WIDTH)}\r");
        }

        Console.Out.Flush();
    }

    private void HideCursor() {
        if(_cursorHidden || !_useColor) {
            return;
        }

        _cursorHidden = true;
        TrySetCursorVisible(false);
    }

    private void RestoreCursor() {
        if(!_cursorHidden) {
            return;
        }

        _cursorHidden = false;
        TrySetCursorVisible(true);
    }

    private static void TrySetCursorVisible(bool visible) {
        try {
            Console.CursorVisible = visible;
        }
        catch(IOException) {
            // Cursor visibility is unavailable for redirected console output.
        }
        catch(PlatformNotSupportedException) {
            // Cursor visibility is unavailable on this platform.
        }
    }

    private static string Stamp() => $"[{DateTimeOffset.Now:HH\\:mm\\:ss}]";

    private static string FormatDuration(TimeSpan duration) =>
        duration.ToString(@"hh\:mm\:ss\.fff", CultureInfo.InvariantCulture);

    private static string FormatEta(TimeSpan eta) =>
        eta.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);
}
