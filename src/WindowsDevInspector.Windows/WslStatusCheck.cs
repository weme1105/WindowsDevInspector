using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows;

public sealed class WslStatusCheck(ICommandRunner commandRunner) : IEnvironmentCheck
{
    public string Id => "devops.wsl";

    public async Task<CheckResult> RunAsync(CancellationToken cancellationToken)
    {
        CommandRunResult result = await commandRunner.RunAsync(
            "wsl",
            "--status",
            TimeSpan.FromSeconds(8),
            cancellationToken);

        bool found = result.Succeeded && !string.IsNullOrWhiteSpace(result.StandardOutput);

        return new CheckResult
        {
            Id = Id,
            Category = "DevOps",
            Name = "WSL installed",
            Severity = found ? CheckSeverity.Pass : CheckSeverity.Warning,
            CurrentValue = found ? FirstMeaningfulLine(result.StandardOutput) : FailureMessage(result),
            ExpectedValue = "wsl --status returns installed WSL status",
            Impact = found
                ? "WSL is available for Linux-based local development workflows."
                : "WSL could not be inspected. Docker, Kubernetes, or Linux tooling may be unavailable.",
            CanFix = false,
            Risk = RiskLevel.None
        };
    }

    private static string FirstMeaningfulLine(string output)
    {
        return output
            .SplitLines()
            .FirstOrDefault() ?? "WSL status returned no details";
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
