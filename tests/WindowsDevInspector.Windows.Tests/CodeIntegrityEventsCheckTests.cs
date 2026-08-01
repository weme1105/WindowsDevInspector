using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows.Tests;

public sealed class CodeIntegrityEventsCheckTests
{
    [Fact]
    public async Task RunAsync_ReturnsInfoWhenRecentEventsExist()
    {
        CodeIntegrityEventsCheck check = new(new FakeCommandRunner(new CommandRunResult
        {
            FileName = "wevtutil",
            Arguments = "qe Microsoft-Windows-CodeIntegrity/Operational /c:5 /rd:true /f:text",
            ExitCode = 0,
            StandardOutput = """
            Event[0]:
              Log Name: Microsoft-Windows-CodeIntegrity/Operational
              Date: 2026-08-02T08:00:00.0000000Z
              Event ID: 3089
            """
        }));

        CheckResult result = await check.RunAsync(CancellationToken.None);

        Assert.Equal(CheckSeverity.Info, result.Severity);
        Assert.Contains("Event ID: 3089", result.CurrentValue);
        Assert.Contains("Date:", result.CurrentValue);
    }

    [Fact]
    public async Task RunAsync_ReturnsInfoWhenEventLogCannotBeQueried()
    {
        CodeIntegrityEventsCheck check = new(new FakeCommandRunner(new CommandRunResult
        {
            FileName = "wevtutil",
            Arguments = "qe Microsoft-Windows-CodeIntegrity/Operational /c:5 /rd:true /f:text",
            ExitCode = 15007,
            StandardError = "The specified channel could not be found."
        }));

        CheckResult result = await check.RunAsync(CancellationToken.None);

        Assert.Equal(CheckSeverity.Info, result.Severity);
        Assert.Contains("channel", result.CurrentValue);
    }

    private sealed class FakeCommandRunner(CommandRunResult result) : ICommandRunner
    {
        public Task<CommandRunResult> RunAsync(string fileName, string arguments, TimeSpan timeout, CancellationToken cancellationToken)
        {
            Assert.Equal("wevtutil", fileName);
            Assert.Equal("qe Microsoft-Windows-CodeIntegrity/Operational /c:5 /rd:true /f:text", arguments);
            return Task.FromResult(result);
        }
    }
}
