using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows.Tests;

public sealed class FirewallProfilesCheckTests
{
    [Fact]
    public async Task RunAsync_ReturnsPassWhenProfilesAreOn()
    {
        FirewallProfilesCheck check = new(new FakeCommandRunner(new CommandRunResult
        {
            FileName = "netsh",
            Arguments = "advfirewall show allprofiles state",
            ExitCode = 0,
            StandardOutput = """
            Domain Profile Settings:
            ----------------------------------------------------------------------
            State                                 ON

            Private Profile Settings:
            ----------------------------------------------------------------------
            State                                 ON

            Public Profile Settings:
            ----------------------------------------------------------------------
            State                                 ON
            """
        }));

        CheckResult result = await check.RunAsync(CancellationToken.None);

        Assert.Equal(CheckSeverity.Pass, result.Severity);
        Assert.Contains("Domain Profile: ON", result.CurrentValue);
        Assert.Contains("Public Profile: ON", result.CurrentValue);
    }

    [Fact]
    public async Task RunAsync_ReturnsWarningWhenNetshFails()
    {
        FirewallProfilesCheck check = new(new FakeCommandRunner(new CommandRunResult
        {
            FileName = "netsh",
            Arguments = "advfirewall show allprofiles state",
            ErrorMessage = "netsh unavailable"
        }));

        CheckResult result = await check.RunAsync(CancellationToken.None);

        Assert.Equal(CheckSeverity.Warning, result.Severity);
        Assert.Equal("netsh unavailable", result.CurrentValue);
    }

    private sealed class FakeCommandRunner(CommandRunResult result) : ICommandRunner
    {
        public Task<CommandRunResult> RunAsync(string fileName, string arguments, TimeSpan timeout, CancellationToken cancellationToken)
        {
            Assert.Equal("netsh", fileName);
            Assert.Equal("advfirewall show allprofiles state", arguments);
            return Task.FromResult(result);
        }
    }
}
