using WindowsDevInspector.Core;

namespace WindowsDevInspector.Remediation;

public sealed record ApprovedInstallationPackage
{
    public required string DiagnosticCheckId { get; init; }

    public required string AvailabilityCheckId { get; init; }

    public required string PackageId { get; init; }

    public required string DisplayName { get; init; }

    public required InstallationSource Source { get; init; }

    public required RiskLevel Risk { get; init; }

    public bool RequiresElevation { get; init; }

    public bool RequiresRestart { get; init; }

    public bool RefreshPathAfterInstall { get; init; }

    public required string VerificationExecutable { get; init; }

    public required IReadOnlyList<string> VerificationArguments { get; init; }
}
