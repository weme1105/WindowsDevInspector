using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows.Tests;

public sealed class SmartAppControlCheckTests
{
    [Theory]
    [InlineData(0, "Off")]
    [InlineData(1, "On")]
    [InlineData(2, "Evaluation")]
    public async Task RunAsync_MapsKnownStateValues(int value, string expectedText)
    {
        SmartAppControlCheck check = new(new FakeRegistryReader(new RegistryDwordReadResult
        {
            Exists = true,
            Value = value
        }));

        CheckResult result = await check.RunAsync(CancellationToken.None);

        Assert.Equal(CheckSeverity.Info, result.Severity);
        Assert.Contains(expectedText, result.CurrentValue);
        Assert.False(result.CanFix);
    }

    [Fact]
    public async Task RunAsync_ReturnsWarningWhenRegistryCannotBeRead()
    {
        SmartAppControlCheck check = new(new FakeRegistryReader(new RegistryDwordReadResult
        {
            ErrorMessage = "Access denied"
        }));

        CheckResult result = await check.RunAsync(CancellationToken.None);

        Assert.Equal(CheckSeverity.Warning, result.Severity);
        Assert.Contains("Access denied", result.CurrentValue);
    }

    private sealed class FakeRegistryReader(RegistryDwordReadResult result) : IRegistryReader
    {
        public RegistryDwordReadResult ReadDword(string hive, string subKeyPath, string valueName)
        {
            Assert.Equal("HKLM", hive);
            Assert.Equal("SYSTEM\\CurrentControlSet\\Control\\CI\\Policy", subKeyPath);
            Assert.Equal("VerifiedAndReputablePolicyState", valueName);
            return result;
        }
    }
}
