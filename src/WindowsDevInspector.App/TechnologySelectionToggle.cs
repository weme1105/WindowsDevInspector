namespace WindowsDevInspector.App;

public static class TechnologySelectionToggle
{
    public const string SelectAllLabel = "勾選全部";

    public const string ClearAllLabel = "取消勾選全部";

    public static string GetButtonLabel(IEnumerable<TechnologyGroup> groups)
    {
        return AreAllSelected(groups) ? ClearAllLabel : SelectAllLabel;
    }

    public static TechnologySelectionToggleResult Toggle(IEnumerable<TechnologyGroup> groups)
    {
        TechnologyItem[] technologies = groups
            .SelectMany(group => group.Technologies)
            .ToArray();

        bool shouldSelectAll = technologies.Any(technology => !technology.IsSelected);
        int changedCount = 0;

        foreach (TechnologyItem technology in technologies)
        {
            if (technology.IsSelected == shouldSelectAll)
            {
                continue;
            }

            technology.IsSelected = shouldSelectAll;
            changedCount++;
        }

        return new TechnologySelectionToggleResult(
            shouldSelectAll ? TechnologySelectionToggleAction.SelectedAll : TechnologySelectionToggleAction.ClearedAll,
            changedCount,
            GetButtonLabel(groups));
    }

    private static bool AreAllSelected(IEnumerable<TechnologyGroup> groups)
    {
        TechnologyItem[] technologies = groups
            .SelectMany(group => group.Technologies)
            .ToArray();

        return technologies.Length > 0
            && technologies.All(technology => technology.IsSelected);
    }
}

public sealed record TechnologySelectionToggleResult(
    TechnologySelectionToggleAction Action,
    int ChangedCount,
    string NextButtonLabel);

public enum TechnologySelectionToggleAction
{
    SelectedAll,
    ClearedAll
}
