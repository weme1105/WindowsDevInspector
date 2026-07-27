namespace WindowsDevInspector.Windows;

public sealed record CommandRunResult
{
    public required string FileName { get; init; }

    public required string Arguments { get; init; }

    public int? ExitCode { get; init; }

    public string StandardOutput { get; init; } = string.Empty;

    public string StandardError { get; init; } = string.Empty;

    public bool TimedOut { get; init; }

    public string? ErrorMessage { get; init; }

    public bool Succeeded => ExitCode == 0 && !TimedOut && ErrorMessage is null;
}
