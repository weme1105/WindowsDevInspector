namespace WindowsDevInspector.Remediation.Tests;

public sealed class DisabledPackageInstallationProcessRunnerTests
{
    [Fact]
    public async Task RunAsync_AlwaysReturnsDisabledWithoutStartingProcess()
    {
        DisabledPackageInstallationProcessRunner runner = new();
        PackageInstallationCommandPreview command = new()
        {
            PackageId = "pnpm.pnpm",
            Source = InstallationSource.Winget,
            FileName = "winget",
            Arguments = ["install", "--id", "pnpm.pnpm", "--exact"]
        };

        PackageInstallationProcessResult result = await runner.RunAsync(
            command,
            TimeSpan.FromMinutes(1),
            CancellationToken.None);

        Assert.Equal(PackageInstallationOutcome.Disabled, result.Outcome);
        Assert.Null(result.ExitCode);
        Assert.Contains("disabled", result.Message, StringComparison.OrdinalIgnoreCase);
    }
}
