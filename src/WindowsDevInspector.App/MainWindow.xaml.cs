using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using WindowsDevInspector.Core;
using WindowsDevInspector.Windows;

namespace WindowsDevInspector.App;

public partial class MainWindow : Window
{
    private readonly CheckCatalog checkCatalog = BuiltInCheckCatalog.Create();

    public MainWindow()
    {
        InitializeComponent();

        TechnologyGroups = new ObservableCollection<TechnologyGroup>(TechnologyCatalog.CreateDefaultGroups());
        Results = new ObservableCollection<CheckResultRow>
        {
            new(new CheckResult
            {
                Id = "system.catalog-ready",
                Category = "System",
                Name = "技術目錄已更新",
                Severity = CheckSeverity.Info,
                CurrentValue = "等待接上掃描器",
                ExpectedValue = "選擇技術後開始檢查",
                Impact = "目前已可由技術 ID 解析去重後的檢查清單。"
            })
        };

        DataContext = this;

        int loadedTechnologyCount = TechnologySelectionConfig.Load(TechnologyGroups);
        if (loadedTechnologyCount > 0)
        {
            ScanStatusTextBlock.Text = $"已載入 {loadedTechnologyCount} 個儲存選項";
        }
    }

    public ObservableCollection<TechnologyGroup> TechnologyGroups { get; }

    public ObservableCollection<CheckResultRow> Results { get; }

    private void TechnologySearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        string query = TechnologySearchTextBox.Text.Trim();

        foreach (TechnologyGroup group in TechnologyGroups)
        {
            group.ApplySearch(query);
        }
    }

    private async void StartScanButton_Click(object sender, RoutedEventArgs e)
    {
        StartScanButton.IsEnabled = false;
        ScanStatusTextBlock.Text = "掃描中...";

        try
        {
            await RunScanAsync();
        }
        finally
        {
            StartScanButton.IsEnabled = true;
        }
    }

    private void SaveOptionsButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            int savedTechnologyCount = TechnologySelectionConfig.Save(TechnologyGroups);
            ScanStatusTextBlock.Text = $"已儲存 {savedTechnologyCount} 個選項到 {TechnologySelectionConfig.FileName}";
        }
        catch (UnauthorizedAccessException)
        {
            ScanStatusTextBlock.Text = "儲存失敗：沒有權限寫入 exe 資料夾";
        }
        catch (IOException ex)
        {
            ScanStatusTextBlock.Text = $"儲存失敗：{ex.Message}";
        }
    }

    private async Task RunScanAsync()
    {
        string[] selectedTechnologyIds = TechnologyGroups
            .SelectMany(group => group.Technologies)
            .Where(technology => technology.IsSelected)
            .Select(technology => technology.Id)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        IReadOnlyList<CheckDefinition> checks = checkCatalog.ResolveChecks(selectedTechnologyIds);
        IReadOnlyDictionary<string, IEnvironmentCheck> executableChecks = BuiltInEnvironmentCheckFactory.CreateAll();

        Results.Clear();

        foreach (CheckDefinition check in checks)
        {
            CheckResult result = executableChecks.TryGetValue(check.Id, out IEnvironmentCheck? executableCheck)
                ? await executableCheck.RunAsync(CancellationToken.None)
                : CreatePendingCheckResult(check);

            Results.Add(new CheckResultRow(result));
        }

        ScanStatusTextBlock.Text = $"已掃描 {Results.Count} 個檢查";
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
