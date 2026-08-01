using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows.Tests;

public sealed class NuGetSourcesCheckTests
{
    [Fact]
    public async Task RunAsync_RedactsCredentialsFromSourceUrls()
    {
        NuGetSourcesCheck check = new(new FakeCommandRunner(new CommandRunResult
        {
            FileName = "dotnet",
            Arguments = "nuget list source",
            ExitCode = 0,
            StandardOutput = "1. Contoso [Enabled]\n   https://user:secret@example.test/v3/index.json?token=abc"
        }));

        CheckResult result = await check.RunAsync(CancellationToken.None);

        Assert.Equal(CheckSeverity.Pass, result.Severity);
        Assert.DoesNotContain("secret", result.CurrentValue, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("token=abc", result.CurrentValue, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("<redacted>", result.CurrentValue);
    }

    [Fact]
    public async Task RunAsync_ReturnsInfoWhenCommandFails()
    {
        NuGetSourcesCheck check = new(new FakeCommandRunner(new CommandRunResult
        {
            FileName = "dotnet",
            Arguments = "nuget list source",
            ExitCode = 1,
            StandardError = "No sources found"
        }));

        CheckResult result = await check.RunAsync(CancellationToken.None);

        Assert.Equal(CheckSeverity.Info, result.Severity);
        Assert.Contains("No sources", result.CurrentValue);
    }

    private sealed class FakeCommandRunner(CommandRunResult result) : ICommandRunner
    {
        public Task<CommandRunResult> RunAsync(string fileName, string arguments, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return Task.FromResult(result);
        }
    }
}
