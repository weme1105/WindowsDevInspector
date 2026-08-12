namespace WindowsDevInspector.App;

public sealed record PackageInstallationConfirmationBuildResult
{
    public required bool IsValid { get; init; }

    public PackageInstallationConfirmation? Confirmation { get; init; }

    public required IReadOnlyList<string> Errors { get; init; }

    public static PackageInstallationConfirmationBuildResult Accepted(
        PackageInstallationConfirmation confirmation)
    {
        return new PackageInstallationConfirmationBuildResult
        {
            IsValid = true,
            Confirmation = confirmation,
            Errors = []
        };
    }

    public static PackageInstallationConfirmationBuildResult Rejected(IReadOnlyList<string> errors)
    {
        return new PackageInstallationConfirmationBuildResult
        {
            IsValid = false,
            Confirmation = null,
            Errors = errors
        };
    }
}
