namespace WindowsDevInspector.Remediation.Tests;

public sealed class PackageInstallationCommandRunnerTests
{
    [Fact]
    public async Task RunAsync_CapturesOutputFromHarmlessCommand()
    {
        PackageInstallationCommandRunner runner = new();

        PackageInstallationCommandRunResult result = await runner.RunAsync(
            "dotnet",
            ["--version"],
            TimeSpan.FromSeconds(15),
            CancellationToken.None);

        Assert.Equal(0, result.ExitCode);
        Assert.False(result.TimedOut);
        Assert.False(string.IsNullOrWhiteSpace(result.StandardOutput));
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public async Task RunAsync_ReturnsFailureWhenExecutableIsMissing()
    {
        PackageInstallationCommandRunner runner = new();

        PackageInstallationCommandRunResult result = await runner.RunAsync(
            $"missing-{Guid.NewGuid():N}.exe",
            [],
            TimeSpan.FromSeconds(5),
            CancellationToken.None);

        Assert.Null(result.ExitCode);
        Assert.False(result.TimedOut);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
    }

    [Fact]
    public async Task RunAsync_StopsHarmlessProcessAfterTimeout()
    {
        PackageInstallationCommandRunner runner = new();

        PackageInstallationCommandRunResult result = await runner.RunAsync(
            "powershell.exe",
            ["-NoProfile", "-NonInteractive", "-Command", "Start-Sleep -Seconds 30"],
            TimeSpan.FromMilliseconds(250),
            CancellationToken.None);

        Assert.True(result.TimedOut);
        Assert.Null(result.ExitCode);
        Assert.Contains("timed out", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }
}
