namespace WindowsDevInspector.Core;

public static class CheckResultSorter
{
    public static IReadOnlyList<CheckResult> Sort(IEnumerable<CheckResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        return results
            .OrderBy(result => SeverityRank(result.Severity))
            .ThenBy(result => result.Category, StringComparer.OrdinalIgnoreCase)
            .ThenBy(result => result.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static int SeverityRank(CheckSeverity severity)
    {
        return severity switch
        {
            CheckSeverity.Critical => 0,
            CheckSeverity.Warning => 1,
            CheckSeverity.Info => 2,
            CheckSeverity.Pass => 3,
            _ => 4
        };
    }
}
