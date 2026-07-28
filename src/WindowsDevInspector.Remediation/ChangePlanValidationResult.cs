namespace WindowsDevInspector.Remediation;

public sealed record ChangePlanValidationResult
{
    public required bool IsValid { get; init; }

    public required IReadOnlyList<string> Errors { get; init; }

    public static ChangePlanValidationResult Valid()
    {
        return new ChangePlanValidationResult
        {
            IsValid = true,
            Errors = []
        };
    }

    public static ChangePlanValidationResult Invalid(IReadOnlyList<string> errors)
    {
        return new ChangePlanValidationResult
        {
            IsValid = false,
            Errors = errors
        };
    }
}
