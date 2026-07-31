using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows.Tests;

public sealed class WingetPackageAvailabilityCheckTests
{
    [Fact]
    public async Task RunAsync_ReturnsPassWhenWingetFindsPackage()
    {
        FakeCommandRunner commandRunner = new(new CommandRunResult
        {
            FileName = "winget",
            Arguments = "show --id pnpm.pnpm --exact --accept-source-agreements",
            ExitCode = 0,
            StandardOutput = """
                Found pnpm [pnpm.pnpm]
                Version: 11.0.0
                Publisher: pnpm
                """
        });
        WingetPackageAvailabilityCheck check = new(
            "install.pnpm-winget",
            "Install Planning",
            "pnpm winget package",
            "pnpm.pnpm",
            commandRunner);

        CheckResult result = await check.RunAsync(CancellationToken.None);

        Assert.Equal(CheckSeverity.Pass, result.Severity);
        Assert.Equal("Package available", result.CurrentValue);
        Assert.Contains("does not require installing or upgrading", result.Impact);
        Assert.False(result.CanFix);
        Assert.Equal("winget package id: pnpm.pnpm", result.ExpectedValue);
        Assert.Equal("winget", commandRunner.FileName);
        Assert.Equal("show --id pnpm.pnpm --exact --accept-source-agreements", commandRunner.Arguments);
    }

    [Fact]
    public async Task RunAsync_ReturnsInfoWhenPackageCannotBeConfirmed()
    {
        WingetPackageAvailabilityCheck check = new(
            "install.terraform-winget",
            "Install Planning",
            "Terraform winget package",
            "Hashicorp.Terraform",
            new FakeCommandRunner(new CommandRunResult
            {
                FileName = "winget",
                Arguments = "show --id Hashicorp.Terraform --exact --accept-source-agreements",
                ExitCode = 1,
                StandardError = "No package found matching input criteria."
            }));

        CheckResult result = await check.RunAsync(CancellationToken.None);

        Assert.Equal(CheckSeverity.Info, result.Severity);
        Assert.Contains("No package found", result.CurrentValue);
        Assert.Contains("Installation planning should stay manual", result.Impact);
        Assert.False(result.CanFix);
    }

    [Fact]
    public async Task RunAsync_ReturnsInfoWhenWingetIsMissing()
    {
        WingetPackageAvailabilityCheck check = new(
            "install.pnpm-winget",
            "Install Planning",
            "pnpm winget package",
            "pnpm.pnpm",
            new FakeCommandRunner(new CommandRunResult
            {
                FileName = "winget",
                Arguments = "show --id pnpm.pnpm --exact --accept-source-agreements",
                ErrorMessage = "The system cannot find the file specified."
            }));

        CheckResult result = await check.RunAsync(CancellationToken.None);

        Assert.Equal(CheckSeverity.Info, result.Severity);
        Assert.Contains("cannot find", result.CurrentValue);
        Assert.False(result.CanFix);
    }

    [Fact]
    public async Task RunAsync_ReturnsInfoWhenWingetTimesOut()
    {
        WingetPackageAvailabilityCheck check = new(
            "install.azure-cli-winget",
            "Install Planning",
            "Azure CLI winget package",
            "Microsoft.AzureCLI",
            new FakeCommandRunner(new CommandRunResult
            {
                FileName = "winget",
                Arguments = "show --id Microsoft.AzureCLI --exact --accept-source-agreements",
                TimedOut = true
            }));

        CheckResult result = await check.RunAsync(CancellationToken.None);

        Assert.Equal(CheckSeverity.Info, result.Severity);
        Assert.Equal("Timed out", result.CurrentValue);
    }

    private sealed class FakeCommandRunner(CommandRunResult result) : ICommandRunner
    {
        public string? FileName { get; private set; }

        public string? Arguments { get; private set; }

        public Task<CommandRunResult> RunAsync(string fileName, string arguments, TimeSpan timeout, CancellationToken cancellationToken)
        {
            FileName = fileName;
            Arguments = arguments;

            return Task.FromResult(result);
        }
    }
}
