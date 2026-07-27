using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WindowsDevInspector.App;

public sealed class TechnologyItem(string id, string name, bool isSelected = false) : INotifyPropertyChanged
{
    private bool isSelected = isSelected;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Id { get; } = id;

    public string Name { get; } = name;

    public int OriginalIndex { get; init; }

    public bool IsSelected
    {
        get => isSelected;
        set
        {
            if (isSelected == value)
            {
                return;
            }

            isSelected = value;
            OnPropertyChanged();
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
