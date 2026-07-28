using System.Text.Json;

namespace WindowsDevInspector.Remediation.Tests;

public sealed class RegistryDwordRemediationExecutorTests
{
    [Fact]
    public async Task ExecuteAsync_WritesExpectedDwordAndReturnsBackup()
    {
        FakeRemediationRegistry registry = new();
        RegistryDwordRemediationExecutor executor = new(BuiltInRemediationCatalog.CreateWhitelist(), registry);

        RemediationExecutionResult result = await executor.ExecuteAsync(
            "enable-long-paths",
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.False(result.Skipped);
        Assert.Equal(1, registry.GetWrittenValue("HKLM", "SYSTEM\\CurrentControlSet\\Control\\FileSystem", "LongPathsEnabled"));

        RegistryDwordBackup backup = JsonSerializer.Deserialize<RegistryDwordBackup>(result.BackupJson!)!;
        Assert.False(backup.Existed);
        Assert.Null(backup.PreviousValue);
    }

    [Fact]
    public async Task ExecuteAsync_DoesNotWriteWhenValueAlreadyMatches()
    {
        FakeRemediationRegistry registry = new();
        registry.Seed("HKLM", "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\AppModelUnlock", "AllowDevelopmentWithoutDevLicense", 1);
        RegistryDwordRemediationExecutor executor = new(BuiltInRemediationCatalog.CreateWhitelist(), registry);

        RemediationExecutionResult result = await executor.ExecuteAsync(
            "enable-developer-mode",
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Empty(registry.Writes);
    }

    [Fact]
    public async Task ExecuteAsync_SkipsUnsupportedWhitelistedRemediation()
    {
        FakeRemediationRegistry registry = new();
        RegistryDwordRemediationExecutor executor = new(BuiltInRemediationCatalog.CreateWhitelist(), registry);

        RemediationExecutionResult result = await executor.ExecuteAsync(
            "create-source-directory",
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.True(result.Skipped);
        Assert.Empty(registry.Writes);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsUnknownRemediation()
    {
        RegistryDwordRemediationExecutor executor = new(BuiltInRemediationCatalog.CreateWhitelist(), new FakeRemediationRegistry());

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => executor.ExecuteAsync("run-arbitrary-command", CancellationToken.None));

        Assert.Contains("not whitelisted", exception.Message);
    }

    private sealed class FakeRemediationRegistry : IRemediationRegistry
    {
        private readonly Dictionary<string, int> values = new(StringComparer.OrdinalIgnoreCase);

        public List<string> Writes { get; } = [];

        public RegistryDwordValue ReadDword(string hive, string subKeyPath, string valueName)
        {
            string key = Key(hive, subKeyPath, valueName);

            return values.TryGetValue(key, out int value)
                ? new RegistryDwordValue { Exists = true, Value = value }
                : new RegistryDwordValue { Exists = false };
        }

        public void WriteDword(string hive, string subKeyPath, string valueName, int value)
        {
            string key = Key(hive, subKeyPath, valueName);
            values[key] = value;
            Writes.Add(key);
        }

        public void DeleteValue(string hive, string subKeyPath, string valueName)
        {
            values.Remove(Key(hive, subKeyPath, valueName));
        }

        public void Seed(string hive, string subKeyPath, string valueName, int value)
        {
            values[Key(hive, subKeyPath, valueName)] = value;
        }

        public int? GetWrittenValue(string hive, string subKeyPath, string valueName)
        {
            return values.TryGetValue(Key(hive, subKeyPath, valueName), out int value)
                ? value
                : null;
        }

        private static string Key(string hive, string subKeyPath, string valueName)
        {
            return string.Join('|', hive, subKeyPath, valueName);
        }
    }
}
