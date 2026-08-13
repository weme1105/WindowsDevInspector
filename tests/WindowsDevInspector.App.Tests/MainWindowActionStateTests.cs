using WindowsDevInspector.App;

namespace WindowsDevInspector.App.Tests;

public sealed class MainWindowActionStateTests
{
    [Fact]
    public void Evaluate_DisablesEveryModifyingActionWhenRuntimeIsNotReady()
    {
        MainWindowActionState state = new();
        state.SetInstallationSelection(isSelected: true, isExecutable: true);

        MainWindowActionAvailability availability = state.Evaluate(hasBackups: true, hasSelectedFixes: false);

        Assert.False(availability.CanSelectRemediation);
        Assert.False(availability.CanSelectInstallation);
        Assert.False(availability.CanStartFix);
        Assert.False(availability.CanSelectLowRiskFixes);
        Assert.False(availability.CanRestoreBackup);
        Assert.False(availability.CanStartInstallation);
    }

    [Fact]
    public void Evaluate_DoesNotReEnableActionsAfterBusyStateEndsWhenRuntimeFailed()
    {
        MainWindowActionState state = new();
        state.SetRuntimeReady(false);
        state.SetBusy(true);
        state.SetBusy(false);

        MainWindowActionAvailability availability = state.Evaluate(hasBackups: true, hasSelectedFixes: false);

        Assert.False(availability.CanStartFix);
        Assert.False(availability.CanRestoreBackup);
    }

    [Fact]
    public void Evaluate_EnablesRemediationAndBackupWhenRuntimeIsReadyAndIdle()
    {
        MainWindowActionState state = new();
        state.SetRuntimeReady(true);

        MainWindowActionAvailability availability = state.Evaluate(hasBackups: true, hasSelectedFixes: true);

        Assert.True(availability.CanSelectRemediation);
        Assert.True(availability.CanSelectInstallation);
        Assert.True(availability.CanStartFix);
        Assert.True(availability.CanSelectLowRiskFixes);
        Assert.True(availability.CanRestoreBackup);
        Assert.False(availability.CanStartInstallation);
    }

    [Fact]
    public void Evaluate_InstallationModeDisablesRemediationAndEnablesOnlyExecutableSelection()
    {
        MainWindowActionState state = new();
        state.SetRuntimeReady(true);
        state.SetInstallationSelection(isSelected: true, isExecutable: true);

        MainWindowActionAvailability availability = state.Evaluate(hasBackups: true, hasSelectedFixes: true);

        Assert.False(availability.CanStartFix);
        Assert.False(availability.CanRestoreBackup);
        Assert.True(availability.CanStartInstallation);
    }

    [Fact]
    public void Evaluate_BusyStateDisablesInstallationAndRemediation()
    {
        MainWindowActionState state = new();
        state.SetRuntimeReady(true);
        state.SetInstallationSelection(isSelected: true, isExecutable: true);
        state.SetBusy(true);

        MainWindowActionAvailability availability = state.Evaluate(hasBackups: true, hasSelectedFixes: true);

        Assert.False(availability.CanStartInstallation);
        Assert.False(availability.CanSelectInstallation);
        Assert.False(availability.CanStartFix);
        Assert.False(availability.CanRestoreBackup);
    }

    [Fact]
    public void Evaluate_DisablesStartFixWhenNoFixIsSelected()
    {
        MainWindowActionState state = new();
        state.SetRuntimeReady(true);

        MainWindowActionAvailability availability = state.Evaluate(
            hasBackups: false,
            hasSelectedFixes: false);

        Assert.False(availability.CanStartFix);
        Assert.True(availability.CanSelectRemediation);
    }
}
