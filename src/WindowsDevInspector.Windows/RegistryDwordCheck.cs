using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows;

public sealed class RegistryDwordCheck(
    string id,
    string category,
    string name,
    string hive,
    string subKeyPath,
    string valueName,
    int expectedValue,
    IRegistryReader registryReader,
    string impact,
    string? remediationId = null) : IEnvironmentCheck
{
    public string Id => id;

    public Task<CheckResult> RunAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        RegistryDwordReadResult readResult = registryReader.ReadDword(hive, subKeyPath, valueName);
        bool passed = readResult.Exists && readResult.Value == expectedValue;
        bool canFix = !passed && remediationId is not null;

        CheckResult result = new()
        {
            Id = id,
            Category = category,
            Name = name,
            Severity = passed ? CheckSeverity.Pass : CheckSeverity.Warning,
            CurrentValue = FormatCurrentValue(readResult),
            ExpectedValue = expectedValue.ToString(),
            Impact = passed ? "The expected Windows setting is enabled." : impact,
            CanFix = canFix,
            Risk = canFix ? RiskLevel.Low : RiskLevel.None,
            RequiresElevation = canFix,
            RequiresRestart = false,
            SupportsRollback = canFix,
            RemediationId = passed ? null : remediationId
        };

        return Task.FromResult(result);
    }

    private static string FormatCurrentValue(RegistryDwordReadResult readResult)
    {
        if (readResult.ErrorMessage is not null)
        {
            return $"Unreadable: {readResult.ErrorMessage}";
        }

        if (!readResult.Exists)
        {
            return "Missing";
        }

        return readResult.Value?.ToString() ?? "Not a DWORD";
    }
}
