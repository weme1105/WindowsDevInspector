using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WindowsDevInspector.Core;
using WindowsDevInspector.Remediation;
using WindowsDevInspector.Windows;

namespace WindowsDevInspector.App;

public partial class MainWindow : Window
{
    private readonly EnvironmentScanService scanService = new(
        BuiltInCheckCatalog.Create(),
        BuiltInEnvironmentCheckFactory.CreateAll());
    private readonly RemediationCoordinator remediationCoordinator = new();
    private readonly ScanReportExporter scanReportExporter = new();

    public MainWindow()
    {
        InitializeComponent();

        TechnologyGroups = new ObservableCollection<TechnologyGroup>(TechnologyCatalog.CreateDefaultGroups());
        AttachTechnologySelectionChangeHandlers();
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
        RefreshBackupFiles();

        int loadedTechnologyCount = TechnologySelectionConfig.Load(TechnologyGroups);
        if (loadedTechnologyCount > 0)
        {
            ScanStatusTextBlock.Text = $"已載入 {loadedTechnologyCount} 個儲存選項";
        }

        RefreshTechnologySelectionToggleButton();
        ResultsView.Filter = ShouldShowResult;
    }

    public ObservableCollection<TechnologyGroup> TechnologyGroups { get; }

    public ObservableCollection<CheckResultRow> Results { get; }

    public ObservableCollection<BackupFileRow> BackupFiles { get; } = [];

    public ObservableCollection<int> ScanConcurrencyOptions { get; } = new(ScanConcurrencySettings.CreateOptions());

    public int SelectedScanConcurrency { get; set; } = ScanConcurrencySettings.DefaultMaxConcurrentChecks;

    private ICollectionView ResultsView => CollectionViewSource.GetDefaultView(Results);

    private void TechnologySearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        string query = TechnologySearchTextBox.Text.Trim();

        foreach (TechnologyGroup group in TechnologyGroups)
        {
            group.ApplySearch(query);
        }
    }

    private void HidePassResultsCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        ResultsView.Refresh();
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

    private void ToggleTechnologySelectionButton_Click(object sender, RoutedEventArgs e)
    {
        TechnologySelectionToggleResult result = TechnologySelectionToggle.Toggle(TechnologyGroups);
        RefreshTechnologySelectionToggleButton();

        ScanStatusTextBlock.Text = result.Action == TechnologySelectionToggleAction.SelectedAll
            ? $"已勾選全部 {result.ChangedCount} 個技術；按「儲存選項」可保留此選擇"
            : $"已取消全部 {result.ChangedCount} 個技術勾選；按「儲存選項」可保留此選擇";
    }

    private void RefreshTechnologySelectionToggleButton()
    {
        ToggleTechnologySelectionButton.Content = TechnologySelectionToggle.GetButtonLabel(TechnologyGroups);
    }

    private void AttachTechnologySelectionChangeHandlers()
    {
        foreach (TechnologyItem technology in TechnologyGroups.SelectMany(group => group.Technologies))
        {
            technology.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(TechnologyItem.IsSelected))
                {
                    RefreshTechnologySelectionToggleButton();
                }
            };
        }
    }

    private async Task RunScanAsync()
    {
        string[] selectedTechnologyIds = GetSelectedTechnologyIds();

        int scanConcurrency = ScanConcurrencySettings.Clamp(SelectedScanConcurrency);
        EnvironmentScanResult scanResult = await scanService.RunScanAsync(
            selectedTechnologyIds,
            CancellationToken.None,
            scanConcurrency);

        Results.Clear();
        foreach (CheckResult result in scanResult.Results)
        {
            Results.Add(new CheckResultRow(result));
        }

        ScoreTextBlock.Text = FormatScore(scanResult.Score);

        ScanStatusTextBlock.Text = $"已掃描 {Results.Count} 個檢查，併發數 {scanConcurrency}";
    }
    private void FixAllButton_Click(object sender, RoutedEventArgs e)
    {
        int selectedCount = ResultFixSelection.SelectLowRiskSupportedFixes(Results);

        ScanStatusTextBlock.Text = selectedCount == 0
            ? "沒有低風險且已支援的修正項目可勾選"
            : $"已批次勾選 {selectedCount} 個低風險且已支援的修正項目";
    }

    private async void StartFixButton_Click(object sender, RoutedEventArgs e)
    {
        CheckResultRow[] selectedRows = ResultFixSelection.GetSelectedFixes(Results);

        await ExecuteFixesAsync(selectedRows, "尚未勾選可執行的修正");
    }

    private async void RestoreLatestBackupButton_Click(object sender, RoutedEventArgs e)
    {
        string? backupPath = BackupComboBox.SelectedItem is BackupFileRow selectedBackup
            ? selectedBackup.FullPath
            : remediationCoordinator.FindLatestBackupPath();
        if (backupPath is null)
        {
            ScanStatusTextBlock.Text = "找不到可還原的備份檔";
            return;
        }

        if (!ConfirmRollback(backupPath))
        {
            ScanStatusTextBlock.Text = "已取消還原，系統未變更";
            return;
        }

        SetRemediationButtonsEnabled(false);
        ScanStatusTextBlock.Text = "還原中...";

        try
        {
            WorkerExecutionResult rollbackResult = await remediationCoordinator.ExecuteRollbackAsync(backupPath);
            await RunScanAsync();
            RefreshBackupFiles();

            ScanStatusTextBlock.Text = rollbackResult.Succeeded
                ? "還原完成，已重新掃描"
                : $"還原失敗：{string.Join("; ", rollbackResult.Results.Select(result => result.Message).Concat(rollbackResult.Errors))}";
        }
        catch (OperationCanceledException)
        {
            ScanStatusTextBlock.Text = "還原已取消或 UAC 未被允許";
        }
        catch (IOException ex)
        {
            ScanStatusTextBlock.Text = $"還原失敗：{ex.Message}";
        }
        finally
        {
            SetRemediationButtonsEnabled(true);
        }
    }

    private async void ExportReportButton_Click(object sender, RoutedEventArgs e)
    {
        if (Results.Count == 0)
        {
            ScanStatusTextBlock.Text = "沒有可匯出的檢查結果";
            return;
        }

        try
        {
            string reportPath = await scanReportExporter.ExportAsync(
                GetSelectedTechnologyIds(),
                Results,
                CancellationToken.None);
            ScanStatusTextBlock.Text = $"已匯出報告：{reportPath}";
        }
        catch (IOException ex)
        {
            ScanStatusTextBlock.Text = $"匯出失敗：{ex.Message}";
        }
        catch (UnauthorizedAccessException)
        {
            ScanStatusTextBlock.Text = "匯出失敗：沒有權限寫入報告資料夾";
        }
    }

    private async Task ExecuteFixesAsync(IReadOnlyList<CheckResultRow> selectedRows, string emptySelectionMessage)
    {
        if (selectedRows.Count == 0)
        {
            ScanStatusTextBlock.Text = emptySelectionMessage;
            return;
        }

        if (!ConfirmRemediation(selectedRows))
        {
            ScanStatusTextBlock.Text = "已取消修正，系統未變更";
            return;
        }

        SetRemediationButtonsEnabled(false);
        ScanStatusTextBlock.Text = "修正中...";

        try
        {
            List<RemediationExecutionResult> executionResults = [];
            IReadOnlyList<RemediationExecutionResult> localResults =
                await remediationCoordinator.ExecuteLocalRemediationsAsync(
                    selectedRows
                        .Where(row => !row.RequiresElevation)
                        .Select(row => row.RemediationId!),
                    CancellationToken.None);
            executionResults.AddRange(localResults);

            CheckResultRow[] elevatedRows = selectedRows
                .Where(row => row.RequiresElevation)
                .ToArray();

            WorkerExecutionResult? workerResult = elevatedRows.Length == 0
                ? null
                : await remediationCoordinator.ExecuteElevatedRemediationAsync(
                    elevatedRows.Select(row => row.RemediationId!).ToArray());
            RefreshBackupFiles();

            if (workerResult is not null)
            {
                executionResults.AddRange(workerResult.Results);
            }

            int succeededCount = executionResults.Count(result => result.Succeeded);
            int skippedCount = executionResults.Count(result => result.Skipped);
            await RunScanAsync();

            ScanStatusTextBlock.Text = workerResult?.Errors.Count > 0
                ? $"修正完成但 Worker 回報錯誤：{string.Join("; ", workerResult.Errors)}"
                : skippedCount == 0
                ? $"修正完成：{succeededCount} 個成功，已重新掃描"
                : $"修正完成：{succeededCount} 個成功，{skippedCount} 個略過，已重新掃描";
        }
        catch (OperationCanceledException)
        {
            ScanStatusTextBlock.Text = "修正已取消或 UAC 未被允許";
        }
        catch (UnauthorizedAccessException)
        {
            ScanStatusTextBlock.Text = "修正失敗：沒有權限建立目標資料夾";
        }
        catch (IOException ex)
        {
            ScanStatusTextBlock.Text = $"修正失敗：{ex.Message}";
        }
        finally
        {
            SetRemediationButtonsEnabled(true);
        }
    }

    private void RefreshBackupFiles()
    {
        BackupFiles.Clear();

        foreach (BackupFileRow file in remediationCoordinator.GetBackupFiles())
        {
            BackupFiles.Add(file);
        }

        BackupComboBox.SelectedIndex = BackupFiles.Count > 0 ? 0 : -1;
        RestoreLatestBackupButton.IsEnabled = BackupFiles.Count > 0;
    }

    private void SetRemediationButtonsEnabled(bool isEnabled)
    {
        StartFixButton.IsEnabled = isEnabled;
        FixAllButton.IsEnabled = isEnabled;
        RestoreLatestBackupButton.IsEnabled = isEnabled && BackupFiles.Count > 0;
    }

    private string[] GetSelectedTechnologyIds()
    {
        return TechnologyGroups
            .SelectMany(group => group.Technologies)
            .Where(technology => technology.IsSelected)
            .GroupBy(technology => technology.Id, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.Key)
            .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string FormatScore(EnvironmentScore score)
    {
        return $"Score {score.Score}/100 - Critical {score.CriticalCount}, Warning {score.WarningCount}, Info {score.InfoCount}, Pass {score.PassCount}";
    }

    private bool ShouldShowResult(object item)
    {
        return item is not CheckResultRow result
            || HidePassResultsCheckBox.IsChecked != true
            || !result.Severity.Equals(CheckSeverity.Pass.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private static bool ConfirmRemediation(IReadOnlyList<CheckResultRow> selectedRows)
    {
        int elevatedCount = selectedRows.Count(row => row.RequiresElevation);
        int restartCount = selectedRows.Count(row => row.RequiresRestart);
        int rollbackCount = selectedRows.Count(row => row.SupportsRollback);

        string message = string.Join(Environment.NewLine, [
            $"即將執行 {selectedRows.Count} 個已勾選修正項目。",
            $"需要 UAC / 系統管理員權限：{elevatedCount} 個。",
            $"需要重開機：{restartCount} 個。",
            $"支援 Rollback：{rollbackCount} 個。",
            "Registry 修正會由 ElevatedWorker 建立備份後執行；本機資料夾修正僅建立白名單目錄。",
            "所有修正都會先經過 remediation whitelist；不會執行任意 PowerShell、cmd 或未核准的 Registry/PATH/Windows Feature 變更。",
            "執行後會重新掃描目前狀態。",
            "",
            "是否繼續？"
        ]);

        return MessageBox.Show(
            message,
            "確認開始修正",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning) == MessageBoxResult.Yes;
    }

    private static bool ConfirmRollback(string backupPath)
    {
        string message = string.Join(Environment.NewLine, [
            "即將以 ElevatedWorker 執行 Registry Rollback。",
            $"備份檔：{backupPath}",
            "此操作需要 UAC / 系統管理員權限，並會由 worker 驗證備份內容後才還原。",
            "如果備份無法解密、格式不正確，或不是核准的 Registry 目標，worker 會拒絕還原。",
            "還原後會重新掃描目前狀態。",
            "",
            "是否繼續？"
        ]);

        return MessageBox.Show(
            message,
            "確認還原備份",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning) == MessageBoxResult.Yes;
    }
}
