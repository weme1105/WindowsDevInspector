using System.Diagnostics;
using System.Text.Json;
using WindowsDevInspector.App;
using WindowsDevInspector.Remediation;

namespace WindowsDevInspector.App.Tests;

public sealed class PackageInstallationCoordinatorTests
{
    [Fact]
    public void CreatePreview_BuildsValidatedConfirmationWithGeneratedPlanId()
    {
        Guid planId = Guid.Parse("973b4ce9-eef9-4944-95ba-46491de8ee0c");
        PackageInstallationCoordinator coordinator = CreateCoordinator(() => planId);

        PackageInstallationConfirmationBuildResult result = coordinator.CreatePreview(
        [
            new InstallationPlanItem
            {
                PackageId = "Kubernetes.kubectl",
                Source = InstallationSource.Winget,
                Action = InstallationAction.Install
            }
        ]);

        Assert.True(result.IsValid);
        PackageInstallationConfirmation confirmation = Assert.IsType<PackageInstallationConfirmation>(result.Confirmation);
        Assert.Equal(planId.ToString("D"), confirmation.PlanId);
        Assert.Equal("Kubernetes.kubectl", Assert.Single(confirmation.Items).PackageId);
    }

    [Fact]
    public void CreatePreview_RejectsEmptyItems()
    {
        PackageInstallationCoordinator coordinator = CreateCoordinator(Guid.NewGuid);

        PackageInstallationConfirmationBuildResult result = coordinator.CreatePreview([]);

        Assert.False(result.IsValid);
        Assert.Null(result.Confirmation);
        Assert.Contains(result.Errors, error => error.Contains("exactly one", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void CreatePreview_RejectsMissingItems()
    {
        PackageInstallationCoordinator coordinator = CreateCoordinator(Guid.NewGuid);

        PackageInstallationConfirmationBuildResult result = coordinator.CreatePreview(null);

        Assert.False(result.IsValid);
        Assert.Null(result.Confirmation);
        Assert.Contains(result.Errors, error => error.Contains("missing", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void CreatePreview_RejectsUnapprovedPackage()
    {
        PackageInstallationCoordinator coordinator = CreateCoordinator(Guid.NewGuid);

        PackageInstallationConfirmationBuildResult result = coordinator.CreatePreview(
        [
            new InstallationPlanItem
            {
                PackageId = "Arbitrary.Package",
                Source = InstallationSource.Winget,
                Action = InstallationAction.Install
            }
        ]);

        Assert.False(result.IsValid);
        Assert.Null(result.Confirmation);
        Assert.Contains(result.Errors, error => error.Contains("not approved", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ExecuteConfirmedAsync_WritesSinglePlanAndUsesInstallWorkerMode()
    {
        string workDirectory = Path.Combine(Path.GetTempPath(), $"WindowsDevInspector-{Guid.NewGuid():N}");
        string workerPath = Path.Combine(workDirectory, "WindowsDevInspector.ElevatedWorker.exe");
        FakeWorkerProcessRunner runner = new(startInfo =>
        {
            string resultPath = ExtractQuotedArguments(startInfo.Arguments)[1];
            PackageInstallationExecutionResult workerResult = new()
            {
                PlanId = "973b4ce9-eef9-4944-95ba-46491de8ee0c",
                Succeeded = true,
                Errors = [],
                Results = []
            };
            File.WriteAllText(resultPath, JsonSerializer.Serialize(workerResult));
            return 0;
        });
        PackageInstallationCoordinator coordinator = CreateCoordinator(
            () => Guid.Parse("973b4ce9-eef9-4944-95ba-46491de8ee0c"),
            runner,
            workerPath,
            workDirectory);
        PackageInstallationConfirmation confirmation = Assert.IsType<PackageInstallationConfirmation>(
            coordinator.CreatePreview([Item("pnpm.pnpm")]).Confirmation);

        PackageInstallationExecutionResult result = await coordinator.ExecuteConfirmedAsync(confirmation);

        Assert.True(result.Succeeded);
        ProcessStartInfo startInfo = Assert.Single(runner.StartInfos);
        Assert.StartsWith("--install ", startInfo.Arguments, StringComparison.Ordinal);
        Assert.Equal("runas", startInfo.Verb);
        string planPath = ExtractQuotedArguments(startInfo.Arguments)[0];
        InstallationPlan plan = Assert.IsType<InstallationPlan>(
            JsonSerializer.Deserialize<InstallationPlan>(File.ReadAllText(planPath), new JsonSerializerOptions(JsonSerializerDefaults.Web)));
        Assert.Single(plan.Items);
        Assert.Equal("pnpm.pnpm", plan.Items[0].PackageId);
    }

    private static PackageInstallationCoordinator CreateCoordinator(
        Func<Guid> planIdProvider,
        IWorkerProcessRunner? runner = null,
        string? workerPath = null,
        string? workDirectory = null)
    {
        ApprovedInstallationCatalog catalog = BuiltInInstallationCatalog.Create();
        PackageInstallationConfirmationBuilder builder = new(
            new InstallationPlanValidator(catalog),
            catalog);

        return new PackageInstallationCoordinator(builder, planIdProvider, runner, workerPath, workDirectory);
    }

    private static InstallationPlanItem Item(string packageId) => new()
    {
        PackageId = packageId,
        Source = InstallationSource.Winget,
        Action = InstallationAction.Install
    };

    private static string[] ExtractQuotedArguments(string arguments)
    {
        return System.Text.RegularExpressions.Regex.Matches(arguments, "\"([^\"]+)\"")
            .Select(match => match.Groups[1].Value)
            .ToArray();
    }

    private sealed class FakeWorkerProcessRunner(Func<ProcessStartInfo, int> run) : IWorkerProcessRunner
    {
        public List<ProcessStartInfo> StartInfos { get; } = [];

        public Task<int> RunAsync(ProcessStartInfo startInfo)
        {
            StartInfos.Add(startInfo);
            return Task.FromResult(run(startInfo));
        }
    }
}
