using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows;

public sealed class DockerDesktopCheck(ICommandRunner commandRunner) : IEnvironmentCheck
{
    public string Id => "devops.docker-desktop";

    public async Task<CheckResult> RunAsync(CancellationToken cancellationToken)
    {
        CommandRunResult result = await commandRunner.RunAsync(
            "docker",
            "info",
            TimeSpan.FromSeconds(8),
            cancellationToken);

        bool running = result.Succeeded;

        return new CheckResult
        {
            Id = Id,
            Category = "DevOps",
            Name = "Docker Desktop",
            Severity = running ? CheckSeverity.Pass : CheckSeverity.Warning,
            CurrentValue = running ? DockerSummary(result.StandardOutput) : FailureMessage(result),
            ExpectedValue = "docker info succeeds against a running Docker engine",
            Impact = running
                ? "Docker engine is reachable for local container workflows."
                : "Docker CLI is installed or selectable, but the Docker engine is not reachable. Docker Desktop may be stopped or misconfigured.",
            CanFix = false,
            Risk = RiskLevel.None
        };
    }

    private static string DockerSummary(string output)
    {
        string[] interestingLines = output
            .SplitLines()
            .Where(line =>
                line.StartsWith("Server Version:", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("OSType:", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("Operating System:", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("Docker Root Dir:", StringComparison.OrdinalIgnoreCase))
            .Take(4)
            .ToArray();

        return interestingLines.Length == 0 ? "Docker engine responded" : string.Join("; ", interestingLines);
    }

    private static string FailureMessage(CommandRunResult result)
    {
        if (result.TimedOut)
        {
            return "Timed out";
        }

        if (result.ErrorMessage is not null)
        {
            return result.ErrorMessage;
        }

        return string.IsNullOrWhiteSpace(result.StandardError)
            ? $"Exit code {result.ExitCode}"
            : result.StandardError.Trim();
    }
}
