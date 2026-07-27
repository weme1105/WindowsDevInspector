using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows;

public sealed class CommandVersionCheck(
    string id,
    string category,
    string name,
    string fileName,
    string arguments,
    ICommandRunner commandRunner,
    TimeSpan? timeout = null) : IEnvironmentCheck
{
    private readonly TimeSpan timeout = timeout ?? TimeSpan.FromSeconds(5);

    public string Id => id;

    public async Task<CheckResult> RunAsync(CancellationToken cancellationToken)
    {
        CommandRunResult runResult = await commandRunner.RunAsync(fileName, arguments, timeout, cancellationToken);
        bool passed = runResult.Succeeded;

        return new CheckResult
        {
            Id = id,
            Category = category,
            Name = name,
            Severity = passed ? CheckSeverity.Pass : CheckSeverity.Warning,
            CurrentValue = passed ? FirstMeaningfulLine(runResult) : FailureMessage(runResult),
            ExpectedValue = $"{fileName} {arguments}".Trim(),
            Impact = passed
                ? "The command is available."
                : "The command could not be executed. Tooling that depends on it may fail.",
            CanFix = false,
            Risk = RiskLevel.None
        };
    }

    private static string FirstMeaningfulLine(CommandRunResult runResult)
    {
        string combined = string.Join(
            Environment.NewLine,
            runResult.StandardOutput,
            runResult.StandardError);

        return combined
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault() ?? $"Exit code {runResult.ExitCode}";
    }

    private static string FailureMessage(CommandRunResult runResult)
    {
        if (runResult.TimedOut)
        {
            return "Timed out";
        }

        if (runResult.ErrorMessage is not null)
        {
            return runResult.ErrorMessage;
        }

        return $"Exit code {runResult.ExitCode}";
    }
}
