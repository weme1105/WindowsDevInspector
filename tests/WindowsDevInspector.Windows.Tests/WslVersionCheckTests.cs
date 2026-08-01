using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows.Tests;

public sealed class WslVersionCheckTests
{
    [Fact]
    public async Task RunAsync_ReturnsPassWithVersionSummary()
    {
        WslVersionCheck check = new(new FakeCommandRunner(new CommandRunResult
        {
            FileName = "wsl",
            Arguments = "--version",
            ExitCode = 0,
            StandardOutput = "WSL version: 2.5.9.0\nKernel version: 6.6.87.2\nWSLg version: 1.0.66\nExtra: ignored"
        }));

        CheckResult result = await check.RunAsync(CancellationToken.None);

        Assert.Equal(CheckSeverity.Pass, result.Severity);
        Assert.Contains("WSL version", result.CurrentValue);
        Assert.DoesNotContain("Extra", result.CurrentValue);
        Assert.Contains("does not require the newest", result.Impact);
    }

    [Fact]
    public async Task RunAsync_ReturnsInfoWhenVersionCannotBeRead()
    {
        WslVersionCheck check = new(new FakeCommandRunner(new CommandRunResult
        {
            FileName = "wsl",
            Arguments = "--version",
            ExitCode = 1,
            StandardError = "Invalid command line option"
        }));

        CheckResult result = await check.RunAsync(CancellationToken.None);

        Assert.Equal(CheckSeverity.Info, result.Severity);
        Assert.Contains("Invalid command", result.CurrentValue);
    }

    private sealed class FakeCommandRunner(CommandRunResult result) : ICommandRunner
    {
        public Task<CommandRunResult> RunAsync(string fileName, string arguments, TimeSpan timeout, CancellationToken cancellationToken)
        {
            Assert.Equal("wsl", fileName);
            Assert.Equal("--version", arguments);
            return Task.FromResult(result);
        }
    }
}
