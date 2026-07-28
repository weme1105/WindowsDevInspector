namespace WindowsDevInspector.Remediation;

public sealed record ChangePlan
{
    public required string PlanId { get; init; }

    public required IReadOnlyList<ChangePlanItem> Items { get; init; }
}
