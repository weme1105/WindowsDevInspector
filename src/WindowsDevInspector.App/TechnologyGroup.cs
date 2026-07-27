using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WindowsDevInspector.App;

public sealed class TechnologyGroup(string id, string name, IEnumerable<TechnologyItem> technologies) : INotifyPropertyChanged
{
    private string displayName = name;
    private bool isExpanded;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Id { get; } = id;

    public string Name { get; } = name;

    public ObservableCollection<TechnologyItem> Technologies { get; } = new(technologies);

    public string DisplayName
    {
        get => displayName;
        set
        {
            if (displayName == value)
            {
                return;
            }

            displayName = value;
            OnPropertyChanged();
        }
    }

    public bool IsExpanded
    {
        get => isExpanded;
        set
        {
            if (isExpanded == value)
            {
                return;
            }

            isExpanded = value;
            OnPropertyChanged();
        }
    }

    public void ApplySearch(string query)
    {
        int matchCount = Technologies.Count(technology => IsMatch(technology, query));

        List<TechnologyItem> sorted = Technologies
            .OrderBy(technology => IsMatch(technology, query) ? 0 : 1)
            .ThenBy(technology => technology.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(technology => technology.Id, StringComparer.OrdinalIgnoreCase)
            .ToList();

        Technologies.Clear();

        foreach (TechnologyItem technology in sorted)
        {
            Technologies.Add(technology);
        }

        if (string.IsNullOrWhiteSpace(query))
        {
            DisplayName = Name;
            return;
        }

        DisplayName = $"{Name} ({matchCount})";

        if (matchCount > 0)
        {
            IsExpanded = true;
        }
    }

    private static bool IsMatch(TechnologyItem technology, string query)
    {
        return !string.IsNullOrWhiteSpace(query)
            && technology.Name.Contains(query, StringComparison.OrdinalIgnoreCase);
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
