using WindowsDevInspector.App;
using WindowsDevInspector.Core;
using WindowsDevInspector.Remediation;

namespace WindowsDevInspector.App.Tests;

public sealed class PackageInstallationSelectionTests
{
    [Fact]
    public void SelectSingle_SelectsTargetAndDisablesOtherCandidates()
    {
        CheckResultRow first = CandidateRow("devops.azure-cli", "Microsoft.AzureCLI");
        CheckResultRow second = CandidateRow("devops.terraform", "Hashicorp.Terraform");

        bool selected = PackageInstallationSelection.SelectSingle([first, second], first);

        Assert.True(selected);
        Assert.True(first.IsSelectedForInstallation);
        Assert.True(first.IsInstallationSelectionEnabled);
        Assert.False(second.IsSelectedForInstallation);
        Assert.False(second.IsInstallationSelectionEnabled);
        Assert.False(first.IsFixSelectionEnabled);
        Assert.False(second.IsFixSelectionEnabled);
        Assert.Same(first, PackageInstallationSelection.GetSelected([first, second]));
    }

    [Fact]
    public void Clear_DeselectsAndEnablesAllCandidates()
    {
        CheckResultRow first = CandidateRow("devops.azure-cli", "Microsoft.AzureCLI");
        CheckResultRow second = CandidateRow("devops.terraform", "Hashicorp.Terraform");
        PackageInstallationSelection.SelectSingle([first, second], first);

        PackageInstallationSelection.Clear([first, second]);

        Assert.False(first.IsSelectedForInstallation);
        Assert.True(first.IsInstallationSelectionEnabled);
        Assert.False(second.IsSelectedForInstallation);
        Assert.True(second.IsInstallationSelectionEnabled);
        Assert.False(first.IsFixSelectionEnabled);
        Assert.False(second.IsFixSelectionEnabled);
        Assert.Null(PackageInstallationSelection.GetSelected([first, second]));
    }

    [Fact]
    public void SelectSingle_RejectsNonCandidateAndClearsExistingSelection()
    {
        CheckResultRow candidate = CandidateRow("devops.azure-cli", "Microsoft.AzureCLI");
        CheckResultRow nonCandidate = Row("common.git");
        PackageInstallationSelection.SelectSingle([candidate, nonCandidate], candidate);

        bool selected = PackageInstallationSelection.SelectSingle([candidate, nonCandidate], nonCandidate);

        Assert.False(selected);
        Assert.False(candidate.IsSelectedForInstallation);
        Assert.True(candidate.IsInstallationSelectionEnabled);
    }

    [Fact]
    public void InstallationSelection_ClearsAndDisablesRemediationSelection()
    {
        CheckResultRow remediation = new(new CheckResult
        {
            Id = "common.directory-source",
            Category = "Common",
            Name = "D:\\Source exists",
            Severity = CheckSeverity.Warning,
            CurrentValue = "Missing",
            ExpectedValue = "Exists",
            Impact = "Test",
            CanFix = true,
            Risk = RiskLevel.Low,
            RemediationId = "create-source-directory"
        });
        remediation.IsSelectedForFix = true;
        CheckResultRow installation = CandidateRow("frontend.pnpm-cli", "pnpm.pnpm");

        PackageInstallationSelection.SelectSingle([remediation, installation], installation);

        Assert.False(remediation.IsSelectedForFix);
        Assert.False(remediation.IsFixSelectionEnabled);
        Assert.False(remediation.IsActionSelectionEnabled);
        Assert.True(installation.IsSelectedForInstallation);
    }

    [Fact]
    public void Clear_ReEnablesRemediationAfterInstallationMode()
    {
        CheckResultRow remediation = RemediationRow();
        CheckResultRow installation = CandidateRow("frontend.pnpm-cli", "pnpm.pnpm");
        PackageInstallationSelection.SelectSingle([remediation, installation], installation);

        PackageInstallationSelection.Clear([remediation, installation]);

        Assert.True(remediation.IsFixSelectionEnabled);
        Assert.True(remediation.IsActionSelectionEnabled);
        Assert.False(remediation.IsSelectedForFix);
    }

    [Fact]
    public void Clear_BeforeInstallationModePreservesRemediationSelection()
    {
        CheckResultRow remediation = RemediationRow();
        CheckResultRow installation = CandidateRow("frontend.pnpm-cli", "pnpm.pnpm");
        remediation.IsSelectedForFix = true;

        PackageInstallationSelection.Clear([remediation, installation]);

        Assert.True(remediation.IsSelectedForFix);
        Assert.True(remediation.IsFixSelectionEnabled);
        Assert.False(installation.IsSelectedForInstallation);
    }

    [Fact]
    public void UnifiedActionProperties_DistinguishInstallationFromRemediation()
    {
        CheckResultRow remediation = RemediationRow();
        CheckResultRow installation = CandidateRow("frontend.pnpm-cli", "pnpm.pnpm");

        remediation.IsSelectedForFix = true;

        Assert.Equal("修正", remediation.ActionLabel);
        Assert.Equal("#162033", remediation.ActionForeground);
        Assert.True(remediation.IsSelectedForAction);
        Assert.Equal("安裝", installation.ActionLabel);
        Assert.Equal("#C62828", installation.ActionForeground);
        Assert.False(installation.IsSelectedForAction);
    }

    private static CheckResultRow CandidateRow(string checkId, string packageId)
    {
        CheckResultRow row = Row(checkId);
        row.SetInstallationCandidate(new PackageInstallationCandidate
        {
            DiagnosticCheckId = checkId,
            PackageId = packageId,
            DisplayName = packageId,
            Source = InstallationSource.Winget,
            Action = InstallationAction.Install,
            Risk = RiskLevel.Medium,
            CurrentValue = "Missing",
            Impact = "Test"
        });
        return row;
    }

    private static CheckResultRow RemediationRow()
    {
        return new CheckResultRow(new CheckResult
        {
            Id = "common.directory-source",
            Category = "Common",
            Name = "D:\\Source exists",
            Severity = CheckSeverity.Warning,
            CurrentValue = "Missing",
            ExpectedValue = "Exists",
            Impact = "Test",
            CanFix = true,
            Risk = RiskLevel.Low,
            RemediationId = "create-source-directory"
        });
    }

    private static CheckResultRow Row(string checkId)
    {
        return new CheckResultRow(new CheckResult
        {
            Id = checkId,
            Category = "Test",
            Name = "Test check",
            Severity = CheckSeverity.Warning,
            CurrentValue = "Missing",
            ExpectedValue = "Available",
            Impact = "Test"
        });
    }
}
