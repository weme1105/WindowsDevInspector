namespace WindowsDevInspector.ElevatedWorker;

public sealed record WorkerCommandParseResult
{
    public WorkerCommand? Command { get; init; }

    public string? Error { get; init; }

    public bool IsValid => Command is not null && Error is null;

    public static WorkerCommandParseResult Valid(WorkerCommand command) => new()
    {
        Command = command
    };

    public static WorkerCommandParseResult Invalid(string error) => new()
    {
        Error = error
    };
}
