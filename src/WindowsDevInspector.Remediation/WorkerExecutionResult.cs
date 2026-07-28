namespace WindowsDevInspector.Remediation;

public sealed record WorkerExecutionResult
{
    public required string PlanId { get; init; }

    public required bool Succeeded { get; init; }

    public required IReadOnlyList<string> Errors { get; init; }

    public required IReadOnlyList<RemediationExecutionResult> Results { get; init; }

    public static WorkerExecutionResult Failed(string planId, IReadOnlyList<string> errors)
    {
        return new WorkerExecutionResult
        {
            PlanId = planId,
            Succeeded = false,
            Errors = errors,
            Results = []
        };
    }
}
