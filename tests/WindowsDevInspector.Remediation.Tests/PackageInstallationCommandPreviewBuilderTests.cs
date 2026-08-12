namespace WindowsDevInspector.Remediation.Tests;

public sealed class PackageInstallationCommandPreviewBuilderTests
{
    [Fact]
    public void Build_UsesOnlyApprovedPackageIdentityAndFixedTokens()
    {
        PackageInstallationCommandPreviewBuilder builder = new(BuiltInInstallationCatalog.Create());

        PackageInstallationCommandPreview preview = builder.Build(new InstallationPlanItem
        {
            PackageId = "Microsoft.AzureCLI",
            Source = InstallationSource.Winget,
            Action = InstallationAction.Install
        });

        Assert.Equal("winget", preview.FileName);
        Assert.Equal(
            [
                "install", "--id", "Microsoft.AzureCLI", "--exact", "--source", "winget",
                "--accept-package-agreements", "--accept-source-agreements"
            ],
            preview.Arguments);
        Assert.Contains("--accept-package-agreements", preview.Arguments);
        Assert.Contains("--accept-source-agreements", preview.Arguments);
        Assert.DoesNotContain(preview.Arguments, argument => argument.Contains("silent", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Build_RejectsUnapprovedPackage()
    {
        PackageInstallationCommandPreviewBuilder builder = new(BuiltInInstallationCatalog.Create());

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => builder.Build(new InstallationPlanItem
            {
                PackageId = "Arbitrary.Package",
                Source = InstallationSource.Winget,
                Action = InstallationAction.Install
            }));

        Assert.Contains("not approved", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}
