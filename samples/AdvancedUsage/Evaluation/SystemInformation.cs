//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace AdvancedUsage.Evaluation;

/// <summary>
/// Captures non-identifying hardware and runtime information for reproducible reports.
/// </summary>
public static class SystemInformation {
    /// <summary>
    /// Captures the current environment.
    /// </summary>
    /// <param name="datasetRoot">The root directory of the dataset.</param>
    public static SystemInformationSnapshot Capture(string datasetRoot) {
        var drive = new DriveInfo(Path.GetPathRoot(Path.GetFullPath(datasetRoot)) ?? Path.DirectorySeparatorChar.ToString());

        return new SystemInformationSnapshot(
            OperatingSystem: RuntimeInformation.OSDescription,
            Framework: RuntimeInformation.FrameworkDescription,
            RuntimeIdentifier: System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier,
            Architecture: RuntimeInformation.OSArchitecture.ToString(),
            ProcessArchitecture: RuntimeInformation.ProcessArchitecture.ToString(),
            ProcessorCount: Environment.ProcessorCount,
            ProcessorName: GetProcessorName(),
            TotalMemoryBytes: GC.GetGCMemoryInfo().TotalAvailableMemoryBytes,
            FileSystem: TryGetFileSystem(drive),
            DatasetDriveFormat: TryGetDriveFormat(drive)
        );
    }

    private static string GetProcessorName() {
        var environmentName = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER");
        if(!string.IsNullOrWhiteSpace(environmentName)) {
            return environmentName;
        }

        const string cpuInfoPath = "/proc/cpuinfo";
        if(File.Exists(cpuInfoPath)) {
            var modelNameLine = File.ReadLines(cpuInfoPath)
                .FirstOrDefault(static line => line.StartsWith("model name", StringComparison.OrdinalIgnoreCase));

            if(modelNameLine is not null) {
                var separator = modelNameLine.IndexOf(':');
                if(separator >= 0) {
                    return modelNameLine[(separator + 1)..].Trim();
                }
            }
        }

        return "Unknown";
    }

    private static string TryGetFileSystem(DriveInfo drive) {
        try {
            return drive.DriveFormat;
        }
        catch(IOException) {
            return "Unknown";
        }
        catch(UnauthorizedAccessException) {
            return "Unknown";
        }
    }

    private static string TryGetDriveFormat(DriveInfo drive) => TryGetFileSystem(drive);
}

/// <summary>
/// Represents non-identifying environment information captured for an evaluation.
/// </summary>
/// <param name="OperatingSystem">The operating system description.</param>
/// <param name="Framework">The .NET runtime framework description.</param>
/// <param name="RuntimeIdentifier">The runtime identifier.</param>
/// <param name="Architecture">The architecture of the operating system.</param>
/// <param name="ProcessArchitecture">The architecture of the process.</param>
/// <param name="ProcessorCount">The number of processors.</param>
/// <param name="ProcessorName">The name of the processor.</param>
/// <param name="TotalMemoryBytes">The total amount of memory in bytes.</param>
/// <param name="FileSystem">The file system of the drive.</param>
/// <param name="DatasetDriveFormat">The format of the dataset drive.</param>
public sealed record SystemInformationSnapshot(
    string OperatingSystem,
    string Framework,
    string RuntimeIdentifier,
    string Architecture,
    string ProcessArchitecture,
    int ProcessorCount,
    string ProcessorName,
    long TotalMemoryBytes,
    string FileSystem,
    string DatasetDriveFormat
);
