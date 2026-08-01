using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows.Tests;

public sealed class DotNetMauiWorkloadCheckTests
{
    [Fact]
    public async Task RunAsync_ReturnsPassWhenMauiWorkloadIsListed()
    {
        DotNetMauiWorkloadCheck check = new(new FakeCommandRunner(new CommandRunResult
        {
            FileName = "dotnet",
            Arguments = "workload list",
            ExitCode = 0,
            StandardOutput = "maui-windows 10.0.100"
        }));

        CheckResult result = await check.RunAsync(CancellationToken.None);

        Assert.Equal(CheckSeverity.Pass, result.Severity);
        Assert.Contains("maui", result.CurrentValue, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RunAsync_ReturnsInfoWhenMauiWorkloadIsMissing()
    {
        DotNetMauiWorkloadCheck check = new(new FakeCommandRunner(new CommandRunResult
        {
            FileName = "dotnet",
            Arguments = "workload list",
            ExitCode = 0,
            StandardOutput = "wasm-tools 10.0.100"
        }));

        CheckResult result = await check.RunAsync(CancellationToken.None);

        Assert.Equal(CheckSeverity.Info, result.Severity);
        Assert.Contains("not listed", result.CurrentValue);
    }

    private sealed class FakeCommandRunner(CommandRunResult result) : ICommandRunner
    {
        public Task<CommandRunResult> RunAsync(string fileName, string arguments, TimeSpan timeout, CancellationToken cancellationToken)
        {
            Assert.Equal("dotnet", fileName);
            Assert.Equal("workload list", arguments);
            return Task.FromResult(result);
        }
    }
}
