using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows;

public sealed class VisualStudioBuildToolsCheck(ICommandRunner commandRunner, IFileSystem fileSystem) : IEnvironmentCheck
{
    private const string VsWherePath = "C:\\Program Files (x86)\\Microsoft Visual Studio\\Installer\\vswhere.exe";

    public string Id => "backend.visualstudio-buildtools";

    public async Task<CheckResult> RunAsync(CancellationToken cancellationToken)
    {
        if (!fileSystem.FileExists(VsWherePath))
        {
            return Result(CheckSeverity.Info, "vswhere.exe not found", VsWherePath, "Visual Studio Installer was not found in the standard location.");
        }

        CommandRunResult result = await commandRunner.RunAsync(
            VsWherePath,
            "-products * -requires Microsoft.Component.MSBuild -property displayName",
            TimeSpan.FromSeconds(8),
            cancellationToken);

        bool found = result.Succeeded && !string.IsNullOrWhiteSpace(result.StandardOutput);

        return Result(
            found ? CheckSeverity.Pass : CheckSeverity.Info,
            found ? result.StandardOutput.Trim() : FailureMessage(result),
            "Visual Studio or Build Tools instance with MSBuild component",
            found
                ? "Visual Studio or Build Tools was detected through vswhere."
                : "Visual Studio Build Tools was not detected through vswhere.");
    }

    private static CheckResult Result(CheckSeverity severity, string currentValue, string expectedValue, string impact)
    {
        return new CheckResult
        {
            Id = "backend.visualstudio-buildtools",
            Category = "Backend",
            Name = "Visual Studio / Build Tools",
            Severity = severity,
            CurrentValue = currentValue,
            ExpectedValue = expectedValue,
            Impact = impact,
            CanFix = false,
            Risk = RiskLevel.None
        };
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
