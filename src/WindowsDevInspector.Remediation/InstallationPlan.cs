using System.Text.Json.Serialization;

namespace WindowsDevInspector.Remediation;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record InstallationPlan
{
    public required string PlanId { get; init; }

    public required IReadOnlyList<InstallationPlanItem> Items { get; init; }
}
