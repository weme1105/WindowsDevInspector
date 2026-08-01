using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows.Tests;

public sealed class BrowserAvailabilityCheckTests
{
    [Fact]
    public async Task RunAsync_ReturnsPassWhenCommonBrowserExists()
    {
        BrowserAvailabilityCheck check = new(new FakeFileSystem([
            "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe"
        ]));

        CheckResult result = await check.RunAsync(CancellationToken.None);

        Assert.Equal(CheckSeverity.Pass, result.Severity);
        Assert.Contains("Chrome", result.CurrentValue);
    }

    [Fact]
    public async Task RunAsync_ReturnsInfoWhenNoBrowserExists()
    {
        BrowserAvailabilityCheck check = new(new FakeFileSystem([]));

        CheckResult result = await check.RunAsync(CancellationToken.None);

        Assert.Equal(CheckSeverity.Info, result.Severity);
        Assert.Contains("No common browser", result.CurrentValue);
    }

    private sealed class FakeFileSystem(IEnumerable<string> files) : IFileSystem
    {
        private readonly HashSet<string> files = new(files, StringComparer.OrdinalIgnoreCase);

        public bool DirectoryExists(string path) => false;

        public bool FileExists(string path) => files.Contains(path);
    }
}
