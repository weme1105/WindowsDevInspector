namespace WindowsDevInspector.ElevatedWorker;

public sealed class WorkerCommandRouter(
    Func<string, Task<int>> runRemediationAsync,
    Func<string, Task<int>> runRollbackAsync,
    Func<string, Task<int>> runInstallationAsync)
{
    public Task<int> RouteAsync(WorkerCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        return command.Mode switch
        {
            WorkerCommandMode.Remediation => runRemediationAsync(command.InputPath),
            WorkerCommandMode.Rollback => runRollbackAsync(command.InputPath),
            WorkerCommandMode.Installation => runInstallationAsync(command.InputPath),
            _ => throw new ArgumentOutOfRangeException(nameof(command), command.Mode, "Unsupported worker command mode.")
        };
    }
}
