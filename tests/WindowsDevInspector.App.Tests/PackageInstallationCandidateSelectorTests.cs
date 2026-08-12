using WindowsDevInspector.App;
using WindowsDevInspector.Core;
using WindowsDevInspector.Remediation;

namespace WindowsDevInspector.App.Tests;

public sealed class PackageInstallationCandidateSelectorTests
{
    [Fact]
    public void GetCandidates_ReturnsMappedNonPassDiagnostic()
    {
        PackageInstallationCandidateSelector selector = new(BuiltInInstallationCatalog.Create());
        CheckResultRow row = Row("devops.terraform", CheckSeverity.Info, "terraform was not found");

        PackageInstallationCandidate candidate = Assert.Single(selector.GetCandidates(
            [row, Available("install.terraform-winget")]));

        Assert.Equal("devops.terraform", candidate.DiagnosticCheckId);
        Assert.Equal("Hashicorp.Terraform", candidate.PackageId);
        Assert.Equal("Terraform", candidate.DisplayName);
        Assert.Equal(InstallationSource.Winget, candidate.Source);
        Assert.Equal(InstallationAction.Install, candidate.Action);
        Assert.Equal(RiskLevel.Medium, candidate.Risk);
        Assert.Equal("terraform was not found", candidate.CurrentValue);
        Assert.Equal("Test impact", candidate.Impact);
    }

    [Fact]
    public void GetCandidates_ExcludesPassingAndUnmappedDiagnostics()
    {
        PackageInstallationCandidateSelector selector = new(BuiltInInstallationCatalog.Create());
        CheckResultRow passing = Row("devops.terraform", CheckSeverity.Pass, "terraform 1.0");
        CheckResultRow unmapped = Row("common.git", CheckSeverity.Warning, "git was not found");

        IReadOnlyList<PackageInstallationCandidate> candidates = selector.GetCandidates(
            [passing, unmapped, Available("install.terraform-winget")]);

        Assert.Empty(candidates);
    }

    [Fact]
    public void GetCandidates_DeduplicatesMappedPackage()
    {
        PackageInstallationCandidateSelector selector = new(BuiltInInstallationCatalog.Create());
        CheckResultRow first = Row("devops.azure-cli", CheckSeverity.Info, "az was not found");
        CheckResultRow duplicate = Row("DEVOPS.AZURE-CLI", CheckSeverity.Warning, "az failed");

        IReadOnlyList<PackageInstallationCandidate> candidates = selector.GetCandidates(
            [first, duplicate, Available("install.azure-cli-winget")]);

        Assert.Single(candidates);
    }

    [Fact]
    public void ToPlanItem_UsesOnlyApprovedIdentityAndTypedAction()
    {
        PackageInstallationCandidateSelector selector = new(BuiltInInstallationCatalog.Create());
        PackageInstallationCandidate candidate = Assert.Single(
            selector.GetCandidates(
            [
                Row("frontend.pnpm-cli", CheckSeverity.Info, "pnpm was not found"),
                Available("install.pnpm-winget")
            ]));

        InstallationPlanItem item = candidate.ToPlanItem();

        Assert.Equal("pnpm.pnpm", item.PackageId);
        Assert.Equal(InstallationSource.Winget, item.Source);
        Assert.Equal(InstallationAction.Install, item.Action);
    }

    [Theory]
    [InlineData(CheckSeverity.Info)]
    [InlineData(CheckSeverity.Warning)]
    public void GetCandidates_HidesPackageWhenWingetAvailabilityIsNotPass(CheckSeverity availabilitySeverity)
    {
        PackageInstallationCandidateSelector selector = new(BuiltInInstallationCatalog.Create());

        IReadOnlyList<PackageInstallationCandidate> candidates = selector.GetCandidates(
        [
            Row("devops.terraform", CheckSeverity.Info, "terraform was not found"),
            Row("install.terraform-winget", availabilitySeverity, "Package could not be confirmed")
        ]);

        Assert.Empty(candidates);
    }

    [Fact]
    public void GetCandidates_HidesPackageWhenAvailabilityCheckIsMissing()
    {
        PackageInstallationCandidateSelector selector = new(BuiltInInstallationCatalog.Create());

        IReadOnlyList<PackageInstallationCandidate> candidates = selector.GetCandidates(
            [Row("devops.terraform", CheckSeverity.Info, "terraform was not found")]);

        Assert.Empty(candidates);
    }

    private static CheckResultRow Available(string id)
    {
        return Row(id, CheckSeverity.Pass, "Package available");
    }

    private static CheckResultRow Row(string id, CheckSeverity severity, string currentValue)
    {
        return new CheckResultRow(new CheckResult
        {
            Id = id,
            Category = "Test",
            Name = "Test check",
            Severity = severity,
            CurrentValue = currentValue,
            ExpectedValue = "CLI available",
            Impact = "Test impact"
        });
    }
}
