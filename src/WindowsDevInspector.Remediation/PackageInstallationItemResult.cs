namespace WindowsDevInspector.Remediation;

public sealed record PackageInstallationItemResult
{
    public required string PackageId { get; init; }

    public required PackageInstallationOutcome Outcome { get; init; }

    public required string Message { get; init; }

    public required PackageInstallationCommandPreview CommandPreview { get; init; }

    public PackageInstallationVerificationResult? Verification { get; init; }
}
