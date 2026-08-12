namespace WindowsDevInspector.Remediation;

public sealed class PackageInstallationCommandPreviewBuilder(ApprovedInstallationCatalog catalog)
{
    public PackageInstallationCommandPreview Build(InstallationPlanItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (item.Action != InstallationAction.Install)
        {
            throw new InvalidOperationException(
                $"Installation action '{item.Action}' does not have an approved command preview.");
        }

        ApprovedInstallationPackage package = catalog.GetRequired(item.PackageId, item.Source);

        return package.Source switch
        {
            InstallationSource.Winget => new PackageInstallationCommandPreview
            {
                PackageId = package.PackageId,
                Source = package.Source,
                FileName = "winget",
                Arguments = Array.AsReadOnly(new[]
                {
                    "install",
                    "--id",
                    package.PackageId,
                    "--exact",
                    "--source",
                    "winget",
                    "--accept-package-agreements",
                    "--accept-source-agreements"
                })
            },
            _ => throw new InvalidOperationException(
                $"Installation source '{package.Source}' does not have an approved command preview.")
        };
    }
}
