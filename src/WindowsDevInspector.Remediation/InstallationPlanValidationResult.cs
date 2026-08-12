namespace WindowsDevInspector.Remediation;

public sealed record InstallationPlanValidationResult
{
    public required bool IsValid { get; init; }

    public required IReadOnlyList<string> Errors { get; init; }

    public static InstallationPlanValidationResult Valid()
    {
        return new InstallationPlanValidationResult
        {
            IsValid = true,
            Errors = []
        };
    }

    public static InstallationPlanValidationResult Invalid(IReadOnlyList<string> errors)
    {
        return new InstallationPlanValidationResult
        {
            IsValid = false,
            Errors = errors
        };
    }
}
