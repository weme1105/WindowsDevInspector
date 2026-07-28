namespace WindowsDevInspector.Core.Tests;

public sealed class EnvironmentScoreCalculatorTests
{
    [Fact]
    public void Calculate_ReturnsFullScoreWhenAllChecksPass()
    {
        EnvironmentScore score = EnvironmentScoreCalculator.Calculate([
            CreateResult("pass-1", CheckSeverity.Pass),
            CreateResult("pass-2", CheckSeverity.Pass)
        ]);

        Assert.Equal(100, score.Score);
        Assert.Equal(2, score.TotalCount);
        Assert.Equal(2, score.PassCount);
    }

    [Fact]
    public void Calculate_AppliesSeverityWeightedPenalties()
    {
        EnvironmentScore score = EnvironmentScoreCalculator.Calculate([
            CreateResult("critical", CheckSeverity.Critical),
            CreateResult("warning", CheckSeverity.Warning),
            CreateResult("info", CheckSeverity.Info),
            CreateResult("pass", CheckSeverity.Pass)
        ]);

        Assert.Equal(42, score.Score);
        Assert.Equal(4, score.TotalCount);
        Assert.Equal(1, score.CriticalCount);
        Assert.Equal(1, score.WarningCount);
        Assert.Equal(1, score.InfoCount);
        Assert.Equal(1, score.PassCount);
    }

    [Fact]
    public void Calculate_NeverReturnsNegativeScore()
    {
        EnvironmentScore score = EnvironmentScoreCalculator.Calculate([
            CreateResult("critical-1", CheckSeverity.Critical),
            CreateResult("critical-2", CheckSeverity.Critical),
            CreateResult("critical-3", CheckSeverity.Critical)
        ]);

        Assert.Equal(0, score.Score);
    }

    private static CheckResult CreateResult(string id, CheckSeverity severity)
    {
        return new CheckResult
        {
            Id = id,
            Category = "Test",
            Name = id,
            Severity = severity,
            CurrentValue = "Current",
            ExpectedValue = "Expected",
            Impact = "Impact"
        };
    }
}
