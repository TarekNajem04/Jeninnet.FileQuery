namespace AdvancedUsage;

public interface IFileQueryCommand {
    Task ExecuteAsync(string root, string[] args, CancellationToken cancellationToken = default);
}
