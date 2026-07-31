using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows;

public sealed class WingetPackageAvailabilityCheck(
    string id,
    string category,
    string name,
    string packageId,
    ICommandRunner commandRunner,
    TimeSpan? timeout = null) : IEnvironmentCheck
{
    private readonly TimeSpan timeout = timeout ?? TimeSpan.FromSeconds(10);

    public string Id => id;

    public async Task<CheckResult> RunAsync(CancellationToken cancellationToken)
    {
        CommandRunResult runResult = await commandRunner.RunAsync(
            "winget",
            $"show --id {packageId} --exact --accept-source-agreements",
            timeout,
            cancellationToken);

        bool packageFound = runResult.Succeeded;

        return new CheckResult
        {
            Id = id,
            Category = category,
            Name = name,
            Severity = packageFound ? CheckSeverity.Pass : CheckSeverity.Info,
            CurrentValue = packageFound ? "Package available" : FailureMessage(runResult),
            ExpectedValue = $"winget package id: {packageId}",
            Impact = packageFound
                ? "The package ID is available from configured winget sources. This does not require installing or upgrading the tool."
                : "The package could not be confirmed from configured winget sources. Installation planning should stay manual until the package ID is verified.",
            CanFix = false,
            Risk = RiskLevel.None
        };
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

        string combined = string.Join(
            Environment.NewLine,
            runResult.StandardOutput,
            runResult.StandardError);

        string? firstLine = SplitLines(combined).FirstOrDefault();

        return firstLine ?? $"Exit code {runResult.ExitCode}";
    }

    private static string[] SplitLines(string value)
    {
        return value.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
