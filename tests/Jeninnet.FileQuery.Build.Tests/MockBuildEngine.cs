//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Build.Tests;

/// <summary>
/// Captures MSBuild diagnostics emitted by a task.
/// </summary>
internal sealed class MockBuildEngine : IBuildEngine {
    /// <summary>Gets the captured errors.</summary>
    public List<BuildErrorEventArgs> Errors { get; } = [];

    /// <summary>Gets the captured warnings.</summary>
    public List<BuildWarningEventArgs> Warnings { get; } = [];

    /// <summary>Gets the captured messages.</summary>
    public List<BuildMessageEventArgs> Messages { get; } = [];

    public bool ContinueOnError => false;

    public int LineNumberOfTaskNode => 0;

    public int ColumnNumberOfTaskNode => 0;

    public string ProjectFileOfTaskNode => string.Empty;

    public bool BuildProjectFile(
        string projectFileName,
        string[] targetNames,
        IDictionary globalProperties,
        IDictionary targetOutputs
    ) => false;

    public void LogErrorEvent(BuildErrorEventArgs e) => Errors.Add(e);

    public void LogWarningEvent(BuildWarningEventArgs e) => Warnings.Add(e);

    public void LogMessageEvent(BuildMessageEventArgs e) => Messages.Add(e);

    public void LogCustomEvent(CustomBuildEventArgs e) { }
}
