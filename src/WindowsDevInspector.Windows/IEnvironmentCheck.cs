using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows;

public interface IEnvironmentCheck
{
    string Id { get; }

    Task<CheckResult> RunAsync(CancellationToken cancellationToken);
}
