namespace WindowsDevInspector.Remediation;

public sealed record RegistryDwordValue
{
    public required bool Exists { get; init; }

    public int? Value { get; init; }

    public string? ErrorMessage { get; init; }
}
