using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows;

public sealed class DotNetRuntimeCheck(
    string id,
    string category,
    string name,
    string runtimeName,
    ICommandRunner commandRunner,
    TimeSpan? timeout = null,
    int? minimumMajorVersion = null,
    string dotnetFileName = "dotnet") : IEnvironmentCheck
{
    private readonly TimeSpan timeout = timeout ?? TimeSpan.FromSeconds(8);

    public string Id => id;

    public async Task<CheckResult> RunAsync(CancellationToken cancellationToken)
    {
        CommandRunResult result = await commandRunner.RunAsync(
            dotnetFileName,
            "--list-runtimes",
            timeout,
            cancellationToken);

        string[] matchingRuntimes = result.Succeeded
            ? SplitLines(result.StandardOutput)
                .Where(line => line.StartsWith(runtimeName, StringComparison.OrdinalIgnoreCase))
                .Where(line => minimumMajorVersion is null || HasMinimumMajorVersion(line, minimumMajorVersion.Value))
                .ToArray()
            : [];

        bool found = matchingRuntimes.Length > 0;

        return new CheckResult
        {
            Id = id,
            Category = category,
            Name = name,
            Severity = found ? CheckSeverity.Pass : CheckSeverity.Info,
            CurrentValue = found ? string.Join(Environment.NewLine, matchingRuntimes) : FailureMessage(result),
            ExpectedValue = minimumMajorVersion is null
                ? $"{runtimeName} listed by dotnet --list-runtimes"
                : $"{runtimeName} {minimumMajorVersion}.x x64 listed by dotnet --list-runtimes",
            Impact = found
                ? "The runtime is installed and visible to the dotnet CLI."
                : "The runtime was not detected. Apps that require it may fail until the matching runtime or SDK workload is installed.",
            CanFix = false,
            Risk = RiskLevel.None
        };
    }

    private static bool HasMinimumMajorVersion(string line, int minimumMajorVersion)
    {
        string[] tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return tokens.Length >= 2
            && Version.TryParse(tokens[1], out Version? version)
            && version.Major >= minimumMajorVersion;
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
            string combined = string.Join(
                Environment.NewLine,
                result.StandardOutput,
                result.StandardError);

            return SplitLines(combined).FirstOrDefault() ?? $"Exit code {result.ExitCode}";
        }

        return "Runtime not found";
    }

    private static string[] SplitLines(string value)
    {
        return value.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
