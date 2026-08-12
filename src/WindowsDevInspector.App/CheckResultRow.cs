using System.ComponentModel;
using WindowsDevInspector.Core;

namespace WindowsDevInspector.App;

public sealed class CheckResultRow(CheckResult result, bool isSimulation = false) : INotifyPropertyChanged
{
    private const int MaxCellLength = 160;
    private bool isSelectedForFix;
    private bool isFixSelectionEnabled = true;
    private PackageInstallationCandidate? installationCandidate;
    private bool isSelectedForInstallation;
    private bool isInstallationSelectionEnabled;
    private string? actionBlockReason;

    public string Id { get; } = result.Id;

    public bool IsSimulation { get; } = isSimulation;

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

    public bool IsFixSelectionEnabled => IsFixSelectable && isFixSelectionEnabled && actionBlockReason is null;

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
            NotifyActionPropertiesChanged();
        }
    }

    public PackageInstallationCandidate? InstallationCandidate => installationCandidate;

    public bool IsInstallationCandidate => installationCandidate is not null;

    public bool IsInstallationSelectionEnabled => isInstallationSelectionEnabled && actionBlockReason is null;

    public bool IsSelectedForInstallation => isSelectedForInstallation;

    public bool IsActionSelectable => IsInstallationCandidate || IsFixSelectable;

    public bool IsActionSelectionEnabled => IsInstallationCandidate
        ? IsInstallationSelectionEnabled
        : IsFixSelectionEnabled;

    public bool IsSelectedForAction => IsSelectedForInstallation || IsSelectedForFix;

    public string ActionLabel => installationCandidate is not null ? "安裝" : "修正";

    public string ActionForeground => installationCandidate is not null || actionBlockReason is not null ? "#C62828" : "#162033";

    public string ActionToolTip => actionBlockReason ?? (installationCandidate is not null
        ? "套件安裝規劃：一次只能選擇一個；確認後其他操作會反灰。"
        : IsFixSelectable
            ? "一般 remediation 修正；套件安裝規劃啟用時會暫停此操作。"
            : string.Empty);

    public void SetActionBlockReason(string? reason)
    {
        actionBlockReason = string.IsNullOrWhiteSpace(reason) ? null : reason;
        if (actionBlockReason is not null)
        {
            isSelectedForFix = false;
            isSelectedForInstallation = false;
        }

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelectedForFix)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelectedForInstallation)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsFixSelectionEnabled)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsInstallationSelectionEnabled)));
        NotifyActionPropertiesChanged();
    }

    public string InstallationPlanningText => installationCandidate is null
        ? "Installation planning: unavailable"
        : $"Installation planning: {installationCandidate.DisplayName} [{installationCandidate.PackageId}] via {installationCandidate.Source}";

    public void SetInstallationCandidate(PackageInstallationCandidate? candidate)
    {
        installationCandidate = candidate;
        isSelectedForInstallation = false;
        isInstallationSelectionEnabled = candidate is not null;
        NotifyInstallationPropertiesChanged();
    }

    internal void SetInstallationSelectionState(bool isSelected, bool isEnabled)
    {
        isSelectedForInstallation = IsInstallationCandidate && isSelected;
        isInstallationSelectionEnabled = IsInstallationCandidate && isEnabled;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelectedForInstallation)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsInstallationSelectionEnabled)));
        NotifyActionPropertiesChanged();
    }

    internal void SetFixSelectionState(bool isSelected, bool isEnabled)
    {
        isSelectedForFix = IsFixSelectable && isSelected;
        isFixSelectionEnabled = isEnabled;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelectedForFix)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsFixSelectionEnabled)));
        NotifyActionPropertiesChanged();
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

    private void NotifyInstallationPropertiesChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InstallationCandidate)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsInstallationCandidate)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsInstallationSelectionEnabled)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelectedForInstallation)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InstallationPlanningText)));
        NotifyActionPropertiesChanged();
    }

    private void NotifyActionPropertiesChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsActionSelectable)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsActionSelectionEnabled)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelectedForAction)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActionToolTip)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActionLabel)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActionForeground)));
    }
}
