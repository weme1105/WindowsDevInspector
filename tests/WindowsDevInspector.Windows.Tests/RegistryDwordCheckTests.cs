using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows.Tests;

public sealed class RegistryDwordCheckTests
{
    [Fact]
    public async Task RunAsync_ReturnsPassWhenDwordMatches()
    {
        RegistryDwordCheck check = CreateCheck(new RegistryDwordReadResult
        {
            Exists = true,
            Value = 1
        });

        CheckResult result = await check.RunAsync(CancellationToken.None);

        Assert.Equal(CheckSeverity.Pass, result.Severity);
        Assert.False(result.CanFix);
    }

    [Fact]
    public async Task RunAsync_ReturnsWarningWhenDwordDoesNotMatch()
    {
        RegistryDwordCheck check = CreateCheck(new RegistryDwordReadResult
        {
            Exists = true,
            Value = 0
        });

        CheckResult result = await check.RunAsync(CancellationToken.None);

        Assert.Equal(CheckSeverity.Warning, result.Severity);
        Assert.True(result.RequiresElevation);
        Assert.Equal("enable-long-paths", result.RemediationId);
    }

    private static RegistryDwordCheck CreateCheck(RegistryDwordReadResult readResult)
    {
        return new RegistryDwordCheck(
            "common.long-paths",
            "Common",
            "Long Paths enabled",
            "HKLM",
            "SYSTEM\\CurrentControlSet\\Control\\FileSystem",
            "LongPathsEnabled",
            1,
            new FakeRegistryReader(readResult),
            "Long path support is disabled.",
            "enable-long-paths");
    }

    private sealed class FakeRegistryReader(RegistryDwordReadResult result) : IRegistryReader
    {
        public RegistryDwordReadResult ReadDword(string hive, string subKeyPath, string valueName)
        {
            return result;
        }
    }
}
