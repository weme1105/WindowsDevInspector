using System.Text.RegularExpressions;
using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows;

public sealed partial class NuGetSourcesCheck(ICommandRunner commandRunner) : IEnvironmentCheck
{
    public string Id => "backend.nuget-sources";

    public async Task<CheckResult> RunAsync(CancellationToken cancellationToken)
    {
        CommandRunResult result = await commandRunner.RunAsync(
            "dotnet",
            "nuget list source",
            TimeSpan.FromSeconds(8),
            cancellationToken);

        bool found = result.Succeeded && !string.IsNullOrWhiteSpace(result.StandardOutput);

        return new CheckResult
        {
            Id = Id,
            Category = "Backend",
            Name = "NuGet sources",
            Severity = found ? CheckSeverity.Pass : CheckSeverity.Info,
            CurrentValue = found ? Sanitize(result.StandardOutput) : FailureMessage(result),
            ExpectedValue = "Readable NuGet source list with credentials redacted",
            Impact = found
                ? "NuGet package sources are readable. Sensitive URL parts are redacted from the result."
                : "NuGet sources could not be listed. Restore may fail if no package source is configured.",
            CanFix = false,
            Risk = RiskLevel.None
        };
    }

    internal static string Sanitize(string value)
    {
        string sanitized = CredentialInUrlRegex().Replace(value, "${scheme}://<redacted>@");
        sanitized = SensitiveKeyValueRegex().Replace(sanitized, "${key}=<redacted>");
        return sanitized.Trim();
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
            : Sanitize(result.StandardError);
    }

    [GeneratedRegex("(?<scheme>https?)://[^\\s/@]+:[^\\s/@]+@", RegexOptions.IgnoreCase)]
    private static partial Regex CredentialInUrlRegex();

    [GeneratedRegex("(?<key>password|passwd|pwd|token|apikey|api_key|access_token)\\s*=\\s*[^\\s;]+", RegexOptions.IgnoreCase)]
    private static partial Regex SensitiveKeyValueRegex();
}
