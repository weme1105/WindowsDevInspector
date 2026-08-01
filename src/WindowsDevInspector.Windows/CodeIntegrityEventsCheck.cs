using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows;

public sealed class CodeIntegrityEventsCheck(ICommandRunner commandRunner) : IEnvironmentCheck
{
    public string Id => "security.code-integrity-events";

    public async Task<CheckResult> RunAsync(CancellationToken cancellationToken)
    {
        CommandRunResult result = await commandRunner.RunAsync(
            "wevtutil",
            "qe Microsoft-Windows-CodeIntegrity/Operational /c:5 /rd:true /f:text",
            TimeSpan.FromSeconds(8),
            cancellationToken);

        if (!result.Succeeded)
        {
            return new CheckResult
            {
                Id = Id,
                Category = "Security",
                Name = "Code Integrity events",
                Severity = CheckSeverity.Info,
                CurrentValue = FailureMessage(result),
                ExpectedValue = "Recent Microsoft-Windows-CodeIntegrity/Operational events are inspectable",
                Impact = "Code Integrity event history could not be inspected. Unsigned driver or blocked app issues may require Event Viewer follow-up.",
                CanFix = false,
                Risk = RiskLevel.None
            };
        }

        string summary = FirstEventSummary(result.StandardOutput);
        bool hasEvents = !summary.Equals("No recent Code Integrity events", StringComparison.OrdinalIgnoreCase);

        return new CheckResult
        {
            Id = Id,
            Category = "Security",
            Name = "Code Integrity events",
            Severity = hasEvents ? CheckSeverity.Info : CheckSeverity.Pass,
            CurrentValue = summary,
            ExpectedValue = "No recent blocking Code Integrity events that explain developer tool failures",
            Impact = hasEvents
                ? "Recent Code Integrity events exist. Review Event Viewer if a driver, emulator, debugger, or tool launch is blocked."
                : "No recent Code Integrity events were returned by the operational log query.",
            CanFix = false,
            Risk = RiskLevel.None
        };
    }

    private static string FirstEventSummary(string output)
    {
        string[] lines = output.SplitLines().ToArray();
        if (lines.Length == 0)
        {
            return "No recent Code Integrity events";
        }

        string? eventId = lines.FirstOrDefault(line => line.StartsWith("Event ID:", StringComparison.OrdinalIgnoreCase));
        string? timeCreated = lines.FirstOrDefault(line => line.StartsWith("Date:", StringComparison.OrdinalIgnoreCase));

        if (eventId is not null && timeCreated is not null)
        {
            return $"{eventId}; {timeCreated}";
        }

        return lines.First();
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
