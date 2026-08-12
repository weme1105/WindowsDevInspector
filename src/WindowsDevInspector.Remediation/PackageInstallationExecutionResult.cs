namespace WindowsDevInspector.Remediation;

public sealed record PackageInstallationExecutionResult
{
    public required string PlanId { get; init; }

    public required bool Succeeded { get; init; }

    public required IReadOnlyList<string> Errors { get; init; }

    public required IReadOnlyList<PackageInstallationItemResult> Results { get; init; }

    public static PackageInstallationExecutionResult Invalid(
        string planId,
        IReadOnlyList<string> errors)
    {
        return new PackageInstallationExecutionResult
        {
            PlanId = planId,
            Succeeded = false,
            Errors = errors,
            Results = []
        };
    }
}
