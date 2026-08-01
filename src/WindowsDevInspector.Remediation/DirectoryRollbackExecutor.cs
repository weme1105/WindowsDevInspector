namespace WindowsDevInspector.Remediation;

public sealed class DirectoryRollbackExecutor(IRemediationFileSystem fileSystem)
{
    private static readonly IReadOnlyDictionary<string, string> ApprovedTargets =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["create-source-directory"] = "D:\\Source",
            ["create-projects-directory"] = "D:\\Projects",
            ["create-note-directory"] = "D:\\Note"
        };

    public Task<RemediationExecutionResult> RollbackAsync(
        DirectoryBackup backup,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!ApprovedTargets.TryGetValue(backup.RemediationId, out string? approvedPath)
            || !string.Equals(approvedPath, backup.Path, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(RemediationExecutionResult.Skip(
                "rollback-directory",
                "Backup target is not approved for directory rollback."));
        }

        if (backup.ExistedBeforeRemediation)
        {
            return Task.FromResult(RemediationExecutionResult.Success(
                "rollback-directory",
                "Directory existed before remediation; rollback does not delete pre-existing user directories."));
        }

        if (!fileSystem.DirectoryExists(backup.Path))
        {
            return Task.FromResult(RemediationExecutionResult.Success(
                "rollback-directory",
                "Directory no longer exists; nothing to rollback."));
        }

        if (!fileSystem.IsDirectoryEmpty(backup.Path))
        {
            return Task.FromResult(RemediationExecutionResult.Skip(
                "rollback-directory",
                "Directory is not empty; rollback refused to delete user content."));
        }

        fileSystem.DeleteDirectory(backup.Path);

        return Task.FromResult(RemediationExecutionResult.Success(
            "rollback-directory",
            $"Directory removed: {backup.Path}"));
    }
}
