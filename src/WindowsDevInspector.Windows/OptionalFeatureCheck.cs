using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows;

public sealed class OptionalFeatureCheck(
    string id,
    string category,
    string name,
    string featureName,
    ICommandRunner commandRunner) : IEnvironmentCheck
{
    public string Id => id;

    public async Task<CheckResult> RunAsync(CancellationToken cancellationToken)
    {
        CommandRunResult result = await commandRunner.RunAsync(
            "dism.exe",
            $"/Online /Get-FeatureInfo /FeatureName:{featureName} /English",
            TimeSpan.FromSeconds(10),
            cancellationToken);

        string output = string.Join(Environment.NewLine, result.StandardOutput, result.StandardError);
        bool enabled = output.Contains("State : Enabled", StringComparison.OrdinalIgnoreCase);
        bool disabled = output.Contains("State : Disabled", StringComparison.OrdinalIgnoreCase);

        return new CheckResult
        {
            Id = id,
            Category = category,
            Name = name,
            Severity = enabled ? CheckSeverity.Pass : CheckSeverity.Info,
            CurrentValue = enabled ? "Enabled" : disabled ? "Disabled" : FailureMessage(result),
            ExpectedValue = featureName,
            Impact = enabled
                ? "The optional Windows feature is enabled."
                : "The optional Windows feature is not enabled or could not be inspected.",
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

        return $"Exit code {result.ExitCode}";
    }
}
