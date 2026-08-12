namespace WindowsDevInspector.Remediation;

public sealed record PackageInstallationCommandRunResult
{
    public int? ExitCode { get; init; }

    public bool TimedOut { get; init; }

    public required string StandardOutput { get; init; }

    public required string StandardError { get; init; }

    public string? ErrorMessage { get; init; }
}
