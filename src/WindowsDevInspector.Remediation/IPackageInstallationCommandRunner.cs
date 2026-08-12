namespace WindowsDevInspector.Remediation;

public interface IPackageInstallationCommandRunner
{
    Task<PackageInstallationCommandRunResult> RunAsync(
        string fileName,
        IReadOnlyList<string> arguments,
        TimeSpan timeout,
        CancellationToken cancellationToken);
}
