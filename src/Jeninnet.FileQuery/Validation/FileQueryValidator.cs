//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
namespace Jeninnet.FileQuery.Validation;

/// <summary>
/// Provides a centralized validation pipeline for file query configuration and execution parameters.
/// </summary>
internal static class FileQueryValidator {
    /// <summary>
    /// Validates the provided configuration and parameters before engine execution.
    /// </summary>
    /// <param name="fileSystem">The file system abstraction.</param>
    /// <param name="rootPath">The root directory path.</param>
    /// <param name="options">The execution options.</param>
    /// <exception cref="ArgumentNullException">Thrown if rootPath or options are null.</exception>
    /// <exception cref="ArgumentException">Thrown if configuration is invalid.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the root path is invalid.</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown if the root path does not exist.</exception>
    public static void ValidateExecution(IFileSystem fileSystem, string? rootPath, FileQueryOptions? options) {
        if(string.IsNullOrWhiteSpace(rootPath)) {
            throw new InvalidOperationException("Root path must be specified.");
        }

        if(rootPath.IndexOfAny(Path.GetInvalidPathChars()) >= 0) {
            throw new ArgumentException("Root path contains invalid characters.", nameof(rootPath));
        }

        if(rootPath.Length > 4096) {
            throw new ArgumentException("Root path exceeds maximum reasonable length.", nameof(rootPath));
        }

        if(rootPath.StartsWith(@"\\", StringComparison.Ordinal) && rootPath.Length < 5) {
            throw new ArgumentException("UNC paths must specify a server and share.", nameof(rootPath));
        }

        if(!fileSystem.DirectoryExists(rootPath)) {
            throw new DirectoryNotFoundException($"The specified root path does not exist: '{rootPath}'");
        }

        if(options is null) {
            throw new InvalidOperationException("Execution options must be provided.");
        }

        options.Validate();
    }
}
