using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using WindowsDevInspector.Remediation;

namespace WindowsDevInspector.App;

public sealed class PackageInstallationCoordinator
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly PackageInstallationConfirmationBuilder confirmationBuilder;
    private readonly Func<Guid> planIdProvider;
    private readonly IWorkerProcessRunner workerProcessRunner;
    private readonly string? workerPathOverride;
    private readonly string? workDirectoryOverride;

    public PackageInstallationCoordinator()
    {
        ApprovedInstallationCatalog catalog = BuiltInInstallationCatalog.Create();
        confirmationBuilder = new PackageInstallationConfirmationBuilder(
            new InstallationPlanValidator(catalog),
            catalog);
        planIdProvider = Guid.NewGuid;
        workerProcessRunner = new WorkerProcessRunner();
    }

    public PackageInstallationCoordinator(
        PackageInstallationConfirmationBuilder confirmationBuilder,
        Func<Guid> planIdProvider,
        IWorkerProcessRunner? workerProcessRunner = null,
        string? workerPathOverride = null,
        string? workDirectoryOverride = null)
    {
        this.confirmationBuilder = confirmationBuilder;
        this.planIdProvider = planIdProvider;
        this.workerProcessRunner = workerProcessRunner ?? new WorkerProcessRunner();
        this.workerPathOverride = workerPathOverride;
        this.workDirectoryOverride = workDirectoryOverride;
    }

    public PackageInstallationConfirmationBuildResult CreatePreview(
        IReadOnlyList<InstallationPlanItem>? items)
    {
        InstallationPlan? plan = items is null
            ? null
            : new InstallationPlan
            {
                PlanId = planIdProvider().ToString("D"),
                Items = items.ToArray()
            };

        return confirmationBuilder.Build(plan);
    }

    public async Task<PackageInstallationExecutionResult> ExecuteConfirmedAsync(
        PackageInstallationConfirmation confirmation)
    {
        ArgumentNullException.ThrowIfNull(confirmation);

        InstallationPlan plan = new()
        {
            PlanId = confirmation.PlanId,
            Items = confirmation.Items
                .Select(item => new InstallationPlanItem
                {
                    PackageId = item.PackageId,
                    Source = item.Source,
                    Action = item.Action
                })
                .ToArray()
        };

        PackageInstallationConfirmationBuildResult validation = confirmationBuilder.Build(plan);
        if (!validation.IsValid)
        {
            return PackageInstallationExecutionResult.Invalid(plan.PlanId, validation.Errors);
        }

        string workerPath = ResolveElevatedWorkerPath();
        string workDirectory = workDirectoryOverride ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WindowsDevInspector",
            "Installation");
        Directory.CreateDirectory(workDirectory);

        string planPath = Path.Combine(workDirectory, $"{plan.PlanId}.installation.json");
        string resultPath = Path.Combine(workDirectory, $"{plan.PlanId}.installation.result.json");
        await File.WriteAllTextAsync(planPath, JsonSerializer.Serialize(plan, JsonOptions));

        ProcessStartInfo startInfo = new()
        {
            FileName = workerPath,
            Arguments = $"--install {Quote(planPath)} {Quote(resultPath)}",
            UseShellExecute = true,
            Verb = "runas",
            WorkingDirectory = Path.GetDirectoryName(workerPath)!
        };

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
            return PackageInstallationExecutionResult.Invalid(
                plan.PlanId,
                [$"ElevatedWorker did not write installation result file. Exit code: {exitCode}"]);
        }

        string resultJson = await File.ReadAllTextAsync(resultPath);
        return JsonSerializer.Deserialize<PackageInstallationExecutionResult>(resultJson, JsonOptions)
            ?? PackageInstallationExecutionResult.Invalid(
                plan.PlanId,
                ["ElevatedWorker returned invalid installation result JSON."]);
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

        throw new FileNotFoundException("ElevatedWorker executable was not found.", outputPath);
    }

    private static string Quote(string value)
    {
        return string.Concat('"', value.Replace("\"", "\\\"", StringComparison.Ordinal), '"');
    }
}
