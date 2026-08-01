using System.Diagnostics;

namespace WindowsDevInspector.App;

public sealed class WorkerProcessRunner : IWorkerProcessRunner
{
    public async Task<int> RunAsync(ProcessStartInfo startInfo)
    {
        using Process? process = Process.Start(startInfo);
        if (process is null)
        {
            throw new OperationCanceledException("ElevatedWorker did not start.");
        }

        await process.WaitForExitAsync();
        return process.ExitCode;
    }
}
