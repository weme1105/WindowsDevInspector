using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows;

public sealed class PathInvalidEntriesCheck(IEnvironmentVariableReader environmentReader) : IEnvironmentCheck
{
    public string Id => "common.path-invalid-entries";

    public Task<CheckResult> RunAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string[] entries = ReadPathEntries(environmentReader);
        string[] invalidEntries = entries
            .Where(entry => !Directory.Exists(entry) && !File.Exists(entry))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return Task.FromResult(new CheckResult
        {
            Id = Id,
            Category = "Common",
            Name = "PATH invalid entries",
            Severity = invalidEntries.Length == 0 ? CheckSeverity.Pass : CheckSeverity.Warning,
            CurrentValue = invalidEntries.Length == 0 ? "No invalid entries" : string.Join("; ", invalidEntries.Take(5)),
            ExpectedValue = "All PATH entries point to existing files or directories",
            Impact = invalidEntries.Length == 0
                ? "PATH entries look valid."
                : "Invalid PATH entries can slow command lookup and make tool resolution harder to diagnose.",
            CanFix = false,
            Risk = RiskLevel.None
        });
    }

    internal static string[] ReadPathEntries(IEnvironmentVariableReader environmentReader)
    {
        string combined = string.Join(
            Path.PathSeparator,
            environmentReader.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine),
            environmentReader.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User),
            environmentReader.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process));

        return combined
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(entry => !string.IsNullOrWhiteSpace(entry))
            .ToArray();
    }
}
