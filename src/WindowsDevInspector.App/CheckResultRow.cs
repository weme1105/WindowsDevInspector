using WindowsDevInspector.Core;

namespace WindowsDevInspector.App;

public sealed class CheckResultRow(CheckResult result)
{
    private const int MaxCellLength = 160;

    public string Id { get; } = result.Id;

    public string Severity { get; } = result.Severity.ToString();

    public string Category { get; } = result.Category;

    public string Name { get; } = result.Name;

    public string CurrentValue { get; } = ToSingleLine(result.CurrentValue);

    public string FullCurrentValue { get; } = result.CurrentValue;

    public string ExpectedValue { get; } = result.ExpectedValue;

    public string CanFix { get; } = result.CanFix ? "是" : "否";

    public bool CanFixValue { get; } = result.CanFix;

    public RiskLevel Risk { get; } = result.Risk;

    public bool RequiresElevation { get; } = result.RequiresElevation;

    public bool RequiresRestart { get; } = result.RequiresRestart;

    public bool SupportsRollback { get; } = result.SupportsRollback;

    public string? RemediationId { get; } = result.RemediationId;

    public bool IsFixSelectable => CanFixValue
        && RemediationId is not null
        && Severity != CheckSeverity.Pass.ToString();

    public bool IsLowRiskLocalFix => IsFixSelectable
        && Risk == RiskLevel.Low
        && !RequiresElevation;

    public bool IsLowRiskSupportedFix => IsFixSelectable
        && Risk == RiskLevel.Low;

    public bool IsSelectedForFix { get; set; }

    public string Impact { get; } = result.Impact;

    public CheckResultReportRow ToReportRow()
    {
        return new CheckResultReportRow
        {
            Id = Id,
            Severity = Severity,
            Category = Category,
            Name = Name,
            CurrentValue = FullCurrentValue,
            ExpectedValue = ExpectedValue,
            Impact = Impact,
            CanFix = CanFixValue,
            RemediationId = RemediationId
        };
    }

    private static string ToSingleLine(string value)
    {
        string singleLine = value
            .Replace("\r", " ", StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal)
            .Trim();

        return singleLine.Length <= MaxCellLength
            ? singleLine
            : string.Concat(singleLine.AsSpan(0, MaxCellLength), "...");
    }
}
