using WindowsDevInspector.Core;
using WindowsDevInspector.Windows;

namespace WindowsDevInspector.Windows.Tests;

public sealed class WindowsVersionCheckTests
{
    [Fact]
    public async Task RunAsync_ReturnsInformationalWindowsVersion()
    {
        WindowsVersionCheck check = new();

        CheckResult result = await check.RunAsync(CancellationToken.None);

        Assert.Equal("common.windows-version", result.Id);
        Assert.Equal(CheckSeverity.Info, result.Severity);
        Assert.StartsWith("Windows ", result.CurrentValue, StringComparison.Ordinal);
        Assert.False(result.CanFix);
        Assert.Equal(RiskLevel.None, result.Risk);
    }
}
