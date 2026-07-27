namespace WindowsDevInspector.Core;

public sealed record TechnologyDefinition
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required IReadOnlySet<string> GroupIds { get; init; }

    public required IReadOnlySet<string> CheckIds { get; init; }

    public IReadOnlySet<string> Aliases { get; init; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
}
