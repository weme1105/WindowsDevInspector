using WindowsDevInspector.App;
using WindowsDevInspector.Core;
using WindowsDevInspector.Remediation;

namespace WindowsDevInspector.App.Tests;

public sealed class PackageInstallationConfirmationBuilderTests
{
    [Fact]
    public void Build_CreatesPreviewFromApprovedCatalogMetadata()
    {
        ApprovedInstallationCatalog catalog = BuiltInInstallationCatalog.Create();
        PackageInstallationConfirmationBuilder builder = new(
            new InstallationPlanValidator(catalog),
            catalog);
        InstallationPlan plan = Plan(
            new InstallationPlanItem
            {
                PackageId = "Microsoft.AzureCLI",
                Source = InstallationSource.Winget,
                Action = InstallationAction.Install
            });

        PackageInstallationConfirmationBuildResult result = builder.Build(plan);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        PackageInstallationConfirmation confirmation = Assert.IsType<PackageInstallationConfirmation>(result.Confirmation);
        PackageInstallationConfirmationItem item = Assert.Single(confirmation.Items);
        Assert.Equal("Microsoft.AzureCLI", item.PackageId);
        Assert.Equal("Azure CLI", item.DisplayName);
        Assert.Equal(InstallationSource.Winget, item.Source);
        Assert.Equal(InstallationAction.Install, item.Action);
        Assert.Equal(RiskLevel.Medium, item.Risk);
        Assert.True(item.RequiresElevation);
        Assert.False(item.RequiresRestart);
        Assert.True(item.RefreshPathAfterInstall);
        Assert.Equal("az", item.VerificationExecutable);
        Assert.Equal(["version"], item.VerificationArguments);
        Assert.Equal(
            "winget install --id Microsoft.AzureCLI --exact --source winget --accept-package-agreements --accept-source-agreements",
            item.CommandPreview.DisplayCommand);
    }

    [Fact]
    public void Build_FormatsAllRequiredConfirmationDetails()
    {
        ApprovedInstallationCatalog catalog = BuiltInInstallationCatalog.Create();
        PackageInstallationConfirmationBuilder builder = new(
            new InstallationPlanValidator(catalog),
            catalog);
        InstallationPlan plan = Plan(
            new InstallationPlanItem
            {
                PackageId = "pnpm.pnpm",
                Source = InstallationSource.Winget,
                Action = InstallationAction.Install
            });

        PackageInstallationConfirmationBuildResult result = builder.Build(plan);

        string message = Assert.IsType<PackageInstallationConfirmation>(result.Confirmation).Message;
        Assert.Contains("pnpm [pnpm.pnpm]", message);
        Assert.Contains("來源：Winget", message);
        Assert.Contains("動作：Install", message);
        Assert.Contains("風險：Medium", message);
        Assert.Contains("UAC：需要", message);
        Assert.Contains("重開機：不需要", message);
        Assert.Contains("PATH refresh：需要", message);
        Assert.Contains(
            "預計命令：winget install --id pnpm.pnpm --exact --source winget --accept-package-agreements --accept-source-agreements",
            message);
        Assert.Contains("安裝後驗證：pnpm --version", message);
        Assert.Contains("必須另按執行按鈕並再次確認", message);
    }

    [Fact]
    public void Build_RejectsInvalidPlanWithoutCreatingPreview()
    {
        ApprovedInstallationCatalog catalog = BuiltInInstallationCatalog.Create();
        PackageInstallationConfirmationBuilder builder = new(
            new InstallationPlanValidator(catalog),
            catalog);
        InstallationPlan plan = Plan(
            new InstallationPlanItem
            {
                PackageId = "Arbitrary.UnapprovedPackage",
                Source = InstallationSource.Winget,
                Action = InstallationAction.Install
            });

        PackageInstallationConfirmationBuildResult result = builder.Build(plan);

        Assert.False(result.IsValid);
        Assert.Null(result.Confirmation);
        Assert.Contains(result.Errors, error => error.Contains("not approved", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Build_RejectsMissingPlanWithoutCreatingPreview()
    {
        ApprovedInstallationCatalog catalog = BuiltInInstallationCatalog.Create();
        PackageInstallationConfirmationBuilder builder = new(
            new InstallationPlanValidator(catalog),
            catalog);

        PackageInstallationConfirmationBuildResult result = builder.Build(null);

        Assert.False(result.IsValid);
        Assert.Null(result.Confirmation);
        Assert.Contains(result.Errors, error => error.Contains("missing", StringComparison.OrdinalIgnoreCase));
    }

    private static InstallationPlan Plan(params InstallationPlanItem[] items)
    {
        return new InstallationPlan
        {
            PlanId = Guid.NewGuid().ToString("D"),
            Items = items
        };
    }
}
