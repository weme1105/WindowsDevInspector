namespace WindowsDevInspector.App;

public sealed class MainWindowActionState
{
    public bool IsRuntimeReady { get; private set; }

    public bool IsBusy { get; private set; }

    public bool IsInstallationMode { get; private set; }

    public bool HasExecutableInstallationSelection { get; private set; }

    public void SetRuntimeReady(bool isReady) => IsRuntimeReady = isReady;

    public void SetBusy(bool isBusy) => IsBusy = isBusy;

    public void SetInstallationSelection(bool isSelected, bool isExecutable)
    {
        IsInstallationMode = isSelected;
        HasExecutableInstallationSelection = isSelected && isExecutable;
    }

    public MainWindowActionAvailability Evaluate(bool hasBackups)
    {
        bool remediationEnabled = IsRuntimeReady && !IsBusy && !IsInstallationMode;
        return new MainWindowActionAvailability
        {
            CanSelectRemediation = remediationEnabled,
            CanSelectInstallation = IsRuntimeReady && !IsBusy,
            CanStartFix = remediationEnabled,
            CanSelectLowRiskFixes = remediationEnabled,
            CanRestoreBackup = remediationEnabled && hasBackups,
            CanStartInstallation = IsRuntimeReady
                && !IsBusy
                && IsInstallationMode
                && HasExecutableInstallationSelection
        };
    }
}

public sealed record MainWindowActionAvailability
{
    public required bool CanSelectRemediation { get; init; }

    public required bool CanSelectInstallation { get; init; }

    public required bool CanStartFix { get; init; }

    public required bool CanSelectLowRiskFixes { get; init; }

    public required bool CanRestoreBackup { get; init; }

    public required bool CanStartInstallation { get; init; }
}
