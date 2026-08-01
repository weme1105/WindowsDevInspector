using System.Text.Json;
using WindowsDevInspector.ElevatedWorker;
using WindowsDevInspector.Remediation;

JsonSerializerOptions jsonOptions = new()
{
    PropertyNameCaseInsensitive = true,
    WriteIndented = true
};
BackupFileService backupFileService = new(
    new WindowsDevInspector.Remediation.DpapiBackupProtector(),
    WindowsDevInspector.Remediation.MachineFingerprint.CreateFromMacAddresses());
string? resultPath = null;

if (args.Length is not 1 and not 2 and not 3)
{
    WriteResult(WorkerExecutionResult.Failed(
        "unknown",
        ["Usage: WindowsDevInspector.ElevatedWorker <change-plan.json> [result.json] OR --rollback <backup.json> [result.json]"]));
    return 2;
}

bool isRollback = string.Equals(args[0], "--rollback", StringComparison.OrdinalIgnoreCase);
resultPath = isRollback
    ? args.Length == 3 ? args[2] : null
    : args.Length == 2 ? args[1] : null;

if (!Elevation.IsProcessElevated())
{
    WriteResult(WorkerExecutionResult.Failed(
        "unknown",
        ["ElevatedWorker must run with administrator privileges."]));
    return 5;
}

if (isRollback)
{
    return await RunRollbackAsync(args[1]);
}

string planPath = args[0];
ChangePlan? plan;

try
{
    await using FileStream stream = File.OpenRead(planPath);
    plan = await JsonSerializer.DeserializeAsync<ChangePlan>(stream, jsonOptions);
}
catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
{
    WriteResult(WorkerExecutionResult.Failed(
        "unknown",
        [$"Could not read change plan: {ex.Message}"]));
    return 3;
}

ChangePlanValidator validator = new(BuiltInRemediationCatalog.CreateWhitelist());
ChangePlanValidationResult validationResult = validator.Validate(plan, allowElevationRequiredItems: true);

if (!validationResult.IsValid)
{
    WriteResult(WorkerExecutionResult.Failed(plan?.PlanId ?? "unknown", validationResult.Errors));
    return 4;
}

RegistryDwordRemediationExecutor registryExecutor = new(
    BuiltInRemediationCatalog.CreateWhitelist(),
    new WindowsRemediationRegistry());

List<RemediationExecutionResult> results = [];

foreach (ChangePlanItem item in plan!.Items)
{
    try
    {
        RemediationExecutionResult executionResult = await registryExecutor.ExecuteAsync(
            item.RemediationId,
            CancellationToken.None);

        results.Add(WriteBackupIfAvailable(plan.PlanId, executionResult));
    }
    catch (Exception ex) when (ex is UnauthorizedAccessException or IOException or InvalidOperationException)
    {
        results.Add(RemediationExecutionResult.Skip(
            item.RemediationId,
            $"Remediation failed: {ex.Message}"));
    }
}

WriteResult(new WorkerExecutionResult
{
    PlanId = plan.PlanId,
    Succeeded = results.All(result => result.Succeeded),
    Errors = [],
    Results = results
});

return 0;

void WriteResult(WorkerExecutionResult result)
{
    string json = JsonSerializer.Serialize(result, jsonOptions);

    if (resultPath is null)
    {
        Console.WriteLine(json);
        return;
    }

    Directory.CreateDirectory(Path.GetDirectoryName(resultPath)!);
    File.WriteAllText(resultPath, json);
}

RemediationExecutionResult WriteBackupIfAvailable(
    string planId,
    RemediationExecutionResult executionResult)
{
    if (!executionResult.Succeeded || string.IsNullOrWhiteSpace(executionResult.BackupJson))
    {
        return executionResult;
    }

    string backupDirectory = resultPath is null
        ? Path.Combine(AppContext.BaseDirectory, "Backups")
        : Path.Combine(Path.GetDirectoryName(resultPath)!, "Backups");
    Directory.CreateDirectory(backupDirectory);

    string safeRemediationId = executionResult.RemediationId.Replace(":", "-", StringComparison.Ordinal);
    string backupPath = Path.Combine(backupDirectory, $"{planId}.{safeRemediationId}.backup.json");
    backupFileService.WriteEncryptedBackup(backupPath, executionResult.BackupJson);

    return executionResult.WithBackupPath(backupPath);
}

async Task<int> RunRollbackAsync(string backupPath)
{
    RegistryDwordBackup? backup;

    try
    {
        backup = backupFileService.ReadEncryptedRegistryBackup(backupPath);
    }
    catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or FormatException or InvalidOperationException or System.Security.Cryptography.CryptographicException)
    {
        WriteResult(WorkerExecutionResult.Failed(
            "rollback",
            [$"Backup is invalid or cannot be decrypted: {ex.Message}"]));
        return 3;
    }

    if (backup is null)
    {
        WriteResult(WorkerExecutionResult.Failed("rollback", ["Rollback backup is invalid JSON."]));
        return 4;
    }

    RegistryDwordRollbackExecutor rollbackExecutor = new(new WindowsRemediationRegistry());
    RemediationExecutionResult rollbackResult = await rollbackExecutor.RollbackAsync(backup, CancellationToken.None);

    WriteResult(new WorkerExecutionResult
    {
        PlanId = "rollback",
        Succeeded = rollbackResult.Succeeded,
        Errors = [],
        Results = [rollbackResult]
    });

    return rollbackResult.Succeeded ? 0 : 6;
}
