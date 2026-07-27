namespace WindowsDevInspector.Windows;

public sealed record RegistryDwordReadResult
{
    public bool Exists { get; init; }

    public int? Value { get; init; }

    public string? ErrorMessage { get; init; }
}
