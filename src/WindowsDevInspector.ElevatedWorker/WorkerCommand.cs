namespace WindowsDevInspector.ElevatedWorker;

public enum WorkerCommandMode
{
    Remediation,
    Rollback,
    Installation
}

public sealed record WorkerCommand
{
    public required WorkerCommandMode Mode { get; init; }

    public required string InputPath { get; init; }

    public string? ResultPath { get; init; }
}
