using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows.Tests;

public sealed class CommandVersionCheckTests
{
    [Fact]
    public async Task RunAsync_ReturnsPassWhenCommandSucceeds()
    {
        CommandVersionCheck check = new(
            "common.git",
            "Common",
            "Git CLI",
            "git",
            "--version",
            new FakeCommandRunner(new CommandRunResult
            {
                FileName = "git",
                Arguments = "--version",
                ExitCode = 0,
                StandardOutput = "git version 2.50.0"
            }));

        CheckResult result = await check.RunAsync(CancellationToken.None);

        Assert.Equal(CheckSeverity.Pass, result.Severity);
        Assert.Equal("git version 2.50.0", result.CurrentValue);
    }

    [Fact]
    public async Task RunAsync_ReturnsWarningWhenCommandFails()
    {
        CommandVersionCheck check = new(
            "common.git",
            "Common",
            "Git CLI",
            "git",
            "--version",
            new FakeCommandRunner(new CommandRunResult
            {
                FileName = "git",
                Arguments = "--version",
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
            return Task.FromResult(result);
        }
    }
}
