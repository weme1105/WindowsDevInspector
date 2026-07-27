namespace WindowsDevInspector.Windows;

public sealed record ServiceReadResult
{
    public bool Exists { get; init; }

    public string? Status { get; init; }

    public string? ErrorMessage { get; init; }
}
