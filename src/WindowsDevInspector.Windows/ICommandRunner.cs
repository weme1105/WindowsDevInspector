namespace WindowsDevInspector.Windows;

public interface ICommandRunner
{
    Task<CommandRunResult> RunAsync(string fileName, string arguments, TimeSpan timeout, CancellationToken cancellationToken);
}
