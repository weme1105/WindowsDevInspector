namespace WindowsDevInspector.Remediation;

public sealed record PackageInstallationVerificationResult
{
    public required PackageInstallationVerificationOutcome Outcome { get; init; }

    public int? ExitCode { get; init; }

    public required string Message { get; init; }

    public static PackageInstallationVerificationResult NotRun(string message) => new()
    {
        Outcome = PackageInstallationVerificationOutcome.NotRun,
        Message = message
    };
}
