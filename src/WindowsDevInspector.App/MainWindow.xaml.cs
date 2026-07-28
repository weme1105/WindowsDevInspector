using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using WindowsDevInspector.Core;
using WindowsDevInspector.Remediation;
using WindowsDevInspector.Windows;

namespace WindowsDevInspector.App;

public partial class MainWindow : Window
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private readonly CheckCatalog checkCatalog = BuiltInCheckCatalog.Create();
    private readonly DirectoryRemediationExecutor directoryRemediationExecutor = new(
        BuiltInRemediationCatalog.CreateWhitelist(),
        new SystemRemediationFileSystem());

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
        RefreshBackupFiles();

        int loadedTechnologyCount = TechnologySelectionConfig.Load(TechnologyGroups);
        if (loadedTechnologyCount > 0)
        {
            ScanStatusTextBlock.Text = $"已載入 {loadedTechnologyCount} 個儲存選項";
        }
    }

    public ObservableCollection<TechnologyGroup> TechnologyGroups { get; }

    public ObservableCollection<CheckResultRow> Results { get; }

    public ObservableCollection<BackupFileRow> BackupFiles { get; } = [];

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
        string[] selectedTechnologyIds = GetSelectedTechnologyIds();

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

    private async void FixAllButton_Click(object sender, RoutedEventArgs e)
    {
        CheckResultRow[] selectableRows = Results
            .Where(result => result.IsFixSelectable && result.RemediationId is not null)
            .ToArray();

        await ExecuteFixesAsync(selectableRows, "沒有可執行的修正項目");
    }

    private async void StartFixButton_Click(object sender, RoutedEventArgs e)
    {
        CheckResultRow[] selectedRows = Results
            .Where(result => result.IsSelectedForFix && result.RemediationId is not null)
            .ToArray();

        await ExecuteFixesAsync(selectedRows, "尚未勾選可執行的修正");
    }

    private async void RestoreLatestBackupButton_Click(object sender, RoutedEventArgs e)
    {
        string? backupPath = BackupComboBox.SelectedItem is BackupFileRow selectedBackup
            ? selectedBackup.FullPath
            : FindLatestBackupPath();
        if (backupPath is null)
        {
            ScanStatusTextBlock.Text = "找不到可還原的備份檔";
            return;
        }

        StartFixButton.IsEnabled = false;
        FixAllButton.IsEnabled = false;
        RestoreLatestBackupButton.IsEnabled = false;
        ScanStatusTextBlock.Text = "還原中...";

        try
        {
            WorkerExecutionResult rollbackResult = await RunElevatedRollbackAsync(backupPath);
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
            StartFixButton.IsEnabled = true;
            FixAllButton.IsEnabled = true;
            RestoreLatestBackupButton.IsEnabled = true;
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
            string reportDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "WindowsDevInspector",
                "Reports");
            Directory.CreateDirectory(reportDirectory);

            string reportPath = Path.Combine(reportDirectory, $"scan-report-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.json");
            ScanReport report = new()
            {
                CreatedAt = DateTimeOffset.UtcNow,
                SelectedTechnologyIds = GetSelectedTechnologyIds(),
                Results = Results.Select(result => result.ToReportRow()).ToArray()
            };

            await File.WriteAllTextAsync(reportPath, JsonSerializer.Serialize(report, JsonOptions));
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

        StartFixButton.IsEnabled = false;
        FixAllButton.IsEnabled = false;
        RestoreLatestBackupButton.IsEnabled = false;
        ScanStatusTextBlock.Text = "修正中...";

        try
        {
            List<RemediationExecutionResult> executionResults = [];
            foreach (CheckResultRow row in selectedRows.Where(row => !row.RequiresElevation))
            {
                executionResults.Add(await directoryRemediationExecutor.ExecuteAsync(row.RemediationId!, CancellationToken.None));
            }

            CheckResultRow[] elevatedRows = selectedRows
                .Where(row => row.RequiresElevation)
                .ToArray();

            WorkerExecutionResult? workerResult = elevatedRows.Length == 0
                ? null
                : await RunElevatedRemediationAsync(elevatedRows);
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
            StartFixButton.IsEnabled = true;
            FixAllButton.IsEnabled = true;
            RestoreLatestBackupButton.IsEnabled = true;
        }
    }

    private static async Task<WorkerExecutionResult> RunElevatedRemediationAsync(IReadOnlyList<CheckResultRow> rows)
    {
        string workerPath = ResolveElevatedWorkerPath();
        string workDirectory = GetRemediationWorkDirectory();
        Directory.CreateDirectory(workDirectory);

        string planId = $"plan-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}";
        string planPath = Path.Combine(workDirectory, $"{planId}.json");
        string resultPath = Path.Combine(workDirectory, $"{planId}.result.json");

        ChangePlan plan = new()
        {
            PlanId = planId,
            Items = rows
                .Select(row => new ChangePlanItem { RemediationId = row.RemediationId! })
                .ToArray()
        };

        await File.WriteAllTextAsync(planPath, JsonSerializer.Serialize(plan, JsonOptions));

        ProcessStartInfo startInfo = new()
        {
            FileName = workerPath,
            Arguments = $"{Quote(planPath)} {Quote(resultPath)}",
            UseShellExecute = true,
            Verb = "runas",
            WorkingDirectory = Path.GetDirectoryName(workerPath)!
        };

        using Process? process = Process.Start(startInfo);
        if (process is null)
        {
            throw new OperationCanceledException("ElevatedWorker did not start.");
        }

        await process.WaitForExitAsync();

        if (!File.Exists(resultPath))
        {
            throw new IOException($"ElevatedWorker did not write result file. Exit code: {process.ExitCode}");
        }

        string resultJson = await File.ReadAllTextAsync(resultPath);
        return JsonSerializer.Deserialize<WorkerExecutionResult>(resultJson, JsonOptions)
            ?? WorkerExecutionResult.Failed(planId, ["ElevatedWorker returned invalid JSON."]);
    }

    private static async Task<WorkerExecutionResult> RunElevatedRollbackAsync(string backupPath)
    {
        string workerPath = ResolveElevatedWorkerPath();
        string workDirectory = GetRemediationWorkDirectory();
        Directory.CreateDirectory(workDirectory);

        string resultPath = Path.Combine(workDirectory, $"rollback-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}.result.json");

        ProcessStartInfo startInfo = new()
        {
            FileName = workerPath,
            Arguments = $"--rollback {Quote(backupPath)} {Quote(resultPath)}",
            UseShellExecute = true,
            Verb = "runas",
            WorkingDirectory = Path.GetDirectoryName(workerPath)!
        };

        using Process? process = Process.Start(startInfo);
        if (process is null)
        {
            throw new OperationCanceledException("ElevatedWorker did not start.");
        }

        await process.WaitForExitAsync();

        if (!File.Exists(resultPath))
        {
            throw new IOException($"ElevatedWorker did not write rollback result file. Exit code: {process.ExitCode}");
        }

        string resultJson = await File.ReadAllTextAsync(resultPath);
        return JsonSerializer.Deserialize<WorkerExecutionResult>(resultJson, JsonOptions)
            ?? WorkerExecutionResult.Failed("rollback", ["ElevatedWorker returned invalid JSON."]);
    }

    private static string? FindLatestBackupPath()
    {
        string backupDirectory = Path.Combine(GetRemediationWorkDirectory(), "Backups");
        if (!Directory.Exists(backupDirectory))
        {
            return null;
        }

        return Directory
            .EnumerateFiles(backupDirectory, "*.backup.json")
            .Select(path => new FileInfo(path))
            .OrderByDescending(file => file.LastWriteTimeUtc)
            .FirstOrDefault()
            ?.FullName;
    }

    private void RefreshBackupFiles()
    {
        BackupFiles.Clear();

        string backupDirectory = Path.Combine(GetRemediationWorkDirectory(), "Backups");
        if (!Directory.Exists(backupDirectory))
        {
            return;
        }

        foreach (FileInfo file in Directory
            .EnumerateFiles(backupDirectory, "*.backup.json")
            .Select(path => new FileInfo(path))
            .OrderByDescending(file => file.LastWriteTimeUtc))
        {
            BackupFiles.Add(new BackupFileRow(file));
        }

        BackupComboBox.SelectedIndex = BackupFiles.Count > 0 ? 0 : -1;
    }

    private static string GetRemediationWorkDirectory()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WindowsDevInspector",
            "Remediation");
    }

    private static string ResolveElevatedWorkerPath()
    {
        string outputPath = Path.Combine(AppContext.BaseDirectory, "WindowsDevInspector.ElevatedWorker.exe");
        if (File.Exists(outputPath))
        {
            return outputPath;
        }

        string developmentPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "WindowsDevInspector.ElevatedWorker",
            "bin",
            "Debug",
            "net10.0-windows",
            "WindowsDevInspector.ElevatedWorker.exe"));

        if (File.Exists(developmentPath))
        {
            return developmentPath;
        }

        throw new FileNotFoundException("ElevatedWorker executable was not found.", outputPath);
    }

    private static string Quote(string value)
    {
        return string.Concat('"', value.Replace("\"", "\\\"", StringComparison.Ordinal), '"');
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
