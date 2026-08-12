namespace WindowsDevInspector.Remediation;

public sealed record PackageInstallationProcessResult
{
    public required PackageInstallationOutcome Outcome { get; init; }

    public int? ExitCode { get; init; }

    public required string Message { get; init; }

    public static PackageInstallationProcessResult Disabled(string message)
    {
        return new PackageInstallationProcessResult
        {
            Outcome = PackageInstallationOutcome.Disabled,
            ExitCode = null,
            Message = message
        };
    }
}
