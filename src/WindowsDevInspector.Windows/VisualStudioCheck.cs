using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows;

public sealed class VisualStudioCheck(ICommandRunner commandRunner) : IEnvironmentCheck
{
    private const string VsWherePath = "C:\\Program Files (x86)\\Microsoft Visual Studio\\Installer\\vswhere.exe";

    public string Id => "desktop.visualstudio";

    public async Task<CheckResult> RunAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(VsWherePath))
        {
            return new CheckResult
            {
                Id = Id,
                Category = "Desktop",
                Name = "Visual Studio",
                Severity = CheckSeverity.Info,
                CurrentValue = "vswhere.exe not found",
                ExpectedValue = VsWherePath,
                Impact = "Visual Studio Installer was not found in the standard location.",
                CanFix = false,
                Risk = RiskLevel.None
            };
        }

        CommandRunResult result = await commandRunner.RunAsync(
            VsWherePath,
            "-latest -products * -property displayName",
            TimeSpan.FromSeconds(8),
            cancellationToken);

        bool found = result.Succeeded && !string.IsNullOrWhiteSpace(result.StandardOutput);

        return new CheckResult
        {
            Id = Id,
            Category = "Desktop",
            Name = "Visual Studio",
            Severity = found ? CheckSeverity.Pass : CheckSeverity.Info,
            CurrentValue = found ? result.StandardOutput.Trim() : FailureMessage(result),
            ExpectedValue = "Installed Visual Studio instance",
            Impact = found
                ? "Visual Studio was detected through vswhere."
                : "Visual Studio was not detected through vswhere.",
            CanFix = false,
            Risk = RiskLevel.None
        };
    }

    private static string FailureMessage(CommandRunResult result)
    {
        if (result.ErrorMessage is not null)
        {
            return result.ErrorMessage;
        }

        if (result.TimedOut)
        {
            return "Timed out";
        }

        return string.IsNullOrWhiteSpace(result.StandardError)
            ? $"Exit code {result.ExitCode}"
            : result.StandardError.Trim();
    }
}
