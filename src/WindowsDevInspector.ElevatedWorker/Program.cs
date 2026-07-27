using WindowsDevInspector.Core;
using WindowsDevInspector.Remediation;

RemediationWhitelist whitelist = new([
    new RemediationDefinition
    {
        Id = "create-source-directory",
        DisplayName = "Create D:\\Source",
        Risk = RiskLevel.Low,
        RequiresElevation = false,
        RequiresRestart = false,
        SupportsRollback = true
    },
    new RemediationDefinition
    {
        Id = "enable-long-paths",
        DisplayName = "Enable Windows long paths",
        Risk = RiskLevel.Low,
        RequiresElevation = true,
        RequiresRestart = false,
        SupportsRollback = true
    }
]);

Console.WriteLine($"WindowsDevInspector ElevatedWorker loaded {whitelist.GetType().Name}.");
