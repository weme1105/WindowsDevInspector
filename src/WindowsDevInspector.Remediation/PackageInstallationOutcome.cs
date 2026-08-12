namespace WindowsDevInspector.Remediation;

public enum PackageInstallationOutcome
{
    Disabled = 0,
    Succeeded = 1,
    Failed = 2,
    TimedOut = 3,
    Cancelled = 4
}
