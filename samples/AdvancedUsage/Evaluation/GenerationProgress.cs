//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace AdvancedUsage.Evaluation;

/// <summary>
/// Identifies the dataset-generation or evaluation phase a progress notification belongs to.
/// </summary>
public enum DatasetGenerationPhase {
    /// <summary>
    /// The evaluation options were validated successfully.
    /// </summary>
    ConfigurationValidated = 0,

    /// <summary>
    /// An existing dataset directory tree is being deleted before regeneration.
    /// </summary>
    CleaningDirectory = 1,

    /// <summary>
    /// The runtime extension distribution has been calculated.
    /// </summary>
    ExtensionDistributionCalculated = 2,

    /// <summary>
    /// The directory tree has been generated.
    /// </summary>
    DirectoryTreeGenerated = 3,

    /// <summary>
    /// Files are being generated.
    /// </summary>
    GeneratingFiles = 4,

    /// <summary>
    /// The dataset manifest is being written.
    /// </summary>
    WritingManifest = 5,

    /// <summary>
    /// The generated dataset is being validated.
    /// </summary>
    ValidatingDataset = 6,

    /// <summary>
    /// The evaluation query is being executed for the first time to warm up the measurement pipeline.
    /// </summary>
    WarmUpExecution = 7,

    /// <summary>
    /// Dataset generation has completed or an existing dataset was reused.
    /// </summary>
    Completed = 8
}

/// <summary>
/// Classifies a progress notification for rendering and color coding.
/// </summary>
public enum GeneratorProgressSeverity {
    /// <summary>
    /// Neutral progress information for an in-flight step.
    /// </summary>
    Info = 0,

    /// <summary>
    /// A step or the overall generation completed successfully.
    /// </summary>
    Success = 1,

    /// <summary>
    /// A recoverable condition worth highlighting.
    /// </summary>
    Warning = 2,

    /// <summary>
    /// A failure occurred.
    /// </summary>
    Error = 3
}

/// <summary>
/// Describes a single dataset-generation progress notification.
/// </summary>
/// <param name="Phase">The generation phase the notification refers to.</param>
/// <param name="Severity">The notification severity used for color and rendering.</param>
/// <param name="Message">An optional phase message. An empty message marks a raw file-progress update.</param>
/// <param name="GeneratedFileCount">The number of files generated so far.</param>
/// <param name="TargetFileCount">The total number of files targeted by generation.</param>
public readonly record struct GenerationProgress(
    DatasetGenerationPhase Phase,
    GeneratorProgressSeverity Severity,
    string Message,
    int GeneratedFileCount,
    int TargetFileCount);
