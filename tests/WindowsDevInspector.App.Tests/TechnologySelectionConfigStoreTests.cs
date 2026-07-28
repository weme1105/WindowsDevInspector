using System.Text.Json;
using WindowsDevInspector.App;

namespace WindowsDevInspector.App.Tests;

public sealed class TechnologySelectionConfigStoreTests : IDisposable
{
    private readonly string testDirectory = Path.Combine(
        Path.GetTempPath(),
        "WindowsDevInspector.App.Tests",
        Guid.NewGuid().ToString("N"));

    [Fact]
    public void Load_ReturnsZeroWhenConfigDoesNotExist()
    {
        TechnologySelectionConfigStore store = CreateStore();
        TechnologyGroup[] groups = CreateGroups();

        int loadedCount = store.Load(groups);

        Assert.Equal(0, loadedCount);
        Assert.All(groups.SelectMany(group => group.Technologies), technology => Assert.False(technology.IsSelected));
    }

    [Fact]
    public void Load_ReturnsZeroWhenJsonIsMalformed()
    {
        TechnologySelectionConfigStore store = CreateStore();
        Directory.CreateDirectory(testDirectory);
        File.WriteAllText(store.ConfigPath, "{ not-json");
        TechnologyGroup[] groups = CreateGroups();

        int loadedCount = store.Load(groups);

        Assert.Equal(0, loadedCount);
        Assert.All(groups.SelectMany(group => group.Technologies), technology => Assert.False(technology.IsSelected));
    }

    [Fact]
    public void Load_SelectsKnownIdsAndIgnoresUnknownDuplicatesAndBlankValues()
    {
        TechnologySelectionConfigStore store = CreateStore();
        Directory.CreateDirectory(testDirectory);
        File.WriteAllText(store.ConfigPath, """
            {
              "selectedTechnologyIds": [
                "dotnet",
                "DOTNET",
                "unknown",
                "",
                "go"
              ]
            }
            """);
        TechnologyGroup[] groups = CreateGroups();

        int loadedCount = store.Load(groups);

        Assert.Equal(2, loadedCount);
        Assert.True(FindTechnology(groups, "dotnet").IsSelected);
        Assert.True(FindTechnology(groups, "go").IsSelected);
        Assert.False(FindTechnology(groups, "python").IsSelected);
    }

    [Fact]
    public void Load_ClearsSelectionsThatAreNotInConfig()
    {
        TechnologySelectionConfigStore store = CreateStore();
        Directory.CreateDirectory(testDirectory);
        File.WriteAllText(store.ConfigPath, """
            {
              "selectedTechnologyIds": [
                "go"
              ]
            }
            """);
        TechnologyGroup[] groups = CreateGroups();
        FindTechnology(groups, "dotnet").IsSelected = true;
        FindTechnology(groups, "python").IsSelected = true;

        int loadedCount = store.Load(groups);

        Assert.Equal(1, loadedCount);
        Assert.False(FindTechnology(groups, "dotnet").IsSelected);
        Assert.True(FindTechnology(groups, "go").IsSelected);
        Assert.False(FindTechnology(groups, "python").IsSelected);
    }

    [Fact]
    public void Save_WritesDistinctSelectedIdsSortedByTechnologyName()
    {
        TechnologySelectionConfigStore store = CreateStore();
        TechnologyGroup[] groups =
        [
            new("backend", "Backend", [
                new TechnologyItem("go", "Go", isSelected: true),
                new TechnologyItem("dotnet", ".NET", isSelected: true),
                new TechnologyItem("python", "Python")
            ]),
            new("desktop", "Desktop", [
                new TechnologyItem("DOTNET", ".NET SDK", isSelected: true)
            ])
        ];

        int savedCount = store.Save(groups);

        Assert.Equal(2, savedCount);
        string json = File.ReadAllText(store.ConfigPath);
        TechnologySelectionConfigModel? config = JsonSerializer.Deserialize<TechnologySelectionConfigModel>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        Assert.NotNull(config);
        Assert.Equal(["dotnet", "go"], config.SelectedTechnologyIds);
        Assert.Contains("\"selectedTechnologyIds\"", json, StringComparison.Ordinal);
    }

    private TechnologySelectionConfigStore CreateStore()
    {
        return new TechnologySelectionConfigStore(Path.Combine(testDirectory, "WindowsDevInspector.cfg"));
    }

    private static TechnologyGroup[] CreateGroups()
    {
        return
        [
            new("backend", "Backend", [
                new TechnologyItem("dotnet", ".NET"),
                new TechnologyItem("go", "Go"),
                new TechnologyItem("python", "Python")
            ])
        ];
    }

    private static TechnologyItem FindTechnology(IEnumerable<TechnologyGroup> groups, string id)
    {
        return groups
            .SelectMany(group => group.Technologies)
            .Single(technology => technology.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    }

    public void Dispose()
    {
        if (Directory.Exists(testDirectory))
        {
            Directory.Delete(testDirectory, recursive: true);
        }
    }
}
