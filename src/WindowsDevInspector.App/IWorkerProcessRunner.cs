using System.Diagnostics;

namespace WindowsDevInspector.App;

public interface IWorkerProcessRunner
{
    Task<int> RunAsync(ProcessStartInfo startInfo);
}
