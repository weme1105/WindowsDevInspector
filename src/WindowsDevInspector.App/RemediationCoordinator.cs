using System.Diagnostics;
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

    private readonly DirectoryRemediationExecutor directoryRemediationExecutor = new(
        BuiltInRemediationCatalog.CreateWhitelist(),
        new SystemRemediationFileSystem());

    public async Task<IReadOnlyList<RemediationExecutionResult>> ExecuteLocalRemediationsAsync(
        IEnumerable<string> remediationIds,
        CancellationToken cancellationToken)
    {
        List<RemediationExecutionResult> executionResults = [];

        foreach (string remediationId in remediationIds)
        {
            executionResults.Add(await directoryRemediationExecutor.ExecuteAsync(remediationId, cancellationToken));
        }

        return executionResults;
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

    private static async Task<WorkerExecutionResult> RunWorkerAsync(
        ProcessStartInfo startInfo,
        string resultPath,
        string fallbackPlanId,
        string missingResultMessage)
    {
        using Process? process = Process.Start(startInfo);
        if (process is null)
        {
            throw new OperationCanceledException("ElevatedWorker did not start.");
        }

        await process.WaitForExitAsync();

        if (!File.Exists(resultPath))
        {
            throw new IOException($"{missingResultMessage} Exit code: {process.ExitCode}");
        }

        string resultJson = await File.ReadAllTextAsync(resultPath);
        return JsonSerializer.Deserialize<WorkerExecutionResult>(resultJson, JsonOptions)
            ?? WorkerExecutionResult.Failed(fallbackPlanId, ["ElevatedWorker returned invalid JSON."]);
    }

    private static string GetBackupDirectory()
    {
        return Path.Combine(GetRemediationWorkDirectory(), "Backups");
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
}

