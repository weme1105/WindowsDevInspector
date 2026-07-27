using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows;

public sealed class FileExistsCheck(
    string id,
    string category,
    string name,
    IReadOnlyList<string> candidatePaths,
    string expectedValue) : IEnvironmentCheck
{
    public string Id => id;

    public Task<CheckResult> RunAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string? existingPath = candidatePaths.FirstOrDefault(path => File.Exists(path) || Directory.Exists(path));

        return Task.FromResult(new CheckResult
        {
            Id = id,
            Category = category,
            Name = name,
            Severity = existingPath is null ? CheckSeverity.Info : CheckSeverity.Pass,
            CurrentValue = existingPath ?? "Not found in known paths",
            ExpectedValue = expectedValue,
            Impact = existingPath is null
                ? "The tool was not found in the known install paths checked by this version."
                : "The tool was found in a known install path.",
            CanFix = false,
            Risk = RiskLevel.None
        });
    }
}
