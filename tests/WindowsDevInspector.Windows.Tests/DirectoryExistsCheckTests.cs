using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows.Tests;

public sealed class DirectoryExistsCheckTests
{
    [Fact]
    public async Task RunAsync_ReturnsPassWhenDirectoryExists()
    {
        DirectoryExistsCheck check = new(
            "common.directory-source",
            "Common",
            "D:\\Source exists",
            "D:\\Source",
            new FakeFileSystem(["D:\\Source"]),
            "create-source-directory");

        CheckResult result = await check.RunAsync(CancellationToken.None);

        Assert.Equal(CheckSeverity.Pass, result.Severity);
        Assert.False(result.CanFix);
    }

    [Fact]
    public async Task RunAsync_ReturnsWarningAndRemediationWhenDirectoryIsMissing()
    {
        DirectoryExistsCheck check = new(
            "common.directory-source",
            "Common",
            "D:\\Source exists",
            "D:\\Source",
            new FakeFileSystem([]),
            "create-source-directory");

        CheckResult result = await check.RunAsync(CancellationToken.None);

        Assert.Equal(CheckSeverity.Warning, result.Severity);
        Assert.True(result.CanFix);
        Assert.Equal("create-source-directory", result.RemediationId);
    }

    private sealed class FakeFileSystem(IEnumerable<string> existingDirectories) : IFileSystem
    {
        private readonly HashSet<string> existingDirectories = new(existingDirectories, StringComparer.OrdinalIgnoreCase);

        public bool DirectoryExists(string path)
        {
            return existingDirectories.Contains(path);
        }
    }
}
