using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows;

public sealed class WslVersionCheck(ICommandRunner commandRunner) : IEnvironmentCheck
{
    public string Id => "devops.wsl-version";

    public async Task<CheckResult> RunAsync(CancellationToken cancellationToken)
    {
        CommandRunResult result = await commandRunner.RunAsync(
            "wsl",
            "--version",
            TimeSpan.FromSeconds(8),
            cancellationToken);

        bool found = result.Succeeded && !string.IsNullOrWhiteSpace(result.StandardOutput);

        return new CheckResult
        {
            Id = Id,
            Category = "DevOps",
            Name = "WSL version",
            Severity = found ? CheckSeverity.Pass : CheckSeverity.Info,
            CurrentValue = found ? Summary(result.StandardOutput) : FailureMessage(result),
            ExpectedValue = "Readable WSL version information",
            Impact = found
                ? "WSL version details are available as informational context. The app does not require the newest WSL version."
                : "WSL version details could not be read; this is informational unless WSL-dependent workflows fail.",
            CanFix = false,
            Risk = RiskLevel.None
        };
    }

    private static string Summary(string output)
    {
        string[] lines = output
            .SplitLines()
            .Take(3)
            .ToArray();

        return lines.Length == 0 ? "WSL version returned no details" : string.Join("; ", lines);
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
