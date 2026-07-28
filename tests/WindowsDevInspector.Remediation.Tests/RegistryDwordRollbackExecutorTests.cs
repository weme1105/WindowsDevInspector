namespace WindowsDevInspector.Remediation.Tests;

public sealed class RegistryDwordRollbackExecutorTests
{
    [Fact]
    public async Task RollbackAsync_RestoresPreviousValue()
    {
        FakeRegistry registry = new();
        RegistryDwordRollbackExecutor executor = new(registry);

        RemediationExecutionResult result = await executor.RollbackAsync(new RegistryDwordBackup
        {
            Hive = "HKLM",
            SubKeyPath = "SYSTEM\\CurrentControlSet\\Control\\FileSystem",
            ValueName = "LongPathsEnabled",
            Existed = true,
            PreviousValue = 0
        }, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(0, registry.ReadDword("HKLM", "SYSTEM\\CurrentControlSet\\Control\\FileSystem", "LongPathsEnabled").Value);
    }

    [Fact]
    public async Task RollbackAsync_DeletesValueWhenBackupDidNotExist()
    {
        FakeRegistry registry = new();
        registry.WriteDword("HKLM", "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\AppModelUnlock", "AllowDevelopmentWithoutDevLicense", 1);
        RegistryDwordRollbackExecutor executor = new(registry);

        RemediationExecutionResult result = await executor.RollbackAsync(new RegistryDwordBackup
        {
            Hive = "HKLM",
            SubKeyPath = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\AppModelUnlock",
            ValueName = "AllowDevelopmentWithoutDevLicense",
            Existed = false
        }, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.False(registry.ReadDword("HKLM", "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\AppModelUnlock", "AllowDevelopmentWithoutDevLicense").Exists);
    }

    [Fact]
    public async Task RollbackAsync_RejectsUnapprovedTarget()
    {
        RegistryDwordRollbackExecutor executor = new(new FakeRegistry());

        RemediationExecutionResult result = await executor.RollbackAsync(new RegistryDwordBackup
        {
            Hive = "HKLM",
            SubKeyPath = "SOFTWARE\\Bad",
            ValueName = "Value",
            Existed = true,
            PreviousValue = 1
        }, CancellationToken.None);

        Assert.True(result.Skipped);
    }

    private sealed class FakeRegistry : IRemediationRegistry
    {
        private readonly Dictionary<string, int> values = new(StringComparer.OrdinalIgnoreCase);

        public RegistryDwordValue ReadDword(string hive, string subKeyPath, string valueName)
        {
            return values.TryGetValue(Key(hive, subKeyPath, valueName), out int value)
                ? new RegistryDwordValue { Exists = true, Value = value }
                : new RegistryDwordValue { Exists = false };
        }

        public void WriteDword(string hive, string subKeyPath, string valueName, int value)
        {
            values[Key(hive, subKeyPath, valueName)] = value;
        }

        public void DeleteValue(string hive, string subKeyPath, string valueName)
        {
            values.Remove(Key(hive, subKeyPath, valueName));
        }

        private static string Key(string hive, string subKeyPath, string valueName)
        {
            return string.Join('|', hive, subKeyPath, valueName);
        }
    }
}
