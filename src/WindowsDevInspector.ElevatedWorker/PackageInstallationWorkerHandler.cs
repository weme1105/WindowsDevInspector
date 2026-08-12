using WindowsDevInspector.Remediation;

namespace WindowsDevInspector.ElevatedWorker;

public sealed class PackageInstallationWorkerHandler(PackageInstallationExecutor executor)
{
    public Task<PackageInstallationExecutionResult> ExecuteAsync(
        InstallationPlan? plan,
        CancellationToken cancellationToken)
    {
        return executor.ExecuteAsync(plan, cancellationToken);
    }

    public static PackageInstallationWorkerHandler CreateDefault()
    {
        ApprovedInstallationCatalog catalog = BuiltInInstallationCatalog.Create();
        PackageInstallationExecutor executor = new(
            new InstallationPlanValidator(catalog),
            catalog,
            new WingetPackageInstallationProcessRunner(new PackageInstallationCommandRunner()),
            new PackageInstallationVerifier(
                new PackageInstallationCommandRunner(),
                new WindowsExecutablePathResolver()),
            new WindowsProcessPathRefresher(),
            new PackageInstallationExecutorOptions
            {
                ExecutionEnabled = true,
                Timeout = TimeSpan.FromMinutes(5)
            });

        return new PackageInstallationWorkerHandler(executor);
    }
}
