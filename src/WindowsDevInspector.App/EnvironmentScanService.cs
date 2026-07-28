using WindowsDevInspector.Core;
using WindowsDevInspector.Windows;

namespace WindowsDevInspector.App;

public sealed class EnvironmentScanService(
    CheckCatalog checkCatalog,
    IReadOnlyDictionary<string, IEnvironmentCheck> executableChecks)
{
    public async Task<EnvironmentScanResult> RunScanAsync(
        IReadOnlyCollection<string> selectedTechnologyIds,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<CheckDefinition> checks = checkCatalog.ResolveChecks(selectedTechnologyIds);

        List<CheckResult> scanResults = [];

        foreach (CheckDefinition check in checks)
        {
            CheckResult result = executableChecks.TryGetValue(check.Id, out IEnvironmentCheck? executableCheck)
                ? await executableCheck.RunAsync(cancellationToken)
                : CreatePendingCheckResult(check);

            scanResults.Add(result);
        }

        CheckResult[] sortedResults = CheckResultSorter.Sort(scanResults).ToArray();

        return new EnvironmentScanResult
        {
            Results = sortedResults,
            Score = EnvironmentScoreCalculator.Calculate(sortedResults)
        };
    }

    private static CheckResult CreatePendingCheckResult(CheckDefinition check)
    {
        return new CheckResult
        {
            Id = check.Id,
            Category = check.Category,
            Name = check.Name,
            Severity = CheckSeverity.Info,
            CurrentValue = "檢查尚未執行",
            ExpectedValue = check.SeverityWhenMissing == CheckSeverity.Warning
                ? "應可被偵測或設定正確"
                : "可選檢查或資訊收集",
            Impact = "目前已完成檢查清單解析；下一步會接上唯讀檢查實作。",
            CanFix = check.CanFix,
            Risk = check.Risk,
            RequiresElevation = check.RequiresElevation,
            RequiresRestart = check.RequiresRestart,
            SupportsRollback = check.SupportsRollback,
            RemediationId = check.RemediationId
        };
    }
}

