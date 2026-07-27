namespace WindowsDevInspector.Core;

public sealed record CheckDefinition
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string Category { get; init; }

    public CheckSeverity SeverityWhenMissing { get; init; } = CheckSeverity.Info;

    public bool CanFix { get; init; }

    public RiskLevel Risk { get; init; } = RiskLevel.None;

    public bool RequiresElevation { get; init; }

    public bool RequiresRestart { get; init; }

    public bool SupportsRollback { get; init; }

    public string? RemediationId { get; init; }
}
