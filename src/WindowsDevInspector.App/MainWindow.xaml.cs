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
    private readonly ApprovedInstallationCatalog installationCatalog = BuiltInInstallationCatalog.Create();
    private readonly PackageInstallationCoordinator packageInstallationCoordinator = new();
    private PackageInstallationConfirmation? selectedInstallationConfirmation;

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
#if DEBUG
        AddDebugActionRows();
#endif

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
        selectedInstallationConfirmation = null;
        StartInstallationButton.IsEnabled = false;
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

        ApplyInstallationCandidates();
#if DEBUG
        AddDebugActionRows();
#endif
        bool runtimeReady = RuntimePrerequisiteSelection.Apply(Results);
        SetRemediationButtonsEnabled(runtimeReady);

        ScoreTextBlock.Text = FormatScore(scanResult.Score);
        ScanStatusTextBlock.Foreground = runtimeReady
            ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x22, 0x51, 0xA4))
            : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xC6, 0x28, 0x28));
        ScanStatusTextBlock.Text = runtimeReady
            ? $"已掃描 {Results.Count} 個檢查，併發數 {scanConcurrency}"
            : $"已掃描 {Results.Count} 個檢查；{RuntimePrerequisiteSelection.BlockReason}";
    }
    private void FixAllButton_Click(object sender, RoutedEventArgs e)
    {
        int selectedCount = ResultFixSelection.SelectLowRiskSupportedFixes(Results);

        ScanStatusTextBlock.Text = selectedCount == 0
            ? "沒有低風險且已支援的修正項目可勾選"
            : $"已批次勾選 {selectedCount} 個低風險且已支援的修正項目";
    }

    private void ResultActionCheckBox_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not CheckBox checkBox
            || checkBox.DataContext is not CheckResultRow result)
        {
            return;
        }

        if (result.InstallationCandidate is not PackageInstallationCandidate candidate)
        {
            result.IsSelectedForFix = checkBox.IsChecked == true;
            return;
        }

        if (result.IsSelectedForInstallation)
        {
            PackageInstallationSelection.Clear(Results);
            selectedInstallationConfirmation = null;
            StartInstallationButton.IsEnabled = false;
            SetRemediationButtonsEnabled(true);
            ScanStatusTextBlock.Text = "已取消安裝規劃選取；系統未變更";
            return;
        }

        PackageInstallationConfirmationBuildResult previewResult =
            packageInstallationCoordinator.CreatePreview([candidate.ToPlanItem()]);
        if (!previewResult.IsValid || previewResult.Confirmation is null)
        {
            PackageInstallationSelection.Clear(Results);
            selectedInstallationConfirmation = null;
            StartInstallationButton.IsEnabled = false;
            SetRemediationButtonsEnabled(true);
            ScanStatusTextBlock.Text = $"無法建立安裝規劃：{string.Join("; ", previewResult.Errors)}";
            return;
        }

        string warningMessage = string.Join(Environment.NewLine,
        [
            previewResult.Confirmation.Message,
            "",
            "安全提醒：套件安裝可能修改 Program Files、PATH、shims 或其他系統設定，未來執行時可能要求 UAC。",
            "確認後會清除既有 remediation 勾選，並暫停其他 remediation 與安裝規劃操作。",
            "目前只會保留單一選取並顯示預覽，不會下載、安裝或修改系統。",
            "",
            "是否將此套件加入目前的安裝規劃預覽？"
        ]);

        bool accepted = MessageBox.Show(
            warningMessage,
            "套件安裝規劃安全提醒",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning) == MessageBoxResult.Yes;

        if (!accepted)
        {
            PackageInstallationSelection.Clear(Results);
            selectedInstallationConfirmation = null;
            StartInstallationButton.IsEnabled = false;
            SetRemediationButtonsEnabled(true);
            ScanStatusTextBlock.Text = "已取消安裝規劃選取；系統未變更";
            return;
        }

        PackageInstallationSelection.SelectSingle(Results, result);
        selectedInstallationConfirmation = previewResult.Confirmation;
        StartInstallationButton.IsEnabled = !result.IsSimulation;
        SetRemediationButtonsEnabled(false);
        ResultsListView.SelectedItem = result;
        ScanStatusTextBlock.Text = result.IsSimulation
            ? $"已選擇 {candidate.DisplayName} 的 Debug 預覽；禁止實際執行"
            : $"已選擇 {candidate.DisplayName}；其他操作已停用，尚未開始安裝";
    }

    private async void StartInstallationButton_Click(object sender, RoutedEventArgs e)
    {
        if (selectedInstallationConfirmation is not PackageInstallationConfirmation confirmation)
        {
            ScanStatusTextBlock.Text = "尚未選擇有效的套件安裝項目";
            return;
        }

        CheckResultRow? selectedRow = Results.SingleOrDefault(row => row.IsSelectedForInstallation);
        if (selectedRow is null || selectedRow.IsSimulation)
        {
            StartInstallationButton.IsEnabled = false;
            ScanStatusTextBlock.Text = "Debug 模擬項目禁止執行套件安裝";
            return;
        }

        string message = string.Join(Environment.NewLine,
        [
            confirmation.Message,
            "",
            "安全提醒：這會啟動 UAC，並由 ElevatedWorker 執行上方唯一一條固定 winget 命令。",
            "安裝可能修改 Program Files、PATH、shims 或其他系統設定。",
            "不會使用 silent、override、強制版本或強制 scope；逾時為 5 分鐘。",
            "失敗時不會自動解除安裝或回復套件造成的變更。",
            "",
            "是否確定開始安裝？"
        ]);

        if (MessageBox.Show(
            message,
            "確認執行套件安裝",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning) != MessageBoxResult.Yes)
        {
            ScanStatusTextBlock.Text = "已取消套件安裝；系統未變更";
            return;
        }

        StartInstallationButton.IsEnabled = false;
        ScanStatusTextBlock.Text = "等待 UAC 並執行套件安裝中...";

        try
        {
            PackageInstallationExecutionResult result = await packageInstallationCoordinator
                .ExecuteConfirmedAsync(confirmation);
            await RunScanAsync();
            selectedInstallationConfirmation = null;

            ScanStatusTextBlock.Text = result.Succeeded
                ? "套件安裝完成，已重新掃描"
                : $"套件安裝未完成：{string.Join("; ", result.Errors.Concat(result.Results.Select(item => item.Message)))}";
        }
        catch (OperationCanceledException)
        {
            ScanStatusTextBlock.Text = "套件安裝已取消或 UAC 未被允許";
        }
        catch (IOException ex)
        {
            ScanStatusTextBlock.Text = $"套件安裝失敗：{ex.Message}";
        }
        finally
        {
            SetRemediationButtonsEnabled(true);
        }
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

    private void ApplyInstallationCandidates()
    {
        PackageInstallationCandidateSelector selector = new(installationCatalog);
        IReadOnlyDictionary<string, PackageInstallationCandidate> candidates = selector
            .GetCandidates(Results)
            .ToDictionary(candidate => candidate.DiagnosticCheckId, StringComparer.OrdinalIgnoreCase);

        foreach (CheckResultRow result in Results)
        {
            result.SetInstallationCandidate(
                candidates.TryGetValue(result.Id, out PackageInstallationCandidate? candidate)
                    ? candidate
                    : null);
        }
    }

#if DEBUG
    private void AddDebugActionRows()
    {
        Results.Add(new CheckResultRow(
            new CheckResult
            {
                Id = "debug.remediation-preview",
                Category = "DEBUG",
                Name = "[DEBUG] 模擬修正",
                Severity = CheckSeverity.Warning,
                CurrentValue = "模擬目前狀態；不會執行真實 remediation",
                ExpectedValue = "驗證修正 checkbox、勾選與反灰行為",
                Impact = "僅供 Debug UI 測試；執行選取時會被安全排除。",
                CanFix = true,
                Risk = RiskLevel.Low,
                RequiresElevation = false,
                RequiresRestart = false,
                SupportsRollback = false,
                RemediationId = "create-source-directory"
            },
            isSimulation: true));

        ApprovedInstallationPackage package = installationCatalog.GetRequired(
            "Hashicorp.Terraform",
            InstallationSource.Winget);
        CheckResultRow installationRow = new(
            new CheckResult
            {
                Id = "debug.installation-preview",
                Category = "DEBUG",
                Name = "[DEBUG] 模擬套件安裝",
                Severity = CheckSeverity.Warning,
                CurrentValue = "模擬 CLI 未安裝；不會執行 winget",
                ExpectedValue = "驗證紅色安裝、安全提醒與互斥反灰行為",
                Impact = "僅供 Debug UI 預覽；不會下載、安裝或修改系統。",
                CanFix = false,
                Risk = RiskLevel.None
            },
            isSimulation: true);
        installationRow.SetInstallationCandidate(new PackageInstallationCandidate
        {
            DiagnosticCheckId = installationRow.Id,
            PackageId = package.PackageId,
            DisplayName = $"{package.DisplayName}（DEBUG 模擬）",
            Source = package.Source,
            Action = InstallationAction.Install,
            Risk = package.Risk,
            CurrentValue = installationRow.FullCurrentValue,
            Impact = installationRow.Impact
        });
        Results.Add(installationRow);
    }
#endif

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
