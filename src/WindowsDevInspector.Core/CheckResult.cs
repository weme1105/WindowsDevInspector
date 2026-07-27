namespace WindowsDevInspector.Core;

public sealed record CheckResult
{
    public required string Id { get; init; }

    public required string Category { get; init; }

    public required string Name { get; init; }

    public required CheckSeverity Severity { get; init; }

    public required string CurrentValue { get; init; }

    public required string ExpectedValue { get; init; }

    public required string Impact { get; init; }

    public bool CanFix { get; init; }

    public RiskLevel Risk { get; init; } = RiskLevel.None;

    public bool RequiresElevation { get; init; }

    public bool RequiresRestart { get; init; }

    public bool SupportsRollback { get; init; }

    public string? RemediationId { get; init; }
}
