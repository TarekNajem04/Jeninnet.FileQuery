//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
using System.Globalization;

namespace Jeninnet.FileQuery.Benchmarks;

/// <summary>
/// Creates and removes the deterministic dataset used by the cross-platform benchmarks.
/// </summary>
internal static class BenchmarkDataset {
    /// <summary>The number of directories in the dataset.</summary>
    public const int DIRECTORY_COUNT = 100;

    /// <summary>The number of files per directory in the dataset.</summary>
    public const int FILES_PER_DIRECTORY = 100;

    private static readonly string[] _extensions = [".cs", ".txt", ".log", ".json"];

    /// <summary>
    /// Creates a dataset under the system temporary directory.
    /// </summary>
    /// <param name="prefix">A short prefix used in the directory name.</param>
    /// <returns>The dataset root.</returns>
    public static string Create(string prefix) {
        var root = Path.Combine(
            Path.GetTempPath(),
            $"jeninnet-{prefix}-benchmark-" + Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(root);

        for(var directory = 0; directory < DIRECTORY_COUNT; directory++) {
            var directoryPath = Path.Combine(
                root,
                "dir" + directory.ToString("D3", CultureInfo.InvariantCulture));

            Directory.CreateDirectory(directoryPath);

            for(var file = 0; file < FILES_PER_DIRECTORY; file++) {
                var extension = _extensions[file % _extensions.Length];
                var fileName = "file"
                    + file.ToString("D3", CultureInfo.InvariantCulture)
                    + extension;

                File.WriteAllText(Path.Combine(directoryPath, fileName), string.Empty);
            }
        }

        return root;
    }

    /// <summary>
    /// Deletes a dataset directory, ignoring cleanup failures.
    /// </summary>
    /// <param name="root">The dataset root, or <see langword="null"/>.</param>
    public static void Delete(string? root) {
        if(string.IsNullOrEmpty(root)) {
            return;
        }

        try {
            Directory.Delete(root, recursive: true);
        }
        catch(IOException) {
            // Best-effort cleanup; the temporary directory is safe to leave behind.
        }
        catch(UnauthorizedAccessException) {
            // Best-effort cleanup; the temporary directory is safe to leave behind.
        }
    }
}
