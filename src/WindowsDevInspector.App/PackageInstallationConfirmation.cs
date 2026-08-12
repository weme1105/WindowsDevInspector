namespace WindowsDevInspector.App;

public sealed record PackageInstallationConfirmation
{
    public required string PlanId { get; init; }

    public required IReadOnlyList<PackageInstallationConfirmationItem> Items { get; init; }

    public required string Message { get; init; }
}
