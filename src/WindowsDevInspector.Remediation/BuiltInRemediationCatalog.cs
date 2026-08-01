using WindowsDevInspector.Core;

namespace WindowsDevInspector.Remediation;

public static class BuiltInRemediationCatalog
{
    public static RemediationWhitelist CreateWhitelist()
    {
        return new RemediationWhitelist(CreateDefinitions());
    }

    public static IReadOnlyList<RemediationDefinition> CreateDefinitions()
    {
        return [
            Directory("create-source-directory", "Create D:\\Source", "Creates the standard source repository directory.", false),
            Directory("create-projects-directory", "Create D:\\Projects", "Creates the standard experiments and generated outputs directory.", false),
            Directory("create-note-directory", "Create D:\\Note", "Creates the standard learning notes and diagnostics directory.", false),
            Registry("enable-long-paths", "Enable Windows long paths", "Enables long path support through the approved registry value."),
            Registry("enable-developer-mode", "Enable Developer Mode", "Enables Developer Mode through the approved registry value.")
        ];
    }

    private static RemediationDefinition Directory(string id, string displayName, string description, bool requiresElevation)
    {
        return new RemediationDefinition
        {
            Id = id,
            DisplayName = displayName,
            Description = description,
            Risk = RiskLevel.Low,
            RequiresElevation = requiresElevation,
            RequiresRestart = false,
            SupportsRollback = true
        };
    }

    private static RemediationDefinition Registry(string id, string displayName, string description)
    {
        return new RemediationDefinition
        {
            Id = id,
            DisplayName = displayName,
            Description = description,
            Risk = RiskLevel.Low,
            RequiresElevation = true,
            RequiresRestart = false,
            SupportsRollback = true
        };
    }
}
