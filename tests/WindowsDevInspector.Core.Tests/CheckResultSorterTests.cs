using WindowsDevInspector.Core;

namespace WindowsDevInspector.Core.Tests;

public sealed class CheckResultSorterTests
{
    [Fact]
    public void Sort_PutsNonPassResultsBeforePassResults()
    {
        CheckResult[] results = [
            CreateResult("pass", CheckSeverity.Pass),
            CreateResult("warning", CheckSeverity.Warning),
            CreateResult("info", CheckSeverity.Info),
            CreateResult("critical", CheckSeverity.Critical)
        ];

        IReadOnlyList<CheckResult> sorted = CheckResultSorter.Sort(results);

        Assert.Equal(["critical", "warning", "info", "pass"], sorted.Select(result => result.Id));
    }

    private static CheckResult CreateResult(string id, CheckSeverity severity)
    {
        return new CheckResult
        {
            Id = id,
            Category = "General",
            Name = id,
            Severity = severity,
            CurrentValue = "current",
            ExpectedValue = "expected",
            Impact = "impact"
        };
    }
}
