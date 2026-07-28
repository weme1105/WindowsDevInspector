namespace WindowsDevInspector.Core;

public static class EnvironmentScoreCalculator
{
    public static EnvironmentScore Calculate(IEnumerable<CheckResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        CheckResult[] materializedResults = results.ToArray();

        int criticalCount = materializedResults.Count(result => result.Severity == CheckSeverity.Critical);
        int warningCount = materializedResults.Count(result => result.Severity == CheckSeverity.Warning);
        int infoCount = materializedResults.Count(result => result.Severity == CheckSeverity.Info);
        int passCount = materializedResults.Count(result => result.Severity == CheckSeverity.Pass);

        int penalty = (criticalCount * 40) + (warningCount * 15) + (infoCount * 3);

        return new EnvironmentScore
        {
            Score = Math.Max(0, 100 - penalty),
            TotalCount = materializedResults.Length,
            CriticalCount = criticalCount,
            WarningCount = warningCount,
            InfoCount = infoCount,
            PassCount = passCount
        };
    }
}
