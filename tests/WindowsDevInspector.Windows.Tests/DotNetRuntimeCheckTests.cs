using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows.Tests;

public sealed class DotNetRuntimeCheckTests
{
    [Fact]
    public async Task RunAsync_ReturnsPassWhenRuntimeIsListed()
    {
        FakeCommandRunner commandRunner = new(new CommandRunResult
        {
            FileName = "dotnet",
            Arguments = "--list-runtimes",
            ExitCode = 0,
            StandardOutput = """
                Microsoft.AspNetCore.App 10.0.0 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]
                Microsoft.NETCore.App 10.0.0 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
                Microsoft.WindowsDesktop.App 10.0.0 [C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App]
                """
        });
        DotNetRuntimeCheck check = new(
            "backend.aspnet-runtime",
            "Backend",
            "ASP.NET Core runtime",
            "Microsoft.AspNetCore.App",
            commandRunner);

        CheckResult result = await check.RunAsync(CancellationToken.None);

        Assert.Equal(CheckSeverity.Pass, result.Severity);
        Assert.Contains("Microsoft.AspNetCore.App 10.0.0", result.CurrentValue);
        Assert.Equal("dotnet", commandRunner.FileName);
        Assert.Equal("--list-runtimes", commandRunner.Arguments);
    }

    [Fact]
    public async Task RunAsync_ReturnsInfoWhenRuntimeIsNotListed()
    {
        DotNetRuntimeCheck check = new(
            "desktop.dotnet-desktop-runtime",
            "Desktop",
            ".NET Desktop Runtime",
            "Microsoft.WindowsDesktop.App",
            new FakeCommandRunner(new CommandRunResult
            {
                FileName = "dotnet",
                Arguments = "--list-runtimes",
                ExitCode = 0,
                StandardOutput = "Microsoft.NETCore.App 10.0.0 [C:\\Program Files\\dotnet\\shared\\Microsoft.NETCore.App]"
            }));

        CheckResult result = await check.RunAsync(CancellationToken.None);

        Assert.Equal(CheckSeverity.Info, result.Severity);
        Assert.Equal("Runtime not found", result.CurrentValue);
        Assert.False(result.CanFix);
    }

    [Fact]
    public async Task RunAsync_ReturnsInfoWhenOnlyOlderDesktopRuntimeIsListed()
    {
        DotNetRuntimeCheck check = new(
            "desktop.dotnet-desktop-runtime",
            "Common",
            ".NET 10 Desktop Runtime x64",
            "Microsoft.WindowsDesktop.App",
            new FakeCommandRunner(new CommandRunResult
            {
                FileName = "dotnet",
                Arguments = "--list-runtimes",
                ExitCode = 0,
                StandardOutput = "Microsoft.WindowsDesktop.App 9.0.8 [C:\\Program Files\\dotnet\\shared\\Microsoft.WindowsDesktop.App]"
            }),
            minimumMajorVersion: 10);

        CheckResult result = await check.RunAsync(CancellationToken.None);

        Assert.Equal(CheckSeverity.Info, result.Severity);
        Assert.Contains("10.x x64", result.ExpectedValue);
    }

    [Fact]
    public async Task RunAsync_ReturnsInfoWhenDotnetIsMissing()
    {
        DotNetRuntimeCheck check = new(
            "backend.aspnet-runtime",
            "Backend",
            "ASP.NET Core runtime",
            "Microsoft.AspNetCore.App",
            new FakeCommandRunner(new CommandRunResult
            {
                FileName = "dotnet",
                Arguments = "--list-runtimes",
                ErrorMessage = "The system cannot find the file specified."
            }));

        CheckResult result = await check.RunAsync(CancellationToken.None);

        Assert.Equal(CheckSeverity.Info, result.Severity);
        Assert.Contains("cannot find", result.CurrentValue);
    }

    [Fact]
    public async Task RunAsync_ReturnsInfoWhenDotnetTimesOut()
    {
        DotNetRuntimeCheck check = new(
            "desktop.dotnet-desktop-runtime",
            "Desktop",
            ".NET Desktop Runtime",
            "Microsoft.WindowsDesktop.App",
            new FakeCommandRunner(new CommandRunResult
            {
                FileName = "dotnet",
                Arguments = "--list-runtimes",
                TimedOut = true
            }));

        CheckResult result = await check.RunAsync(CancellationToken.None);

        Assert.Equal(CheckSeverity.Info, result.Severity);
        Assert.Equal("Timed out", result.CurrentValue);
    }

    private sealed class FakeCommandRunner(CommandRunResult result) : ICommandRunner
    {
        public string? FileName { get; private set; }

        public string? Arguments { get; private set; }

        public Task<CommandRunResult> RunAsync(string fileName, string arguments, TimeSpan timeout, CancellationToken cancellationToken)
        {
            FileName = fileName;
            Arguments = arguments;

            return Task.FromResult(result);
        }
    }
}
