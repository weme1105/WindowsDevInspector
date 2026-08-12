namespace WindowsDevInspector.Remediation;

public interface IPackageInstallationVerifier
{
    Task<PackageInstallationVerificationResult> VerifyAsync(
        ApprovedInstallationPackage package,
        TimeSpan timeout,
        CancellationToken cancellationToken);
}
