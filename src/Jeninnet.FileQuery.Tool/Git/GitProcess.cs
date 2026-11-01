//
// Copyright (c) Jeninnet.
// Part of the Jeninnet Platform.
// Platform Repository: https://github.com/TarekNajem04/Jeninnet.Platform/ [PENDING]
// Solution Repository: https://github.com/TarekNajem04/Jeninnet.FileQuery [LIVE]
// Licensed under the MIT License.
//
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace Jeninnet.FileQuery.Tool.Git;

/// <summary>
/// Runs the <c>git</c> executable and returns its captured output.
/// </summary>
internal static class GitProcess {
    private const int TIMEOUT_MILLISECONDS = 30_000;

    /// <summary>
    /// Runs Git in <paramref name="workingDirectory"/> with the supplied arguments.
    /// </summary>
    /// <param name="workingDirectory">The directory in which Git runs.</param>
    /// <param name="arguments">The Git arguments.</param>
    /// <returns>The exit code and captured output.</returns>
    /// <exception cref="GitException">
    /// Thrown when Git cannot be started or does not finish within the timeout.
    /// </exception>
    public static GitResult Run(string workingDirectory, params string[] arguments) {
        ArgumentException.ThrowIfNullOrWhiteSpace(workingDirectory);
        ArgumentNullException.ThrowIfNull(arguments);

#pragma warning disable S4036 // Git is a user-installed CLI; resolving it from PATH is the documented behaviour.
        var startInfo = new ProcessStartInfo("git") {
            WorkingDirectory = workingDirectory,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };
#pragma warning restore S4036

        foreach(var argument in arguments) {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = new Process { StartInfo = startInfo };

        try {
            process.Start();
        }
        catch(Win32Exception exception) {
            throw new GitException("Git is not installed or is not available on PATH.", exception);
        }

        // Closing stdin immediately lets commands such as 'hash-object --stdin' read an empty input.
        process.StandardInput.Close();

        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();

        if(!process.WaitForExit(TIMEOUT_MILLISECONDS)) {
            TryKill(process);
            throw new GitException($"Git timed out after {TIMEOUT_MILLISECONDS / 1000} seconds.");
        }

        return new GitResult(
            process.ExitCode,
            outputTask.GetAwaiter().GetResult(),
            errorTask.GetAwaiter().GetResult());
    }

    /// <summary>
    /// Terminates a Git process that exceeded the timeout.
    /// </summary>
    /// <param name="process">The process to terminate.</param>
    private static void TryKill(Process process) {
        try {
            process.Kill(entireProcessTree: true);
        }
        catch(Exception exception) when(exception is InvalidOperationException or Win32Exception) {
            // The process already exited between the timeout check and the kill request.
        }
    }
}

/// <summary>
/// Captures the result of a Git invocation.
/// </summary>
/// <param name="ExitCode">The process exit code.</param>
/// <param name="StandardOutput">The captured standard output.</param>
/// <param name="StandardError">The captured standard error.</param>
internal sealed record GitResult(int ExitCode, string StandardOutput, string StandardError);
