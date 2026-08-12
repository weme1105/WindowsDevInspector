namespace WindowsDevInspector.Windows;

public sealed record LocalhostBindProbeResult
{
    public required bool Succeeded { get; init; }

    public int? Port { get; init; }

    public string? ErrorMessage { get; init; }
}
