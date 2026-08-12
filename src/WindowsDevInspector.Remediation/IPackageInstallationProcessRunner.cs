namespace WindowsDevInspector.Remediation;

public interface IPackageInstallationProcessRunner
{
    Task<PackageInstallationProcessResult> RunAsync(
        PackageInstallationCommandPreview command,
        TimeSpan timeout,
        CancellationToken cancellationToken);
}
