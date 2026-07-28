namespace WindowsDevInspector.Remediation;

public sealed record RegistryDwordBackup
{
    public required string Hive { get; init; }

    public required string SubKeyPath { get; init; }

    public required string ValueName { get; init; }

    public required bool Existed { get; init; }

    public int? PreviousValue { get; init; }
}
