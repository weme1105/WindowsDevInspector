namespace WindowsDevInspector.App;

public static class ResultFixSelection
{
    public static int SelectLowRiskSupportedFixes(IEnumerable<CheckResultRow> results)
    {
        int selectedCount = 0;

        foreach (CheckResultRow result in results)
        {
            bool shouldSelect = result.IsLowRiskSupportedFix;
            result.IsSelectedForFix = shouldSelect;

            if (shouldSelect)
            {
                selectedCount++;
            }
        }

        return selectedCount;
    }

    public static CheckResultRow[] GetSelectedFixes(IEnumerable<CheckResultRow> results)
    {
        return results
            .Where(result => result.IsSelectedForFix && result.IsFixSelectable && result.RemediationId is not null)
            .ToArray();
    }
}
