using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows.Tests;

public sealed class VisualStudioBuildToolsCheckTests
{
    [Fact]
    public async Task RunAsync_ReturnsPassWhenVsWhereFindsMsBuildInstance()
    {
        VisualStudioBuildToolsCheck check = new(
            new FakeCommandRunner(new CommandRunResult
            {
                FileName = "vswhere",
                Arguments = "-products * -requires Microsoft.Component.MSBuild -property displayName",
                ExitCode = 0,
                StandardOutput = "Visual Studio Build Tools 2026"
            }),
            new FakeFileSystem(fileExists: true));

        CheckResult result = await check.RunAsync(CancellationToken.None);

        Assert.Equal(CheckSeverity.Pass, result.Severity);
        Assert.Contains("Build Tools", result.CurrentValue);
    }

    [Fact]
    public async Task RunAsync_ReturnsInfoWhenVsWhereIsMissing()
    {
        VisualStudioBuildToolsCheck check = new(
            new FakeCommandRunner(new CommandRunResult
            {
                FileName = "vswhere",
                Arguments = "-products * -requires Microsoft.Component.MSBuild -property displayName"
            }),
            new FakeFileSystem(fileExists: false));

        CheckResult result = await check.RunAsync(CancellationToken.None);

        Assert.Equal(CheckSeverity.Info, result.Severity);
        Assert.Contains("vswhere.exe not found", result.CurrentValue);
    }

    private sealed class FakeCommandRunner(CommandRunResult result) : ICommandRunner
    {
        public Task<CommandRunResult> RunAsync(string fileName, string arguments, TimeSpan timeout, CancellationToken cancellationToken)
        {
            Assert.Contains("Microsoft.Component.MSBuild", arguments);
            return Task.FromResult(result);
        }
    }

    private sealed class FakeFileSystem(bool fileExists) : IFileSystem
    {
        public bool DirectoryExists(string path) => false;

        public bool FileExists(string path) => fileExists;
    }
}
