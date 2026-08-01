using WindowsDevInspector.Core;
using WindowsDevInspector.Windows;

namespace WindowsDevInspector.Windows.Tests;

public sealed class ProcessorArchitectureCheckTests
{
    [Fact]
    public async Task RunAsync_ReturnsInformationalProcessorArchitecture()
    {
        ProcessorArchitectureCheck check = new();

        CheckResult result = await check.RunAsync(CancellationToken.None);

        Assert.Equal("common.processor-architecture", result.Id);
        Assert.Equal(CheckSeverity.Info, result.Severity);
        Assert.False(string.IsNullOrWhiteSpace(result.CurrentValue));
        Assert.False(result.CanFix);
        Assert.Equal(RiskLevel.None, result.Risk);
    }
}
