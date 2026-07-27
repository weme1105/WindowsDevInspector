using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WindowsDevInspector.App;

public static class TechnologySelectionConfig
{
    public const string FileName = "WindowsDevInspector.cfg";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string ConfigPath => Path.Combine(AppContext.BaseDirectory, FileName);

    public static int Load(IReadOnlyCollection<TechnologyGroup> groups)
    {
        if (!File.Exists(ConfigPath))
        {
            return 0;
        }

        TechnologySelectionConfigModel? config;

        try
        {
            string json = File.ReadAllText(ConfigPath);
            config = JsonSerializer.Deserialize<TechnologySelectionConfigModel>(json, SerializerOptions);
        }
        catch (JsonException)
        {
            return 0;
        }
        catch (IOException)
        {
            return 0;
        }
        catch (UnauthorizedAccessException)
        {
            return 0;
        }

        if (config?.SelectedTechnologyIds is null)
        {
            return 0;
        }

        HashSet<string> selectedIds = config.SelectedTechnologyIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        HashSet<string> matchedIds = new(StringComparer.OrdinalIgnoreCase);

        foreach (TechnologyItem technology in groups.SelectMany(group => group.Technologies))
        {
            bool isSelected = selectedIds.Contains(technology.Id);
            technology.IsSelected = isSelected;

            if (isSelected)
            {
                matchedIds.Add(technology.Id);
            }
        }

        return matchedIds.Count;
    }

    public static int Save(IReadOnlyCollection<TechnologyGroup> groups)
    {
        string[] selectedTechnologyIds = groups
            .SelectMany(group => group.Technologies)
            .Where(technology => technology.IsSelected)
            .Select(technology => technology.Id)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        TechnologySelectionConfigModel config = new()
        {
            SelectedTechnologyIds = selectedTechnologyIds
        };

        string json = JsonSerializer.Serialize(config, SerializerOptions);
        File.WriteAllText(ConfigPath, json);

        return selectedTechnologyIds.Length;
    }
}

public sealed class TechnologySelectionConfigModel
{
    public string[] SelectedTechnologyIds { get; init; } = [];
}
