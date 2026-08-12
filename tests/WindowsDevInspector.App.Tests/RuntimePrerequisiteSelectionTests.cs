using WindowsDevInspector.App;
using WindowsDevInspector.Core;
using WindowsDevInspector.Remediation;

namespace WindowsDevInspector.App.Tests;

public sealed class RuntimePrerequisiteSelectionTests
{
    [Fact]
    public void Apply_EnablesActionsWhenRuntimeCheckPasses()
    {
        CheckResultRow runtime = RuntimeRow(CheckSeverity.Pass);
        CheckResultRow remediation = RemediationRow();
        CheckResultRow installation = InstallationRow();

        bool ready = RuntimePrerequisiteSelection.Apply([runtime, remediation, installation]);

        Assert.True(ready);
        Assert.True(remediation.IsActionSelectionEnabled);
        Assert.True(installation.IsActionSelectionEnabled);
    }

    [Theory]
    [InlineData(CheckSeverity.Warning)]
    [InlineData(CheckSeverity.Info)]
    public void Apply_DisablesAndClearsDependentActionsWhenRuntimeIsNotReady(CheckSeverity severity)
    {
        CheckResultRow runtime = RuntimeRow(severity);
        CheckResultRow remediation = RemediationRow();
        CheckResultRow installation = InstallationRow();
        remediation.IsSelectedForFix = true;
        PackageInstallationSelection.SelectSingle([remediation, installation], installation);

        bool ready = RuntimePrerequisiteSelection.Apply([runtime, remediation, installation]);

        Assert.False(ready);
        Assert.False(remediation.IsActionSelectionEnabled);
        Assert.False(installation.IsActionSelectionEnabled);
        Assert.False(remediation.IsSelectedForFix);
        Assert.False(installation.IsSelectedForInstallation);
        Assert.Equal("#C62828", remediation.ActionForeground);
        Assert.Contains("Runtime", remediation.ActionToolTip);
    }

    [Fact]
    public void Apply_DisablesActionsWhenRuntimeResultIsMissing()
    {
        CheckResultRow remediation = RemediationRow();

        Assert.False(RuntimePrerequisiteSelection.Apply([remediation]));
        Assert.False(remediation.IsActionSelectionEnabled);
    }

    private static CheckResultRow RuntimeRow(CheckSeverity severity) => new(new CheckResult
    {
        Id = RuntimePrerequisiteSelection.RuntimeCheckId,
        Category = "Common",
        Name = ".NET 10 Desktop Runtime x64",
        Severity = severity,
        CurrentValue = "Test",
        ExpectedValue = "Microsoft.WindowsDesktop.App 10.x x64",
        Impact = "Required by WindowsDevInspector"
    });

    private static CheckResultRow RemediationRow() => new(new CheckResult
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

    private static CheckResultRow InstallationRow()
    {
        CheckResultRow row = new(new CheckResult
        {
            Id = "devops.terraform",
            Category = "DevOps",
            Name = "Terraform",
            Severity = CheckSeverity.Warning,
            CurrentValue = "Missing",
            ExpectedValue = "Available",
            Impact = "Test"
        });
        row.SetInstallationCandidate(new PackageInstallationCandidate
        {
            DiagnosticCheckId = row.Id,
            PackageId = "Hashicorp.Terraform",
            DisplayName = "Terraform",
            Source = InstallationSource.Winget,
            Action = InstallationAction.Install,
            Risk = RiskLevel.Medium,
            CurrentValue = row.CurrentValue,
            Impact = row.Impact
        });
        return row;
    }
}
