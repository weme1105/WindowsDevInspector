using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows.Tests;

public sealed class AndroidSdkCheckTests
{
    [Fact]
    public async Task RunAsync_ReturnsPassWhenSdkRootExists()
    {
        AndroidSdkCheck check = new(
            new FakeEnvironmentVariableReader("D:\\Android\\Sdk"),
            new FakeFileSystem(["D:\\Android\\Sdk"], ["D:\\Android\\Sdk\\platform-tools\\adb.exe"]));

        CheckResult result = await check.RunAsync(CancellationToken.None);

        Assert.Equal(CheckSeverity.Pass, result.Severity);
        Assert.Contains("D:\\Android\\Sdk", result.CurrentValue);
        Assert.Contains("found", result.CurrentValue);
    }

    [Fact]
    public async Task RunAsync_ReturnsInfoWhenSdkRootIsMissing()
    {
        AndroidSdkCheck check = new(
            new FakeEnvironmentVariableReader(null),
            new FakeFileSystem([], []));

        CheckResult result = await check.RunAsync(CancellationToken.None);

        Assert.Equal(CheckSeverity.Info, result.Severity);
        Assert.Contains("not found", result.CurrentValue);
    }

    private sealed class FakeEnvironmentVariableReader(string? sdkRoot) : IEnvironmentVariableReader
    {
        public string? GetEnvironmentVariable(string variable, EnvironmentVariableTarget target)
        {
            return variable is "ANDROID_HOME" or "ANDROID_SDK_ROOT" ? sdkRoot : null;
        }
    }

    private sealed class FakeFileSystem(IEnumerable<string> directories, IEnumerable<string> files) : IFileSystem
    {
        private readonly HashSet<string> directories = new(directories, StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> files = new(files, StringComparer.OrdinalIgnoreCase);

        public bool DirectoryExists(string path) => directories.Contains(path);

        public bool FileExists(string path) => files.Contains(path);
    }
}
