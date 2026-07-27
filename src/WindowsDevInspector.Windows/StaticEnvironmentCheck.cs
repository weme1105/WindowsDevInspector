using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows;

public sealed class StaticEnvironmentCheck(CheckResult result) : IEnvironmentCheck
{
    public string Id => result.Id;

    public Task<CheckResult> RunAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(result);
    }
}
