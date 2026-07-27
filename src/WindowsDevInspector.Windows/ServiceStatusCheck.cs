using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows;

public sealed class ServiceStatusCheck(
    string id,
    string category,
    string name,
    string serviceName,
    IServiceReader serviceReader) : IEnvironmentCheck
{
    public string Id => id;

    public Task<CheckResult> RunAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ServiceReadResult result = serviceReader.ReadService(serviceName);

        return Task.FromResult(new CheckResult
        {
            Id = id,
            Category = category,
            Name = name,
            Severity = result.Exists ? CheckSeverity.Pass : CheckSeverity.Info,
            CurrentValue = result.Exists ? result.Status ?? "Unknown" : result.ErrorMessage ?? "Not found",
            ExpectedValue = serviceName,
            Impact = result.Exists
                ? "The Windows service exists and can be inspected."
                : "The Windows service was not found. Related local development networking may be unavailable.",
            CanFix = false,
            Risk = RiskLevel.None
        });
    }
}
