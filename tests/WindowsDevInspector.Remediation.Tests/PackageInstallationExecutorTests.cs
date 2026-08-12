namespace WindowsDevInspector.Remediation.Tests;

public sealed class PackageInstallationExecutorTests
{
    [Fact]
    public async Task ExecuteAsync_DefaultDisabledOptions_ReturnPreviewWithoutCallingRunner()
    {
        RecordingRunner runner = new(PackageInstallationOutcome.Succeeded);
        PackageInstallationExecutor executor = CreateExecutor(
            runner,
            new PackageInstallationExecutorOptions());

        PackageInstallationExecutionResult result = await executor.ExecuteAsync(
            Plan("pnpm.pnpm"),
            CancellationToken.None);

        Assert.False(result.Succeeded);
        PackageInstallationItemResult item = Assert.Single(result.Results);
        Assert.Equal(PackageInstallationOutcome.Disabled, item.Outcome);
        Assert.Equal(
            [
                "install", "--id", "pnpm.pnpm", "--exact", "--source", "winget",
                "--accept-package-agreements", "--accept-source-agreements"
            ],
            item.CommandPreview.Arguments);
        Assert.Equal(
            "winget install --id pnpm.pnpm --exact --source winget --accept-package-agreements --accept-source-agreements",
            item.CommandPreview.DisplayCommand);
        Assert.Empty(runner.Commands);
        Assert.Equal(PackageInstallationVerificationOutcome.NotRun, item.Verification?.Outcome);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidPlan_FailsBeforeCallingRunner()
    {
        RecordingRunner runner = new(PackageInstallationOutcome.Succeeded);
        PackageInstallationExecutor executor = CreateExecutor(
            runner,
            new PackageInstallationExecutorOptions { ExecutionEnabled = true });

        PackageInstallationExecutionResult result = await executor.ExecuteAsync(
            Plan("Arbitrary.Package"),
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Empty(result.Results);
        Assert.Contains(result.Errors, error => error.Contains("not approved", StringComparison.OrdinalIgnoreCase));
        Assert.Empty(runner.Commands);
    }

    [Fact]
    public async Task ExecuteAsync_EnabledOptions_StillHonorsDisabledRunner()
    {
        DisabledPackageInstallationProcessRunner runner = new();
        PackageInstallationExecutor executor = CreateExecutor(
            runner,
            new PackageInstallationExecutorOptions { ExecutionEnabled = true });

        PackageInstallationExecutionResult result = await executor.ExecuteAsync(
            Plan("Microsoft.AzureCLI"),
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(PackageInstallationOutcome.Disabled, Assert.Single(result.Results).Outcome);
    }

    [Fact]
    public async Task ExecuteAsync_PassesFixedPreviewAndTimeoutToInjectedRunner()
    {
        RecordingRunner runner = new(PackageInstallationOutcome.Succeeded);
        TimeSpan timeout = TimeSpan.FromSeconds(45);
        PackageInstallationExecutor executor = CreateExecutor(
            runner,
            new PackageInstallationExecutorOptions
            {
                ExecutionEnabled = true,
                Timeout = timeout
            });

        PackageInstallationExecutionResult result = await executor.ExecuteAsync(
            Plan("Kubernetes.kubectl"),
            CancellationToken.None);

        Assert.True(result.Succeeded);
        PackageInstallationCommandPreview command = Assert.Single(runner.Commands);
        Assert.Equal("winget", command.FileName);
        Assert.Equal(
            [
                "install", "--id", "Kubernetes.kubectl", "--exact", "--source", "winget",
                "--accept-package-agreements", "--accept-source-agreements"
            ],
            command.Arguments);
        Assert.Equal(timeout, runner.Timeout);
    }

    [Fact]
    public async Task ExecuteAsync_SuccessfulInstall_RefreshesPathAndRunsCatalogVerification()
    {
        RecordingRunner runner = new(PackageInstallationOutcome.Succeeded);
        RecordingVerifier verifier = new(PackageInstallationVerificationOutcome.Succeeded);
        RecordingPathRefresher pathRefresher = new();
        PackageInstallationExecutor executor = CreateExecutor(
            runner,
            new PackageInstallationExecutorOptions
            {
                ExecutionEnabled = true,
                VerificationTimeout = TimeSpan.FromSeconds(12)
            },
            verifier,
            pathRefresher);

        PackageInstallationExecutionResult result = await executor.ExecuteAsync(
            Plan("Kubernetes.kubectl"),
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(1, pathRefresher.RefreshCount);
        Assert.Equal("kubectl", verifier.Package?.VerificationExecutable);
        Assert.Equal(["version", "--client"], verifier.Package?.VerificationArguments);
        Assert.Equal(TimeSpan.FromSeconds(12), verifier.Timeout);
        Assert.Equal(
            PackageInstallationVerificationOutcome.Succeeded,
            Assert.Single(result.Results).Verification?.Outcome);
    }

    [Fact]
    public async Task ExecuteAsync_FailedVerification_MarksOverallResultFailed()
    {
        RecordingVerifier verifier = new(PackageInstallationVerificationOutcome.Failed);
        PackageInstallationExecutor executor = CreateExecutor(
            new RecordingRunner(PackageInstallationOutcome.Succeeded),
            new PackageInstallationExecutorOptions { ExecutionEnabled = true },
            verifier,
            new RecordingPathRefresher());

        PackageInstallationExecutionResult result = await executor.ExecuteAsync(
            Plan("pnpm.pnpm"),
            CancellationToken.None);

        Assert.False(result.Succeeded);
        PackageInstallationItemResult item = Assert.Single(result.Results);
        Assert.Equal(PackageInstallationOutcome.Failed, item.Outcome);
        Assert.Equal(PackageInstallationVerificationOutcome.Failed, item.Verification?.Outcome);
        Assert.Contains("Post-install verification failed", item.Message);
    }

    [Fact]
    public async Task ExecuteAsync_FailedInstall_DoesNotRefreshPathOrVerify()
    {
        RecordingVerifier verifier = new(PackageInstallationVerificationOutcome.Succeeded);
        RecordingPathRefresher pathRefresher = new();
        PackageInstallationExecutor executor = CreateExecutor(
            new RecordingRunner(PackageInstallationOutcome.Failed),
            new PackageInstallationExecutorOptions { ExecutionEnabled = true },
            verifier,
            pathRefresher);

        PackageInstallationExecutionResult result = await executor.ExecuteAsync(
            Plan("pnpm.pnpm"),
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(0, pathRefresher.RefreshCount);
        Assert.Null(verifier.Package);
        Assert.Equal(
            PackageInstallationVerificationOutcome.NotRun,
            Assert.Single(result.Results).Verification?.Outcome);
    }

    [Fact]
    public async Task ExecuteAsync_ThrowsWhenCancelledBeforeValidation()
    {
        PackageInstallationExecutor executor = CreateExecutor(
            new DisabledPackageInstallationProcessRunner(),
            new PackageInstallationExecutorOptions());
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => executor.ExecuteAsync(Plan("pnpm.pnpm"), cancellation.Token));
    }

    private static PackageInstallationExecutor CreateExecutor(
        IPackageInstallationProcessRunner runner,
        PackageInstallationExecutorOptions options,
        IPackageInstallationVerifier? verifier = null,
        IProcessPathRefresher? pathRefresher = null)
    {
        ApprovedInstallationCatalog catalog = BuiltInInstallationCatalog.Create();
        return new PackageInstallationExecutor(
            new InstallationPlanValidator(catalog),
            catalog,
            runner,
            verifier ?? new RecordingVerifier(PackageInstallationVerificationOutcome.Succeeded),
            pathRefresher ?? new RecordingPathRefresher(),
            options);
    }

    private static InstallationPlan Plan(string packageId)
    {
        return new InstallationPlan
        {
            PlanId = Guid.NewGuid().ToString("D"),
            Items =
            [
                new InstallationPlanItem
                {
                    PackageId = packageId,
                    Source = InstallationSource.Winget,
                    Action = InstallationAction.Install
                }
            ]
        };
    }

    private sealed class RecordingRunner(PackageInstallationOutcome outcome) : IPackageInstallationProcessRunner
    {
        public List<PackageInstallationCommandPreview> Commands { get; } = [];

        public TimeSpan? Timeout { get; private set; }

        public Task<PackageInstallationProcessResult> RunAsync(
            PackageInstallationCommandPreview command,
            TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Commands.Add(command);
            Timeout = timeout;
            return Task.FromResult(new PackageInstallationProcessResult
            {
                Outcome = outcome,
                ExitCode = outcome == PackageInstallationOutcome.Succeeded ? 0 : 1,
                Message = outcome.ToString()
            });
        }
    }

    private sealed class RecordingVerifier(PackageInstallationVerificationOutcome outcome)
        : IPackageInstallationVerifier
    {
        public ApprovedInstallationPackage? Package { get; private set; }

        public TimeSpan? Timeout { get; private set; }

        public Task<PackageInstallationVerificationResult> VerifyAsync(
            ApprovedInstallationPackage package,
            TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Package = package;
            Timeout = timeout;
            return Task.FromResult(new PackageInstallationVerificationResult
            {
                Outcome = outcome,
                ExitCode = outcome == PackageInstallationVerificationOutcome.Succeeded ? 0 : 1,
                Message = outcome.ToString()
            });
        }
    }

    private sealed class RecordingPathRefresher : IProcessPathRefresher
    {
        public int RefreshCount { get; private set; }

        public void Refresh()
        {
            RefreshCount++;
        }
    }
}
