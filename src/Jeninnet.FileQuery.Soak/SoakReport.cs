//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Soak;

/// <summary>
/// The serialized soak report.
/// </summary>
/// <param name="StartedAtUtc">When the run started (ISO 8601 UTC).</param>
/// <param name="OperatingSystem">The operating-system description.</param>
/// <param name="Runtime">The .NET runtime description.</param>
/// <param name="ProcessorCount">The logical processor count.</param>
/// <param name="Options">The effective soak options.</param>
/// <param name="DatasetFiles">The number of files in the dataset.</param>
/// <param name="Analysis">The computed analysis and verdict.</param>
/// <param name="Samples">The managed-heap samples.</param>
internal sealed record SoakReport(
    string StartedAtUtc,
    string OperatingSystem,
    string Runtime,
    int ProcessorCount,
    SoakOptions Options,
    int DatasetFiles,
    SoakAnalysis Analysis,
    IReadOnlyList<SoakSample> Samples
);
