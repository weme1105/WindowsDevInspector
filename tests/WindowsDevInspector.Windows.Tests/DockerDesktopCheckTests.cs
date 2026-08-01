using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows.Tests;

public sealed class DockerDesktopCheckTests
{
    [Fact]
    public async Task RunAsync_ReturnsPassWhenDockerInfoSucceeds()
    {
        DockerDesktopCheck check = new(new FakeCommandRunner(new CommandRunResult
        {
            FileName = "docker",
            Arguments = "info",
            ExitCode = 0,
            StandardOutput = "Client:\nServer Version: 28.3.2\nOSType: linux\nDocker Root Dir: /var/lib/docker"
        }));

        CheckResult result = await check.RunAsync(CancellationToken.None);

        Assert.Equal(CheckSeverity.Pass, result.Severity);
        Assert.Contains("Server Version", result.CurrentValue);
        Assert.Contains("engine is reachable", result.Impact);
    }

    [Fact]
    public async Task RunAsync_ReturnsWarningWhenDockerEngineIsUnreachable()
    {
        DockerDesktopCheck check = new(new FakeCommandRunner(new CommandRunResult
        {
            FileName = "docker",
            Arguments = "info",
            ExitCode = 1,
            StandardError = "error during connect"
        }));

        CheckResult result = await check.RunAsync(CancellationToken.None);

        Assert.Equal(CheckSeverity.Warning, result.Severity);
        Assert.Contains("error during connect", result.CurrentValue);
        Assert.Contains("Docker Desktop may be stopped", result.Impact);
    }

    private sealed class FakeCommandRunner(CommandRunResult result) : ICommandRunner
    {
        public Task<CommandRunResult> RunAsync(string fileName, string arguments, TimeSpan timeout, CancellationToken cancellationToken)
        {
            Assert.Equal("docker", fileName);
            Assert.Equal("info", arguments);
            return Task.FromResult(result);
        }
    }
}
