using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows.Tests;

public sealed class WslStatusCheckTests
{
    [Fact]
    public async Task RunAsync_ReturnsPassWhenWslStatusSucceeds()
    {
        WslStatusCheck check = new(new FakeCommandRunner(new CommandRunResult
        {
            FileName = "wsl",
            Arguments = "--status",
            ExitCode = 0,
            StandardOutput = "Default Version: 2\nKernel version: 6.6.87.2"
        }));

        CheckResult result = await check.RunAsync(CancellationToken.None);

        Assert.Equal(CheckSeverity.Pass, result.Severity);
        Assert.Equal("Default Version: 2", result.CurrentValue);
        Assert.Contains("Linux-based", result.Impact);
    }

    [Fact]
    public async Task RunAsync_ReturnsWarningWhenWslCannotRun()
    {
        WslStatusCheck check = new(new FakeCommandRunner(new CommandRunResult
        {
            FileName = "wsl",
            Arguments = "--status",
            ErrorMessage = "The system cannot find the file specified."
        }));

        CheckResult result = await check.RunAsync(CancellationToken.None);

        Assert.Equal(CheckSeverity.Warning, result.Severity);
        Assert.Contains("cannot find", result.CurrentValue);
    }

    private sealed class FakeCommandRunner(CommandRunResult result) : ICommandRunner
    {
        public Task<CommandRunResult> RunAsync(string fileName, string arguments, TimeSpan timeout, CancellationToken cancellationToken)
        {
            Assert.Equal("wsl", fileName);
            Assert.Equal("--status", arguments);
            return Task.FromResult(result);
        }
    }
}
