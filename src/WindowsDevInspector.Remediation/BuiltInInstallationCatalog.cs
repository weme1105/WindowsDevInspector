using WindowsDevInspector.Core;

namespace WindowsDevInspector.Remediation;

public static class BuiltInInstallationCatalog
{
    public static ApprovedInstallationCatalog Create()
    {
        return new ApprovedInstallationCatalog(CreatePackages());
    }

    public static IReadOnlyList<ApprovedInstallationPackage> CreatePackages()
    {
        return
        [
            Winget("frontend.pnpm-cli", "install.pnpm-winget", "pnpm.pnpm", "pnpm", "pnpm", ["--version"]),
            Winget("devops.azure-cli", "install.azure-cli-winget", "Microsoft.AzureCLI", "Azure CLI", "az", ["version"]),
            Winget("devops.kubectl", "install.kubectl-winget", "Kubernetes.kubectl", "kubectl", "kubectl", ["version", "--client"]),
            Winget("devops.terraform", "install.terraform-winget", "Hashicorp.Terraform", "Terraform", "terraform", ["version"])
        ];
    }

    private static ApprovedInstallationPackage Winget(
        string diagnosticCheckId,
        string availabilityCheckId,
        string packageId,
        string displayName,
        string verificationExecutable,
        IReadOnlyList<string> verificationArguments)
    {
        return new ApprovedInstallationPackage
        {
            DiagnosticCheckId = diagnosticCheckId,
            AvailabilityCheckId = availabilityCheckId,
            PackageId = packageId,
            DisplayName = displayName,
            Source = InstallationSource.Winget,
            Risk = RiskLevel.Medium,
            RequiresElevation = true,
            RequiresRestart = false,
            RefreshPathAfterInstall = true,
            VerificationExecutable = verificationExecutable,
            VerificationArguments = verificationArguments
        };
    }
}
