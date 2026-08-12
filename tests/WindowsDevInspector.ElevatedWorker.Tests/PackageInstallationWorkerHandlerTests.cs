using WindowsDevInspector.Remediation;

namespace WindowsDevInspector.ElevatedWorker.Tests;

public sealed class PackageInstallationWorkerHandlerTests
{
    [Fact]
    public async Task ExecuteAsync_ValidSinglePackage_UsesApprovedFixedCommand()
    {
        RecordingRunner runner = new(PackageInstallationOutcome.Succeeded);
        PackageInstallationWorkerHandler handler = CreateHandler(runner, executionEnabled: true);

        PackageInstallationExecutionResult result = await handler.ExecuteAsync(
            Plan("Microsoft.AzureCLI"),
            CancellationToken.None);

        Assert.True(result.Succeeded);
        PackageInstallationCommandPreview command = Assert.Single(runner.Commands);
        Assert.Equal(
            [
                "install", "--id", "Microsoft.AzureCLI", "--exact", "--source", "winget",
                "--accept-package-agreements", "--accept-source-agreements"
            ],
            command.Arguments);
        Assert.Equal(TimeSpan.FromMinutes(5), runner.Timeout);
    }

    [Fact]
    public async Task ExecuteAsync_MultiplePackages_FailsBeforeProcessInvocation()
    {
        RecordingRunner runner = new(PackageInstallationOutcome.Succeeded);
        PackageInstallationWorkerHandler handler = CreateHandler(runner, executionEnabled: true);
        InstallationPlan plan = Plan("pnpm.pnpm") with
        {
            Items =
            [
                Item("pnpm.pnpm"),
                Item("Kubernetes.kubectl")
            ]
        };

        PackageInstallationExecutionResult result = await handler.ExecuteAsync(plan, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.Contains("exactly one", StringComparison.OrdinalIgnoreCase));
        Assert.Empty(runner.Commands);
    }

    [Fact]
    public async Task ExecuteAsync_UnapprovedPackage_FailsBeforeProcessInvocation()
    {
        RecordingRunner runner = new(PackageInstallationOutcome.Succeeded);
        PackageInstallationWorkerHandler handler = CreateHandler(runner, executionEnabled: true);

        PackageInstallationExecutionResult result = await handler.ExecuteAsync(
            Plan("Arbitrary.Package"),
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.Contains("not approved", StringComparison.OrdinalIgnoreCase));
        Assert.Empty(runner.Commands);
    }

    private static PackageInstallationWorkerHandler CreateHandler(
        IPackageInstallationProcessRunner runner,
        bool executionEnabled)
    {
        ApprovedInstallationCatalog catalog = BuiltInInstallationCatalog.Create();
        PackageInstallationExecutor executor = new(
            new InstallationPlanValidator(catalog),
            catalog,
            runner,
            new SuccessfulVerifier(),
            new NoOpPathRefresher(),
            new PackageInstallationExecutorOptions
            {
                ExecutionEnabled = executionEnabled,
                Timeout = TimeSpan.FromMinutes(5)
            });
        return new PackageInstallationWorkerHandler(executor);
    }

    private sealed class SuccessfulVerifier : IPackageInstallationVerifier
    {
        public Task<PackageInstallationVerificationResult> VerifyAsync(
            ApprovedInstallationPackage package,
            TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(new PackageInstallationVerificationResult
            {
                Outcome = PackageInstallationVerificationOutcome.Succeeded,
                ExitCode = 0,
                Message = "Verified."
            });
        }
    }

    private sealed class NoOpPathRefresher : IProcessPathRefresher
    {
        public void Refresh()
        {
        }
    }

    private static InstallationPlan Plan(string packageId)
    {
        return new InstallationPlan
        {
            PlanId = Guid.NewGuid().ToString("D"),
            Items = [Item(packageId)]
        };
    }

    private static InstallationPlanItem Item(string packageId)
    {
        return new InstallationPlanItem
        {
            PackageId = packageId,
            Source = InstallationSource.Winget,
            Action = InstallationAction.Install
        };
    }

    private sealed class RecordingRunner(PackageInstallationOutcome outcome)
        : IPackageInstallationProcessRunner
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
}
