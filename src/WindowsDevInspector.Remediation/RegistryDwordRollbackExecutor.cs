namespace WindowsDevInspector.Remediation;

public sealed class RegistryDwordRollbackExecutor(IRemediationRegistry registry)
{
    private static readonly HashSet<string> AllowedTargets = new(StringComparer.OrdinalIgnoreCase)
    {
        Key("HKLM", "SYSTEM\\CurrentControlSet\\Control\\FileSystem", "LongPathsEnabled"),
        Key("HKLM", "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\AppModelUnlock", "AllowDevelopmentWithoutDevLicense")
    };

    public Task<RemediationExecutionResult> RollbackAsync(
        RegistryDwordBackup backup,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!AllowedTargets.Contains(Key(backup.Hive, backup.SubKeyPath, backup.ValueName)))
        {
            return Task.FromResult(RemediationExecutionResult.Skip(
                "rollback-registry-dword",
                "Backup target is not approved for rollback."));
        }

        if (backup.Existed)
        {
            if (backup.PreviousValue is null)
            {
                return Task.FromResult(RemediationExecutionResult.Skip(
                    "rollback-registry-dword",
                    "Backup says the value existed but does not contain a previous DWORD value."));
            }

            registry.WriteDword(backup.Hive, backup.SubKeyPath, backup.ValueName, backup.PreviousValue.Value);
        }
        else
        {
            registry.DeleteValue(backup.Hive, backup.SubKeyPath, backup.ValueName);
        }

        RegistryDwordValue verifiedValue = registry.ReadDword(backup.Hive, backup.SubKeyPath, backup.ValueName);
        bool verified = backup.Existed
            ? verifiedValue.Exists && verifiedValue.Value == backup.PreviousValue
            : !verifiedValue.Exists;

        return Task.FromResult(verified
            ? RemediationExecutionResult.Success("rollback-registry-dword", "Registry backup restored and verified.")
            : RemediationExecutionResult.Skip("rollback-registry-dword", "Rollback verification failed."));
    }

    private static string Key(string hive, string subKeyPath, string valueName)
    {
        return string.Join('|', hive, subKeyPath, valueName);
    }
}
