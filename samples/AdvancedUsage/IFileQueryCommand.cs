namespace AdvancedUsage;

/// <summary>
/// Defines the interface for a file query command.
/// </summary>
public interface IFileQueryCommand
{
    /// <summary>
    /// Executes the command asynchronously.
    /// </summary>
    /// <param name="root">The root directory.</param>
    /// <param name="args">The command arguments.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task ExecuteAsync(string root, string[] args, CancellationToken cancellationToken = default);
}
