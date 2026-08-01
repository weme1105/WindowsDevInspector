using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows.Tests;

public sealed class WslDistributionsCheckTests
{
    [Fact]
    public async Task RunAsync_ReturnsPassWhenDistributionsAreListed()
    {
        WslDistributionsCheck check = new(new FakeCommandRunner(new CommandRunResult
        {
            FileName = "wsl",
            Arguments = "--list --verbose",
            ExitCode = 0,
            StandardOutput = "  NAME      STATE           VERSION\n* Ubuntu    Running         2\n  Debian    Stopped         2"
        }));

        CheckResult result = await check.RunAsync(CancellationToken.None);

        Assert.Equal(CheckSeverity.Pass, result.Severity);
        Assert.Contains("Ubuntu", result.CurrentValue);
        Assert.Contains("Debian", result.CurrentValue);
        Assert.DoesNotContain("NAME", result.CurrentValue);
    }

    [Fact]
    public async Task RunAsync_ReturnsInfoWhenNoDistributionIsListed()
    {
        WslDistributionsCheck check = new(new FakeCommandRunner(new CommandRunResult
        {
            FileName = "wsl",
            Arguments = "--list --verbose",
            ExitCode = 0,
            StandardOutput = "Windows Subsystem for Linux has no installed distributions."
        }));

        CheckResult result = await check.RunAsync(CancellationToken.None);

        Assert.Equal(CheckSeverity.Info, result.Severity);
        Assert.Contains("No WSL distributions", result.CurrentValue);
    }

    private sealed class FakeCommandRunner(CommandRunResult result) : ICommandRunner
    {
        public Task<CommandRunResult> RunAsync(string fileName, string arguments, TimeSpan timeout, CancellationToken cancellationToken)
        {
            Assert.Equal("wsl", fileName);
            Assert.Equal("--list --verbose", arguments);
            return Task.FromResult(result);
        }
    }
}
