using System.Diagnostics;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using WindowsDevInspector.Remediation;

namespace WindowsDevInspector.App;

public sealed class RemediationCoordinator
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private readonly DirectoryRemediationExecutor directoryRemediationExecutor;
    private readonly IWorkerProcessRunner workerProcessRunner;
    private readonly string? workerPathOverride;
    private readonly string? workDirectoryOverride;
    private readonly BackupFileService backupFileService = new(
        new DpapiBackupProtector(),
        MachineFingerprint.CreateFromMacAddresses());

    public RemediationCoordinator()
        : this(new WorkerProcessRunner(), workerPathOverride: null, workDirectoryOverride: null)
    {
    }

    public RemediationCoordinator(
        IWorkerProcessRunner workerProcessRunner,
        string? workerPathOverride = null,
        string? workDirectoryOverride = null)
    {
        this.workerProcessRunner = workerProcessRunner;
        this.workerPathOverride = workerPathOverride;
        this.workDirectoryOverride = workDirectoryOverride;
        directoryRemediationExecutor = new DirectoryRemediationExecutor(
            BuiltInRemediationCatalog.CreateWhitelist(),
            new SystemRemediationFileSystem());
    }

    public async Task<IReadOnlyList<RemediationExecutionResult>> ExecuteLocalRemediationsAsync(
        IEnumerable<string> remediationIds,
        CancellationToken cancellationToken)
    {
        List<RemediationExecutionResult> executionResults = [];

        foreach (string remediationId in remediationIds)
        {
            RemediationExecutionResult result = await directoryRemediationExecutor.ExecuteAsync(remediationId, cancellationToken);
            executionResults.Add(WriteBackupIfAvailable($"local-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}", result));
        }

        return executionResults;
    }

    public async Task<WorkerExecutionResult> ExecuteRollbackAsync(string backupPath)
    {
        try
        {
            DirectoryBackup directoryBackup = backupFileService.ReadEncryptedDirectoryBackup(backupPath);
            DirectoryRollbackExecutor rollbackExecutor = new(new SystemRemediationFileSystem());
            RemediationExecutionResult rollbackResult = await rollbackExecutor.RollbackAsync(
                directoryBackup,
                CancellationToken.None);

            return new WorkerExecutionResult
            {
                PlanId = "rollback",
                Succeeded = rollbackResult.Succeeded,
                Errors = [],
                Results = [rollbackResult]
            };
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or FormatException or InvalidOperationException or System.Security.Cryptography.CryptographicException)
        {
            return await ExecuteElevatedRollbackAsync(backupPath);
        }
    }

    public Task<WorkerExecutionResult> ExecuteElevatedRemediationAsync(
        IReadOnlyCollection<string> remediationIds)
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
            Items = remediationIds
                .Select(remediationId => new ChangePlanItem { RemediationId = remediationId })
                .ToArray()
        };

        File.WriteAllText(planPath, JsonSerializer.Serialize(plan, JsonOptions));

        ProcessStartInfo startInfo = new()
        {
            FileName = workerPath,
            Arguments = $"{Quote(planPath)} {Quote(resultPath)}",
            UseShellExecute = true,
            Verb = "runas",
            WorkingDirectory = Path.GetDirectoryName(workerPath)!
        };

        return RunWorkerAsync(startInfo, resultPath, planId, "ElevatedWorker did not write result file.");
    }

    public Task<WorkerExecutionResult> ExecuteElevatedRollbackAsync(string backupPath)
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

        return RunWorkerAsync(startInfo, resultPath, "rollback", "ElevatedWorker did not write rollback result file.");
    }

    public string? FindLatestBackupPath()
    {
        string backupDirectory = GetBackupDirectory();
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

    public IReadOnlyList<BackupFileRow> GetBackupFiles()
    {
        string backupDirectory = GetBackupDirectory();
        if (!Directory.Exists(backupDirectory))
        {
            return [];
        }

        return Directory
            .EnumerateFiles(backupDirectory, "*.backup.json")
            .Select(path => new FileInfo(path))
            .OrderByDescending(file => file.LastWriteTimeUtc)
            .Select(file => new BackupFileRow(file))
            .ToArray();
    }

    private async Task<WorkerExecutionResult> RunWorkerAsync(
        ProcessStartInfo startInfo,
        string resultPath,
        string fallbackPlanId,
        string missingResultMessage)
    {
        int exitCode;
        try
        {
            exitCode = await workerProcessRunner.RunAsync(startInfo);
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
        {
            throw new OperationCanceledException("UAC was canceled.", ex);
        }

        if (!File.Exists(resultPath))
        {
            return WorkerExecutionResult.Failed(
                fallbackPlanId,
                [$"{missingResultMessage} Exit code: {exitCode}"]);
        }

        string resultJson = await File.ReadAllTextAsync(resultPath);
        return JsonSerializer.Deserialize<WorkerExecutionResult>(resultJson, JsonOptions)
            ?? WorkerExecutionResult.Failed(fallbackPlanId, ["ElevatedWorker returned invalid JSON."]);
    }

    private RemediationExecutionResult WriteBackupIfAvailable(
        string planId,
        RemediationExecutionResult executionResult)
    {
        if (!executionResult.Succeeded || string.IsNullOrWhiteSpace(executionResult.BackupJson))
        {
            return executionResult;
        }

        string backupDirectory = GetBackupDirectory();
        Directory.CreateDirectory(backupDirectory);

        string safeRemediationId = executionResult.RemediationId.Replace(":", "-", StringComparison.Ordinal);
        string backupPath = Path.Combine(backupDirectory, $"{planId}.{safeRemediationId}.backup.json");
        backupFileService.WriteEncryptedBackup(backupPath, executionResult.BackupJson);

        return executionResult.WithBackupPath(backupPath);
    }

    private string GetBackupDirectory()
    {
        return Path.Combine(GetRemediationWorkDirectory(), "Backups");
    }

    private string GetRemediationWorkDirectory()
    {
        return workDirectoryOverride ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WindowsDevInspector",
            "Remediation");
    }

    private string ResolveElevatedWorkerPath()
    {
        if (workerPathOverride is not null)
        {
            return workerPathOverride;
        }

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
}

