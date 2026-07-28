namespace WindowsDevInspector.Remediation.Tests;

public sealed class RemediationExecutionResultTests
{
    [Fact]
    public void WithBackupPath_AddsBackupPathWithoutChangingOutcome()
    {
        RemediationExecutionResult original = RemediationExecutionResult.Success(
            "enable-long-paths",
            "Updated.",
            "{}");

        RemediationExecutionResult updated = original.WithBackupPath("backup.json");

        Assert.True(updated.Succeeded);
        Assert.False(updated.Skipped);
        Assert.Equal(original.RemediationId, updated.RemediationId);
        Assert.Equal(original.BackupJson, updated.BackupJson);
        Assert.Equal("backup.json", updated.BackupPath);
    }
}
