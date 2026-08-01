namespace WindowsDevInspector.Remediation.Tests;

public sealed class DirectoryRollbackExecutorTests
{
    [Fact]
    public async Task RollbackAsync_RemovesToolCreatedEmptyDirectory()
    {
        FakeRemediationFileSystem fileSystem = new(["D:\\Source"]);
        DirectoryRollbackExecutor executor = new(fileSystem);

        RemediationExecutionResult result = await executor.RollbackAsync(new DirectoryBackup
        {
            RemediationId = "create-source-directory",
            Path = "D:\\Source",
            ExistedBeforeRemediation = false
        }, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Contains("D:\\Source", fileSystem.DeletedDirectories);
    }

    [Fact]
    public async Task RollbackAsync_DoesNotDeletePreExistingDirectory()
    {
        FakeRemediationFileSystem fileSystem = new(["D:\\Projects"]);
        DirectoryRollbackExecutor executor = new(fileSystem);

        RemediationExecutionResult result = await executor.RollbackAsync(new DirectoryBackup
        {
            RemediationId = "create-projects-directory",
            Path = "D:\\Projects",
            ExistedBeforeRemediation = true
        }, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Empty(fileSystem.DeletedDirectories);
    }

    [Fact]
    public async Task RollbackAsync_RefusesToDeleteNonEmptyDirectory()
    {
        FakeRemediationFileSystem fileSystem = new(["D:\\Note"], emptyDirectories: []);
        DirectoryRollbackExecutor executor = new(fileSystem);

        RemediationExecutionResult result = await executor.RollbackAsync(new DirectoryBackup
        {
            RemediationId = "create-note-directory",
            Path = "D:\\Note",
            ExistedBeforeRemediation = false
        }, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.True(result.Skipped);
        Assert.Empty(fileSystem.DeletedDirectories);
    }

    private sealed class FakeRemediationFileSystem(
        IEnumerable<string> existingDirectories,
        IEnumerable<string>? emptyDirectories = null) : IRemediationFileSystem
    {
        private readonly HashSet<string> existingDirectories = new(existingDirectories, StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> emptyDirectories = new(
            emptyDirectories ?? existingDirectories,
            StringComparer.OrdinalIgnoreCase);

        public List<string> DeletedDirectories { get; } = [];

        public bool DirectoryExists(string path)
        {
            return existingDirectories.Contains(path);
        }

        public void CreateDirectory(string path)
        {
            existingDirectories.Add(path);
            emptyDirectories.Add(path);
        }

        public bool IsDirectoryEmpty(string path)
        {
            return emptyDirectories.Contains(path);
        }

        public void DeleteDirectory(string path)
        {
            DeletedDirectories.Add(path);
            existingDirectories.Remove(path);
            emptyDirectories.Remove(path);
        }
    }
}
