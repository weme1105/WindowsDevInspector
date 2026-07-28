namespace WindowsDevInspector.Remediation;

public sealed record RegistryDwordRemediationDefinition
{
    public required string RemediationId { get; init; }

    public required string Hive { get; init; }

    public required string SubKeyPath { get; init; }

    public required string ValueName { get; init; }

    public required int DesiredValue { get; init; }
}
