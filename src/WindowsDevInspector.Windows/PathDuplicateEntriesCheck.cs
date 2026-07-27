using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows;

public sealed class PathDuplicateEntriesCheck(IEnvironmentVariableReader environmentReader) : IEnvironmentCheck
{
    public string Id => "common.path-duplicate-entries";

    public Task<CheckResult> RunAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string[] duplicates = PathInvalidEntriesCheck
            .ReadPathEntries(environmentReader)
            .GroupBy(NormalizePath, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.First())
            .ToArray();

        return Task.FromResult(new CheckResult
        {
            Id = Id,
            Category = "Common",
            Name = "PATH duplicate entries",
            Severity = duplicates.Length == 0 ? CheckSeverity.Pass : CheckSeverity.Info,
            CurrentValue = duplicates.Length == 0 ? "No duplicate entries" : string.Join("; ", duplicates.Take(5)),
            ExpectedValue = "PATH entries are unique",
            Impact = duplicates.Length == 0
                ? "PATH does not contain duplicate entries."
                : "Duplicate PATH entries are usually harmless but make environment troubleshooting noisier.",
            CanFix = false,
            Risk = RiskLevel.None
        });
    }

    private static string NormalizePath(string path)
    {
        return path.Trim().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }
}
