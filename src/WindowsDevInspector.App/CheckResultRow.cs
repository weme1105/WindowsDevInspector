using System.ComponentModel;
using WindowsDevInspector.Core;

namespace WindowsDevInspector.App;

public sealed class CheckResultRow(CheckResult result) : INotifyPropertyChanged
{
    private const int MaxCellLength = 160;
    private bool isSelectedForFix;

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

    public string RequiresElevationText { get; } = result.RequiresElevation ? "需要 UAC / 系統管理員權限" : "不需要提升權限";

    public bool RequiresRestart { get; } = result.RequiresRestart;

    public string RequiresRestartText { get; } = result.RequiresRestart ? "可能需要重開機" : "不需要重開機";

    public bool SupportsRollback { get; } = result.SupportsRollback;

    public string SupportsRollbackText { get; } = result.SupportsRollback ? "支援 Rollback" : "不支援 Rollback";

    public string? RemediationId { get; } = result.RemediationId;

    public string RemediationText { get; } = result.CanFix && result.RemediationId is not null
        ? result.RemediationId
        : "未提供自動修正";

    public string FixabilityText { get; } =
        $"可自動修正：{(result.CanFix ? "是" : "否")} | 風險：{result.Risk}";

    public string RemediationContextText { get; } = string.Join(" | ", [
        result.RequiresElevation ? "需要 UAC / 系統管理員權限" : "不需要提升權限",
        result.RequiresRestart ? "可能需要重開機" : "不需要重開機",
        result.SupportsRollback ? "支援 Rollback" : "不支援 Rollback"
    ]);

    public bool IsFixSelectable => CanFixValue
        && RemediationId is not null
        && Severity != CheckSeverity.Pass.ToString();

    public bool IsLowRiskLocalFix => IsFixSelectable
        && Risk == RiskLevel.Low
        && !RequiresElevation;

    public bool IsLowRiskSupportedFix => IsFixSelectable
        && Risk == RiskLevel.Low;

    public bool IsSelectedForFix
    {
        get => isSelectedForFix;
        set
        {
            bool normalizedValue = IsFixSelectable && value;
            if (isSelectedForFix == normalizedValue)
            {
                return;
            }

            isSelectedForFix = normalizedValue;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelectedForFix)));
        }
    }

    public string Impact { get; } = result.Impact;

    public event PropertyChangedEventHandler? PropertyChanged;

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
