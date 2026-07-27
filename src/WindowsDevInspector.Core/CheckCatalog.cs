namespace WindowsDevInspector.Core;

public sealed class CheckCatalog(
    IEnumerable<TechnologyDefinition> technologies,
    IEnumerable<CheckDefinition> checks)
{
    private readonly IReadOnlyDictionary<string, TechnologyDefinition> technologiesById =
        technologies.ToDictionary(technology => technology.Id, StringComparer.OrdinalIgnoreCase);

    private readonly IReadOnlyDictionary<string, CheckDefinition> checksById =
        checks.ToDictionary(check => check.Id, StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<TechnologyDefinition> Technologies => technologiesById.Values.ToArray();

    public IReadOnlyCollection<CheckDefinition> Checks => checksById.Values.ToArray();

    public IReadOnlyList<CheckDefinition> ResolveChecks(IEnumerable<string> selectedTechnologyIds, bool includeCommonChecks = true)
    {
        ArgumentNullException.ThrowIfNull(selectedTechnologyIds);

        HashSet<string> checkIds = new(StringComparer.OrdinalIgnoreCase);

        if (includeCommonChecks)
        {
            foreach (CheckDefinition check in checksById.Values.Where(check => check.Category.Equals("Common", StringComparison.OrdinalIgnoreCase)))
            {
                checkIds.Add(check.Id);
            }
        }

        foreach (string technologyId in selectedTechnologyIds.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!technologiesById.TryGetValue(technologyId, out TechnologyDefinition? technology))
            {
                continue;
            }

            foreach (string checkId in technology.CheckIds)
            {
                checkIds.Add(checkId);
            }
        }

        return checkIds
            .Select(checkId => checksById.GetValueOrDefault(checkId))
            .Where(check => check is not null)
            .Select(check => check!)
            .OrderBy(check => check.Category, StringComparer.OrdinalIgnoreCase)
            .ThenBy(check => check.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
