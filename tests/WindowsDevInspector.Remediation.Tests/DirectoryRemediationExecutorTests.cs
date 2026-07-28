using WindowsDevInspector.Core;

namespace WindowsDevInspector.Remediation.Tests;

public sealed class DirectoryRemediationExecutorTests
{
    [Fact]
    public async Task ExecuteAsync_CreatesMissingWhitelistedDirectory()
    {
        FakeRemediationFileSystem fileSystem = new();
        DirectoryRemediationExecutor executor = new(BuiltInRemediationCatalog.CreateWhitelist(), fileSystem);

        RemediationExecutionResult result = await executor.ExecuteAsync(
            "create-source-directory",
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.False(result.Skipped);
        Assert.Contains("D:\\Source", fileSystem.CreatedDirectories);
    }

    [Fact]
    public async Task ExecuteAsync_SucceedsWhenDirectoryAlreadyExists()
    {
        FakeRemediationFileSystem fileSystem = new(["D:\\Projects"]);
        DirectoryRemediationExecutor executor = new(BuiltInRemediationCatalog.CreateWhitelist(), fileSystem);

        RemediationExecutionResult result = await executor.ExecuteAsync(
            "create-projects-directory",
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Empty(fileSystem.CreatedDirectories);
    }

    [Fact]
    public async Task ExecuteAsync_SkipsElevatedRemediation()
    {
        FakeRemediationFileSystem fileSystem = new();
        DirectoryRemediationExecutor executor = new(BuiltInRemediationCatalog.CreateWhitelist(), fileSystem);

        RemediationExecutionResult result = await executor.ExecuteAsync(
            "enable-long-paths",
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.True(result.Skipped);
        Assert.Contains("elevated worker", result.Message);
        Assert.Empty(fileSystem.CreatedDirectories);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsUnknownRemediation()
    {
        DirectoryRemediationExecutor executor = new(BuiltInRemediationCatalog.CreateWhitelist(), new FakeRemediationFileSystem());

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => executor.ExecuteAsync("run-arbitrary-command", CancellationToken.None));

        Assert.Contains("not whitelisted", exception.Message);
    }

    private sealed class FakeRemediationFileSystem(IEnumerable<string>? existingDirectories = null) : IRemediationFileSystem
    {
        private readonly HashSet<string> existingDirectories = new(
            existingDirectories ?? [],
            StringComparer.OrdinalIgnoreCase);

        public List<string> CreatedDirectories { get; } = [];

        public bool DirectoryExists(string path)
        {
            return existingDirectories.Contains(path);
        }

        public void CreateDirectory(string path)
        {
            CreatedDirectories.Add(path);
            existingDirectories.Add(path);
        }
    }
}
