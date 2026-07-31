using WindowsDevInspector.App;

namespace WindowsDevInspector.App.Tests;

public sealed class ScanConcurrencySettingsTests
{
    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 1)]
    [InlineData(8, 7)]
    [InlineData(16, 15)]
    public void RecommendedForProcessorCount_LeavesOneProcessorAvailable(int processorCount, int expected)
    {
        int result = ScanConcurrencySettings.RecommendedForProcessorCount(processorCount);

        Assert.Equal(expected, result);
    }
}
