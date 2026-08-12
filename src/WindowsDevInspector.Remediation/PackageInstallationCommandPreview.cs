namespace WindowsDevInspector.Remediation;

public sealed record PackageInstallationCommandPreview
{
    public required string PackageId { get; init; }

    public required InstallationSource Source { get; init; }

    public required string FileName { get; init; }

    public required IReadOnlyList<string> Arguments { get; init; }

    public string DisplayCommand => string.Join(" ", new[] { FileName }.Concat(Arguments));
}
