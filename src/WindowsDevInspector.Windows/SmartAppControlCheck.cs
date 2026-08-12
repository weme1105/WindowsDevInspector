using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows;

public sealed class SmartAppControlCheck(IRegistryReader registryReader) : IEnvironmentCheck
{
    private const string Hive = "HKLM";
    private const string SubKeyPath = "SYSTEM\\CurrentControlSet\\Control\\CI\\Policy";
    private const string ValueName = "VerifiedAndReputablePolicyState";

    public string Id => "security.smart-app-control";

    public Task<CheckResult> RunAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        RegistryDwordReadResult result = registryReader.ReadDword(Hive, SubKeyPath, ValueName);

        bool readable = result.ErrorMessage is null;
        return Task.FromResult(new CheckResult
        {
            Id = Id,
            Category = "Security",
            Name = "Smart App Control diagnostics",
            Severity = readable ? CheckSeverity.Info : CheckSeverity.Warning,
            CurrentValue = FormatCurrentValue(result),
            ExpectedValue = $"{Hive}\\{SubKeyPath}\\{ValueName} is readable when Smart App Control state is exposed by Windows",
            Impact = readable
                ? "Smart App Control state was inspected without changing security settings."
                : "Smart App Control state could not be inspected. Windows Security may need to be checked manually if app launch is blocked.",
            CanFix = false,
            Risk = RiskLevel.None
        });
    }

    private static string FormatCurrentValue(RegistryDwordReadResult result)
    {
        if (result.ErrorMessage is not null)
        {
            return $"Unreadable: {result.ErrorMessage}";
        }

        if (!result.Exists)
        {
            return "Registry value not present on this Windows installation";
        }

        return result.Value switch
        {
            0 => "0 (Off)",
            1 => "1 (On)",
            2 => "2 (Evaluation)",
            null => "Present but not a DWORD",
            _ => $"{result.Value} (unrecognized)"
        };
    }
}
