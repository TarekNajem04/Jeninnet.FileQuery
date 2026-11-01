//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
using System.Diagnostics;
using System.Globalization;

namespace Jeninnet.FileQuery.Tool.Tests.Git;

/// <summary>
/// Creates a disposable Git repository for change-detection tests.
/// </summary>
internal sealed class TemporaryGitRepository : IDisposable {
    private readonly TestEnvironment _environment = new();

    /// <summary>
    /// Gets the repository root.
    /// </summary>
    public string Root => _environment.Root;

    /// <summary>
    /// Initializes a new repository with an identity configured for test commits.
    /// </summary>
    public TemporaryGitRepository() {
        Run("init", "--quiet");
        Run("config", "user.email", "tests@example.com");
        Run("config", "user.name", "Jeninnet Tests");
        Run("config", "commit.gpgsign", "false");
    }

    /// <summary>
    /// Creates or overwrites a file in the repository.
    /// </summary>
    /// <param name="relativePath">The repository-relative path.</param>
    /// <param name="contents">The file contents.</param>
    /// <returns>The absolute file path.</returns>
    public string Write(string relativePath, string contents = "content") =>
        _environment.CreateFile(relativePath, contents);

    /// <summary>
    /// Deletes a file from the working tree.
    /// </summary>
    /// <param name="relativePath">The repository-relative path.</param>
    public void Delete(string relativePath) =>
        File.Delete(Path.Combine(Root, relativePath.Replace('/', Path.DirectorySeparatorChar)));

    /// <summary>
    /// Stages all changes and creates a commit.
    /// </summary>
    /// <param name="message">The commit message.</param>
    /// <param name="date">The optional author and committer date.</param>
    public void Commit(string message, DateTimeOffset? date = null) {
        Run("add", "-A");
        RunWithDate(date, "commit", "--quiet", "-m", message);
    }

    /// <summary>
    /// Runs a Git command in the repository.
    /// </summary>
    /// <param name="arguments">The Git arguments.</param>
    /// <returns>The captured standard output.</returns>
    public string Run(params string[] arguments) => RunWithDate(null, arguments);

    /// <summary>
    /// Runs a Git command with an optional commit date.
    /// </summary>
    /// <param name="date">The commit date applied through the Git environment variables.</param>
    /// <param name="arguments">The Git arguments.</param>
    /// <returns>The captured standard output.</returns>
    private string RunWithDate(DateTimeOffset? date, params string[] arguments) {
#pragma warning disable S4036 // Tests run the user-installed Git from PATH by design.
        var startInfo = new ProcessStartInfo("git") {
            WorkingDirectory = Root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
#pragma warning restore S4036

        foreach(var argument in arguments) {
            startInfo.ArgumentList.Add(argument);
        }

        if(date is not null) {
            var timestamp = date.Value.ToUniversalTime()
                .ToString("yyyy-MM-ddTHH:mm:ss'Z'", CultureInfo.InvariantCulture);
            startInfo.Environment["GIT_AUTHOR_DATE"] = timestamp;
            startInfo.Environment["GIT_COMMITTER_DATE"] = timestamp;
        }

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start git.");

        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if(process.ExitCode != 0) {
            throw new InvalidOperationException(
                $"git {string.Join(' ', arguments)} failed: {error.Trim()}");
        }

        return output;
    }

    /// <inheritdoc/>
    public void Dispose() => _environment.Dispose();
}
