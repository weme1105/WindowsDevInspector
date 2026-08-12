using System.Text.Json.Serialization;

namespace WindowsDevInspector.Remediation;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record InstallationPlanItem
{
    public required string PackageId { get; init; }

    public required InstallationSource Source { get; init; }

    public required InstallationAction Action { get; init; }
}
