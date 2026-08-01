using WindowsDevInspector.App;

namespace WindowsDevInspector.App.Tests;

public sealed class TechnologySelectionToggleTests
{
    [Fact]
    public void GetButtonLabel_ReturnsSelectAllWhenAnyTechnologyIsUnselected()
    {
        TechnologyGroup[] groups = CreateGroups();
        FindTechnology(groups, "dotnet").IsSelected = true;

        string label = TechnologySelectionToggle.GetButtonLabel(groups);

        Assert.Equal("勾選全部", label);
    }

    [Fact]
    public void GetButtonLabel_ReturnsClearAllWhenAllTechnologiesAreSelected()
    {
        TechnologyGroup[] groups = CreateGroups();
        foreach (TechnologyItem technology in groups.SelectMany(group => group.Technologies))
        {
            technology.IsSelected = true;
        }

        string label = TechnologySelectionToggle.GetButtonLabel(groups);

        Assert.Equal("取消勾選全部", label);
    }

    [Fact]
    public void Toggle_SelectsAllWhenAnyTechnologyIsUnselected()
    {
        TechnologyGroup[] groups = CreateGroups();
        FindTechnology(groups, "dotnet").IsSelected = true;

        TechnologySelectionToggleResult result = TechnologySelectionToggle.Toggle(groups);

        Assert.Equal(TechnologySelectionToggleAction.SelectedAll, result.Action);
        Assert.Equal(2, result.ChangedCount);
        Assert.Equal("取消勾選全部", result.NextButtonLabel);
        Assert.All(groups.SelectMany(group => group.Technologies), technology => Assert.True(technology.IsSelected));
    }

    [Fact]
    public void Toggle_ClearsAllWhenAllTechnologiesAreSelected()
    {
        TechnologyGroup[] groups = CreateGroups();
        foreach (TechnologyItem technology in groups.SelectMany(group => group.Technologies))
        {
            technology.IsSelected = true;
        }

        TechnologySelectionToggleResult result = TechnologySelectionToggle.Toggle(groups);

        Assert.Equal(TechnologySelectionToggleAction.ClearedAll, result.Action);
        Assert.Equal(3, result.ChangedCount);
        Assert.Equal("勾選全部", result.NextButtonLabel);
        Assert.All(groups.SelectMany(group => group.Technologies), technology => Assert.False(technology.IsSelected));
    }

    private static TechnologyGroup[] CreateGroups()
    {
        return
        [
            new("backend", "Backend", [
                new TechnologyItem("dotnet", ".NET"),
                new TechnologyItem("csharp", "C#")
            ]),
            new("devops", "DevOps", [
                new TechnologyItem("git", "Git")
            ])
        ];
    }

    private static TechnologyItem FindTechnology(IEnumerable<TechnologyGroup> groups, string id)
    {
        return groups
            .SelectMany(group => group.Technologies)
            .Single(technology => technology.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    }
}
