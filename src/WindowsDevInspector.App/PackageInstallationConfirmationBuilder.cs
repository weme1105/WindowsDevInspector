using WindowsDevInspector.Remediation;

namespace WindowsDevInspector.App;

public sealed class PackageInstallationConfirmationBuilder(
    InstallationPlanValidator validator,
    ApprovedInstallationCatalog catalog)
{
    private readonly PackageInstallationCommandPreviewBuilder commandPreviewBuilder = new(catalog);

    public PackageInstallationConfirmationBuildResult Build(InstallationPlan? plan)
    {
        InstallationPlanValidationResult validationResult = validator.Validate(plan);
        if (!validationResult.IsValid || plan is null)
        {
            return PackageInstallationConfirmationBuildResult.Rejected(validationResult.Errors);
        }

        PackageInstallationConfirmationItem[] items = plan.Items
            .Select(item => CreateItem(item, catalog.GetRequired(item.PackageId, item.Source)))
            .ToArray();

        return PackageInstallationConfirmationBuildResult.Accepted(
            new PackageInstallationConfirmation
            {
                PlanId = plan.PlanId,
                Items = items,
                Message = BuildMessage(items)
            });
    }

    private PackageInstallationConfirmationItem CreateItem(
        InstallationPlanItem item,
        ApprovedInstallationPackage package)
    {
        return new PackageInstallationConfirmationItem
        {
            PackageId = package.PackageId,
            DisplayName = package.DisplayName,
            Source = package.Source,
            Action = item.Action,
            Risk = package.Risk,
            RequiresElevation = package.RequiresElevation,
            RequiresRestart = package.RequiresRestart,
            RefreshPathAfterInstall = package.RefreshPathAfterInstall,
            VerificationExecutable = package.VerificationExecutable,
            VerificationArguments = package.VerificationArguments.ToArray(),
            CommandPreview = commandPreviewBuilder.Build(item)
        };
    }

    private static string BuildMessage(IReadOnlyList<PackageInstallationConfirmationItem> items)
    {
        List<string> lines =
        [
            "以下是已驗證的單一套件安裝計畫。",
            $"計畫包含 {items.Count} 個核准套件。",
            ""
        ];

        for (int index = 0; index < items.Count; index++)
        {
            PackageInstallationConfirmationItem item = items[index];
            string verification = string.Join(
                " ",
                new[] { item.VerificationExecutable }.Concat(item.VerificationArguments));

            lines.Add($"{index + 1}. {item.DisplayName} [{item.PackageId}]");
            lines.Add($"   來源：{item.Source}；動作：{item.Action}；風險：{item.Risk}");
            lines.Add($"   UAC：{RequiredText(item.RequiresElevation)}；重開機：{RequiredText(item.RequiresRestart)}；PATH refresh：{RequiredText(item.RefreshPathAfterInstall)}");
            lines.Add($"   預計命令：{item.CommandPreview.DisplayCommand}");
            lines.Add($"   安裝後驗證：{verification}");
        }

        lines.Add("");
        lines.Add("此畫面只建立預覽；必須另按執行按鈕並再次確認，才會啟動 UAC 與 ElevatedWorker。");

        return string.Join(Environment.NewLine, lines);
    }

    private static string RequiredText(bool required)
    {
        return required ? "需要" : "不需要";
    }
}
