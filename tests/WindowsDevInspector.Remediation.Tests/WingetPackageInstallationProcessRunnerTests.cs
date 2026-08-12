namespace WindowsDevInspector.Remediation.Tests;

public sealed class WingetPackageInstallationProcessRunnerTests
{
    [Fact]
    public async Task RunAsync_PassesApprovedFixedTokensToCommandRunner()
    {
        FakeCommandRunner commandRunner = new(new PackageInstallationCommandRunResult
        {
            ExitCode = 0,
            StandardOutput = "Successfully installed",
            StandardError = string.Empty
        });
        WingetPackageInstallationProcessRunner runner = new(commandRunner);
        PackageInstallationCommandPreview command = ApprovedCommand("pnpm.pnpm");

        PackageInstallationProcessResult result = await runner.RunAsync(
            command,
            TimeSpan.FromMinutes(5),
            CancellationToken.None);

        Assert.Equal(PackageInstallationOutcome.Succeeded, result.Outcome);
        Assert.Equal("winget", commandRunner.FileName);
        Assert.Equal(command.Arguments, commandRunner.Arguments);
        Assert.Equal(TimeSpan.FromMinutes(5), commandRunner.Timeout);
    }

    [Fact]
    public async Task RunAsync_RejectsAdditionalOrChangedArguments()
    {
        FakeCommandRunner commandRunner = new(new PackageInstallationCommandRunResult
        {
            ExitCode = 0,
            StandardOutput = string.Empty,
            StandardError = string.Empty
        });
        WingetPackageInstallationProcessRunner runner = new(commandRunner);
        PackageInstallationCommandPreview command = ApprovedCommand("pnpm.pnpm") with
        {
            Arguments =
            [
                "install", "--id", "pnpm.pnpm", "--exact", "--source", "winget",
                "--accept-package-agreements", "--accept-source-agreements", "--override", "arbitrary"
            ]
        };

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => runner.RunAsync(command, TimeSpan.FromMinutes(5), CancellationToken.None));

        Assert.Contains("fixed-token", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Null(commandRunner.FileName);
    }

    [Theory]
    [InlineData(true, null, PackageInstallationOutcome.TimedOut)]
    [InlineData(false, 1, PackageInstallationOutcome.Failed)]
    public async Task RunAsync_MapsTimeoutAndFailure(
        bool timedOut,
        int? exitCode,
        PackageInstallationOutcome expectedOutcome)
    {
        FakeCommandRunner commandRunner = new(new PackageInstallationCommandRunResult
        {
            ExitCode = exitCode,
            TimedOut = timedOut,
            StandardOutput = string.Empty,
            StandardError = "failure"
        });
        WingetPackageInstallationProcessRunner runner = new(commandRunner);

        PackageInstallationProcessResult result = await runner.RunAsync(
            ApprovedCommand("Hashicorp.Terraform"),
            TimeSpan.FromMinutes(5),
            CancellationToken.None);

        Assert.Equal(expectedOutcome, result.Outcome);
    }

    private static PackageInstallationCommandPreview ApprovedCommand(string packageId)
    {
        PackageInstallationCommandPreviewBuilder builder = new(BuiltInInstallationCatalog.Create());
        return builder.Build(new InstallationPlanItem
        {
            PackageId = packageId,
            Source = InstallationSource.Winget,
            Action = InstallationAction.Install
        });
    }

    private sealed class FakeCommandRunner(PackageInstallationCommandRunResult result)
        : IPackageInstallationCommandRunner
    {
        public string? FileName { get; private set; }

        public IReadOnlyList<string>? Arguments { get; private set; }

        public TimeSpan? Timeout { get; private set; }

        public Task<PackageInstallationCommandRunResult> RunAsync(
            string fileName,
            IReadOnlyList<string> arguments,
            TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            FileName = fileName;
            Arguments = arguments;
            Timeout = timeout;
            return Task.FromResult(result);
        }
    }
}
