using WindowsDevInspector.App;
using WindowsDevInspector.Core;

namespace WindowsDevInspector.App.Tests;

public sealed class ResultFixSelectionTests
{
    [Fact]
    public void SelectLowRiskSupportedFixes_SelectsOnlyLowRiskSupportedNonPassResults()
    {
        CheckResultRow pass = Row(CheckSeverity.Pass, canFix: true, RiskLevel.Low, "pass-fix");
        CheckResultRow unsupported = Row(CheckSeverity.Warning, canFix: false, RiskLevel.Low, null);
        CheckResultRow highRisk = Row(CheckSeverity.Warning, canFix: true, RiskLevel.High, "high-risk-fix");
        CheckResultRow lowRisk = Row(CheckSeverity.Warning, canFix: true, RiskLevel.Low, "low-risk-fix");

        int selectedCount = ResultFixSelection.SelectLowRiskSupportedFixes([pass, unsupported, highRisk, lowRisk]);

        Assert.Equal(1, selectedCount);
        Assert.False(pass.IsSelectedForFix);
        Assert.False(unsupported.IsSelectedForFix);
        Assert.False(highRisk.IsSelectedForFix);
        Assert.True(lowRisk.IsSelectedForFix);
    }

    [Fact]
    public void CheckResultRow_DoesNotAllowUnsupportedResultToBeSelectedForFix()
    {
        CheckResultRow row = Row(CheckSeverity.Warning, canFix: false, RiskLevel.Low, null);

        row.IsSelectedForFix = true;

        Assert.False(row.IsSelectedForFix);
    }

    [Fact]
    public void GetSelectedFixes_ReturnsOnlySelectableRowsWithRemediationIds()
    {
        CheckResultRow unsupported = Row(CheckSeverity.Warning, canFix: false, RiskLevel.Low, null);
        CheckResultRow selected = Row(CheckSeverity.Warning, canFix: true, RiskLevel.Low, "create-source-directory");
        unsupported.IsSelectedForFix = true;
        selected.IsSelectedForFix = true;

        CheckResultRow[] selectedRows = ResultFixSelection.GetSelectedFixes([unsupported, selected]);

        CheckResultRow row = Assert.Single(selectedRows);
        Assert.Equal("create-source-directory", row.RemediationId);
    }

    [Fact]
    public void GetSelectedFixes_ExcludesDebugSimulationRows()
    {
        CheckResultRow simulation = new(
            new CheckResult
            {
                Id = "debug.remediation-preview",
                Category = "DEBUG",
                Name = "Debug remediation",
                Severity = CheckSeverity.Warning,
                CurrentValue = "Simulation",
                ExpectedValue = "Simulation only",
                Impact = "Must not execute",
                CanFix = true,
                Risk = RiskLevel.Low,
                RemediationId = "create-source-directory"
            },
            isSimulation: true);
        simulation.IsSelectedForFix = true;

        CheckResultRow[] selectedRows = ResultFixSelection.GetSelectedFixes([simulation]);

        Assert.True(simulation.IsSelectedForFix);
        Assert.Empty(selectedRows);
    }

    private static CheckResultRow Row(
        CheckSeverity severity,
        bool canFix,
        RiskLevel risk,
        string? remediationId)
    {
        return new CheckResultRow(new CheckResult
        {
            Id = remediationId ?? "check-id",
            Category = "Common",
            Name = "Test check",
            Severity = severity,
            CurrentValue = "Current",
            ExpectedValue = "Expected",
            Impact = "Impact",
            CanFix = canFix,
            Risk = risk,
            RemediationId = remediationId
        });
    }
}
