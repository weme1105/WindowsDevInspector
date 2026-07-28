namespace WindowsDevInspector.Remediation;

public sealed record RemediationExecutionResult
{
    public required string RemediationId { get; init; }

    public required bool Succeeded { get; init; }

    public required bool Skipped { get; init; }

    public required string Message { get; init; }

    public string? BackupJson { get; init; }

    public string? BackupPath { get; init; }

    public static RemediationExecutionResult Success(
        string remediationId,
        string message,
        string? backupJson = null,
        string? backupPath = null)
    {
        return new RemediationExecutionResult
        {
            RemediationId = remediationId,
            Succeeded = true,
            Skipped = false,
            Message = message,
            BackupJson = backupJson,
            BackupPath = backupPath
        };
    }

    public static RemediationExecutionResult Skip(string remediationId, string message)
    {
        return new RemediationExecutionResult
        {
            RemediationId = remediationId,
            Succeeded = false,
            Skipped = true,
            Message = message,
            BackupJson = null,
            BackupPath = null
        };
    }

    public RemediationExecutionResult WithBackupPath(string backupPath)
    {
        return this with { BackupPath = backupPath };
    }
}
