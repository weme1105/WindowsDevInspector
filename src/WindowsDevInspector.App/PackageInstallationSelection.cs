namespace WindowsDevInspector.App;

public static class PackageInstallationSelection
{
    public static bool SelectSingle(
        IEnumerable<CheckResultRow> results,
        CheckResultRow selectedResult)
    {
        ArgumentNullException.ThrowIfNull(results);
        ArgumentNullException.ThrowIfNull(selectedResult);

        if (!selectedResult.IsInstallationCandidate)
        {
            Clear(results);
            return false;
        }

        foreach (CheckResultRow result in results)
        {
            result.SetFixSelectionState(isSelected: false, isEnabled: false);
        }

        foreach (CheckResultRow result in results)
        {
            if (!result.IsInstallationCandidate)
            {
                continue;
            }

            bool isSelected = ReferenceEquals(result, selectedResult);
            result.SetInstallationSelectionState(isSelected, isSelected);
        }

        return true;
    }

    public static void Clear(IEnumerable<CheckResultRow> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        foreach (CheckResultRow result in results)
        {
            result.SetInstallationSelectionState(isSelected: false, isEnabled: result.IsInstallationCandidate);
            result.SetFixSelectionState(isSelected: result.IsSelectedForFix, isEnabled: true);
        }
    }

    public static CheckResultRow? GetSelected(IEnumerable<CheckResultRow> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        return results.SingleOrDefault(result => result.IsSelectedForInstallation);
    }
}
