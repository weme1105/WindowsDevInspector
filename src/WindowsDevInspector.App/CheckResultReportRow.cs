namespace WindowsDevInspector.App;

public sealed record CheckResultReportRow
{
    public required string Id { get; init; }

    public required string Severity { get; init; }

    public required string Category { get; init; }

    public required string Name { get; init; }

    public required string CurrentValue { get; init; }

    public required string ExpectedValue { get; init; }

    public required string Impact { get; init; }

    public required bool CanFix { get; init; }

    public required string? RemediationId { get; init; }
}
