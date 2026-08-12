using WindowsDevInspector.Core;
using WindowsDevInspector.Remediation;

namespace WindowsDevInspector.App;

public sealed record PackageInstallationConfirmationItem
{
    public required string PackageId { get; init; }

    public required string DisplayName { get; init; }

    public required InstallationSource Source { get; init; }

    public required InstallationAction Action { get; init; }

    public required RiskLevel Risk { get; init; }

    public bool RequiresElevation { get; init; }

    public bool RequiresRestart { get; init; }

    public bool RefreshPathAfterInstall { get; init; }

    public required string VerificationExecutable { get; init; }

    public required IReadOnlyList<string> VerificationArguments { get; init; }

    public required PackageInstallationCommandPreview CommandPreview { get; init; }
}
