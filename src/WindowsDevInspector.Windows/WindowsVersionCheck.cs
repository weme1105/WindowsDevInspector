using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows;

public sealed class WindowsVersionCheck : IEnvironmentCheck
{
    public string Id => "common.windows-version";

    public Task<CheckResult> RunAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Version version = Environment.OSVersion.Version;
        string currentValue = $"Windows {version.Major}.{version.Minor}.{version.Build}";

        return Task.FromResult(new CheckResult
        {
            Id = Id,
            Category = "Common",
            Name = "Windows version and build",
            Severity = CheckSeverity.Info,
            CurrentValue = currentValue,
            ExpectedValue = "Windows 10/11 development workstation",
            Impact = "Windows version is collected as baseline context for interpreting developer tooling diagnostics.",
            CanFix = false,
            Risk = RiskLevel.None
        });
    }
}
