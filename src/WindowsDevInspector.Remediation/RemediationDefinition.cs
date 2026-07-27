using WindowsDevInspector.Core;

namespace WindowsDevInspector.Remediation;

public sealed record RemediationDefinition
{
    public required string Id { get; init; }

    public required string DisplayName { get; init; }

    public required RiskLevel Risk { get; init; }

    public bool RequiresElevation { get; init; }

    public bool RequiresRestart { get; init; }

    public bool SupportsRollback { get; init; }
}
