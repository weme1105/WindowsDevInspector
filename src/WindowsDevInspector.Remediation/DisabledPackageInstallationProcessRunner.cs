namespace WindowsDevInspector.Remediation;

public sealed class DisabledPackageInstallationProcessRunner : IPackageInstallationProcessRunner
{
    public Task<PackageInstallationProcessResult> RunAsync(
        PackageInstallationCommandPreview command,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(PackageInstallationProcessResult.Disabled(
            "Package installation process execution is disabled."));
    }
}
