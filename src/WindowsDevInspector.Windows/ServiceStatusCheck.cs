using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows;

public sealed class ServiceStatusCheck(
    string id,
    string category,
    string name,
    string serviceName,
    IServiceReader serviceReader,
    string? expectedStatus = null,
    CheckSeverity severityWhenUnexpected = CheckSeverity.Info,
    string? expectedValue = null,
    string? missingImpact = null,
    string? availableImpact = null) : IEnvironmentCheck
{
    public string Id => id;

    public Task<CheckResult> RunAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ServiceReadResult result = serviceReader.ReadService(serviceName);
        bool expectedStatusMatched = expectedStatus is null ||
            string.Equals(result.Status, expectedStatus, StringComparison.OrdinalIgnoreCase);

        return Task.FromResult(new CheckResult
        {
            Id = id,
            Category = category,
            Name = name,
            Severity = result.Exists && expectedStatusMatched ? CheckSeverity.Pass : severityWhenUnexpected,
            CurrentValue = result.Exists ? result.Status ?? "Unknown" : result.ErrorMessage ?? "Not found",
            ExpectedValue = expectedValue ?? (expectedStatus is null ? serviceName : $"{serviceName} service status: {expectedStatus}"),
            Impact = result.Exists && expectedStatusMatched
                ? availableImpact ?? "The Windows service exists and can be inspected."
                : missingImpact ?? "The Windows service was not found or is not in the expected state. Related local development networking may be unavailable.",
            CanFix = false,
            Risk = RiskLevel.None
        });
    }
}
