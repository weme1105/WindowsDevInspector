using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows.Tests;

public sealed class LocalhostBindHealthCheckTests
{
    [Fact]
    public async Task RunAsync_ReturnsPassWhenProbeSucceeds()
    {
        LocalhostBindHealthCheck check = new(new FakeLocalhostBindProbe(new LocalhostBindProbeResult
        {
            Succeeded = true,
            Port = 54321
        }));

        CheckResult result = await check.RunAsync(CancellationToken.None);

        Assert.Equal(CheckSeverity.Pass, result.Severity);
        Assert.Contains("54321", result.CurrentValue);
    }

    [Fact]
    public async Task RunAsync_ReturnsWarningWhenProbeFails()
    {
        LocalhostBindHealthCheck check = new(new FakeLocalhostBindProbe(new LocalhostBindProbeResult
        {
            Succeeded = false,
            ErrorMessage = "Loopback blocked"
        }));

        CheckResult result = await check.RunAsync(CancellationToken.None);

        Assert.Equal(CheckSeverity.Warning, result.Severity);
        Assert.Equal("Loopback blocked", result.CurrentValue);
    }

    private sealed class FakeLocalhostBindProbe(LocalhostBindProbeResult result) : ILocalhostBindProbe
    {
        public LocalhostBindProbeResult Probe()
        {
            return result;
        }
    }
}
