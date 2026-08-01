using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows;

public sealed class DirectoryExistsCheck(
    string id,
    string category,
    string name,
    string path,
    IFileSystem fileSystem,
    string? remediationId = null) : IEnvironmentCheck
{
    public string Id => id;

    public Task<CheckResult> RunAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        bool exists = fileSystem.DirectoryExists(path);
        bool canFix = !exists && remediationId is not null;

        CheckResult result = new()
        {
            Id = id,
            Category = category,
            Name = name,
            Severity = exists ? CheckSeverity.Pass : CheckSeverity.Warning,
            CurrentValue = exists ? "Exists" : "Missing",
            ExpectedValue = path,
            Impact = exists
                ? "The expected development directory is available."
                : "The expected development directory is missing and may break repository or workspace conventions.",
            CanFix = canFix,
            Risk = canFix ? RiskLevel.Low : RiskLevel.None,
            RequiresElevation = false,
            RequiresRestart = false,
            SupportsRollback = canFix,
            RemediationId = exists ? null : remediationId
        };

        return Task.FromResult(result);
    }
}
