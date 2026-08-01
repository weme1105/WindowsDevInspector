using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows;

public sealed class WslDistributionsCheck(ICommandRunner commandRunner) : IEnvironmentCheck
{
    public string Id => "devops.wsl-distros";

    public async Task<CheckResult> RunAsync(CancellationToken cancellationToken)
    {
        CommandRunResult result = await commandRunner.RunAsync(
            "wsl",
            "--list --verbose",
            TimeSpan.FromSeconds(8),
            cancellationToken);

        string[] distributions = result.Succeeded ? ParseDistributionLines(result.StandardOutput) : [];
        bool hasDistribution = distributions.Length > 0;

        return new CheckResult
        {
            Id = Id,
            Category = "DevOps",
            Name = "WSL distributions",
            Severity = hasDistribution ? CheckSeverity.Pass : CheckSeverity.Info,
            CurrentValue = hasDistribution ? string.Join("; ", distributions) : FailureMessage(result),
            ExpectedValue = "At least one WSL distribution listed when Linux workflows are needed",
            Impact = hasDistribution
                ? "WSL distributions are available for Linux-based local development."
                : "No WSL distribution was listed. WSL-dependent workflows may need a distribution installed.",
            CanFix = false,
            Risk = RiskLevel.None
        };
    }

    private static string[] ParseDistributionLines(string output)
    {
        return output
            .SplitLines()
            .Where(line => !line.StartsWith("NAME", StringComparison.OrdinalIgnoreCase))
            .Where(line => !line.Contains("Windows Subsystem for Linux has no installed distributions.", StringComparison.OrdinalIgnoreCase))
            .Select(line => line.TrimStart('*').Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();
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

        if (!result.Succeeded)
        {
            return string.IsNullOrWhiteSpace(result.StandardError)
                ? $"Exit code {result.ExitCode}"
                : result.StandardError.Trim();
        }

        return "No WSL distributions listed";
    }
}
