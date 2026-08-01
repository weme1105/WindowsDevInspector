using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows;

public sealed class LocalhostBindHealthCheck(ILocalhostBindProbe probe) : IEnvironmentCheck
{
    public string Id => "devops.localhost-bind";

    public Task<CheckResult> RunAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        LocalhostBindProbeResult result = probe.Probe();

        return Task.FromResult(new CheckResult
        {
            Id = Id,
            Category = "DevOps",
            Name = "localhost bind health",
            Severity = result.Succeeded ? CheckSeverity.Pass : CheckSeverity.Warning,
            CurrentValue = result.Succeeded
                ? $"Loopback bind succeeded on ephemeral port {result.Port}"
                : result.ErrorMessage ?? "Loopback bind failed",
            ExpectedValue = "An ephemeral TCP listener can bind to 127.0.0.1",
            Impact = result.Succeeded
                ? "Local development servers can bind to localhost."
                : "Local development servers, test runners, or container proxies may fail to bind to localhost.",
            CanFix = false,
            Risk = RiskLevel.None
        });
    }
}
