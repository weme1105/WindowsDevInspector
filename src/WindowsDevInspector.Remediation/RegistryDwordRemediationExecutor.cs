using System.Text.Json;

namespace WindowsDevInspector.Remediation;

public sealed class RegistryDwordRemediationExecutor(
    RemediationWhitelist whitelist,
    IRemediationRegistry registry)
{
    private static readonly IReadOnlyDictionary<string, RegistryDwordRemediationDefinition> Definitions =
        new Dictionary<string, RegistryDwordRemediationDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            ["enable-long-paths"] = new RegistryDwordRemediationDefinition
            {
                RemediationId = "enable-long-paths",
                Hive = "HKLM",
                SubKeyPath = "SYSTEM\\CurrentControlSet\\Control\\FileSystem",
                ValueName = "LongPathsEnabled",
                DesiredValue = 1
            },
            ["enable-developer-mode"] = new RegistryDwordRemediationDefinition
            {
                RemediationId = "enable-developer-mode",
                Hive = "HKLM",
                SubKeyPath = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\AppModelUnlock",
                ValueName = "AllowDevelopmentWithoutDevLicense",
                DesiredValue = 1
            }
        };

    public Task<RemediationExecutionResult> ExecuteAsync(
        string remediationId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        whitelist.GetRequired(remediationId);

        if (!Definitions.TryGetValue(remediationId, out RegistryDwordRemediationDefinition? definition))
        {
            return Task.FromResult(RemediationExecutionResult.Skip(
                remediationId,
                "This remediation is whitelisted but is not supported by the registry executor."));
        }

        RegistryDwordValue currentValue = registry.ReadDword(
            definition.Hive,
            definition.SubKeyPath,
            definition.ValueName);

        if (currentValue.ErrorMessage is not null)
        {
            return Task.FromResult(RemediationExecutionResult.Skip(
                remediationId,
                $"Could not read registry value before remediation: {currentValue.ErrorMessage}"));
        }

        RegistryDwordBackup backup = new()
        {
            Hive = definition.Hive,
            SubKeyPath = definition.SubKeyPath,
            ValueName = definition.ValueName,
            Existed = currentValue.Exists,
            PreviousValue = currentValue.Value
        };

        string backupJson = JsonSerializer.Serialize(backup);

        if (currentValue.Exists && currentValue.Value == definition.DesiredValue)
        {
            return Task.FromResult(RemediationExecutionResult.Success(
                remediationId,
                "Registry value already has the expected DWORD value.",
                backupJson));
        }

        registry.WriteDword(
            definition.Hive,
            definition.SubKeyPath,
            definition.ValueName,
            definition.DesiredValue);

        RegistryDwordValue verifiedValue = registry.ReadDword(
            definition.Hive,
            definition.SubKeyPath,
            definition.ValueName);

        if (!verifiedValue.Exists || verifiedValue.Value != definition.DesiredValue)
        {
            return Task.FromResult(RemediationExecutionResult.Skip(
                remediationId,
                "Registry value was written but verification did not read the expected DWORD value."));
        }

        return Task.FromResult(RemediationExecutionResult.Success(
            remediationId,
            "Registry DWORD value updated and verified.",
            backupJson));
    }
}
