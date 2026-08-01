namespace WindowsDevInspector.Remediation;

public sealed record DirectoryBackup
{
    public required string RemediationId { get; init; }

    public required string Path { get; init; }

    public required bool ExistedBeforeRemediation { get; init; }
}
