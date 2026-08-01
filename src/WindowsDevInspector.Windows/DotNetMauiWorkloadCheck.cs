using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows;

public sealed class DotNetMauiWorkloadCheck(ICommandRunner commandRunner) : IEnvironmentCheck
{
    public string Id => "mobile.dotnet-maui";

    public async Task<CheckResult> RunAsync(CancellationToken cancellationToken)
    {
        CommandRunResult result = await commandRunner.RunAsync(
            "dotnet",
            "workload list",
            TimeSpan.FromSeconds(10),
            cancellationToken);

        bool found = result.Succeeded && ContainsMauiWorkload(result.StandardOutput);

        return new CheckResult
        {
            Id = Id,
            Category = "Mobile",
            Name = ".NET MAUI workload",
            Severity = found ? CheckSeverity.Pass : CheckSeverity.Info,
            CurrentValue = found ? FirstMauiLine(result.StandardOutput) : FailureMessage(result),
            ExpectedValue = "Installed .NET workload containing maui",
            Impact = found
                ? ".NET MAUI workload is listed by dotnet."
                : ".NET MAUI projects may not build until the workload is installed.",
            CanFix = false,
            Risk = RiskLevel.None
        };
    }

    private static bool ContainsMauiWorkload(string output)
    {
        return output
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(line => line.Contains("maui", StringComparison.OrdinalIgnoreCase));
    }

    private static string FirstMauiLine(string output)
    {
        return output
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .First(line => line.Contains("maui", StringComparison.OrdinalIgnoreCase));
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

        return "MAUI workload not listed";
    }
}
