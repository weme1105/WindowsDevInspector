using WindowsDevInspector.Core;
using WindowsDevInspector.Remediation;

namespace WindowsDevInspector.App;

public sealed record PackageInstallationCandidate
{
    public required string DiagnosticCheckId { get; init; }

    public required string PackageId { get; init; }

    public required string DisplayName { get; init; }

    public required InstallationSource Source { get; init; }

    public required InstallationAction Action { get; init; }

    public required RiskLevel Risk { get; init; }

    public required string CurrentValue { get; init; }

    public required string Impact { get; init; }

    public InstallationPlanItem ToPlanItem()
    {
        return new InstallationPlanItem
        {
            PackageId = PackageId,
            Source = Source,
            Action = Action
        };
    }
}
