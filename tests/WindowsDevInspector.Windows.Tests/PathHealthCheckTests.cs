using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows.Tests;

public sealed class PathHealthCheckTests
{
    [Fact]
    public async Task InvalidEntriesCheck_ReturnsWarningForMissingPath()
    {
        PathInvalidEntriesCheck check = new(new FakeEnvironmentVariableReader("Z:\\DefinitelyMissingForWindowsDevInspector"));

        CheckResult result = await check.RunAsync(CancellationToken.None);

        Assert.Equal(CheckSeverity.Warning, result.Severity);
        Assert.Contains("DefinitelyMissing", result.CurrentValue);
    }

    [Fact]
    public async Task DuplicateEntriesCheck_ReturnsInfoForDuplicatePath()
    {
        string tempPath = Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar);
        PathDuplicateEntriesCheck check = new(new FakeEnvironmentVariableReader($"{tempPath};{tempPath}\\"));

        CheckResult result = await check.RunAsync(CancellationToken.None);

        Assert.Equal(CheckSeverity.Info, result.Severity);
        Assert.Contains(tempPath, result.CurrentValue);
    }

    private sealed class FakeEnvironmentVariableReader(string pathValue) : IEnvironmentVariableReader
    {
        public string? GetEnvironmentVariable(string variable, EnvironmentVariableTarget target)
        {
            return target == EnvironmentVariableTarget.Process ? pathValue : null;
        }
    }
}
