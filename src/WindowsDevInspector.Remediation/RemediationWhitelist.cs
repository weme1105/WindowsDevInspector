namespace WindowsDevInspector.Remediation;

public sealed class RemediationWhitelist
{
    private readonly IReadOnlyDictionary<string, RemediationDefinition> definitions;

    public RemediationWhitelist(IEnumerable<RemediationDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);

        this.definitions = definitions.ToDictionary(
            definition => definition.Id,
            StringComparer.OrdinalIgnoreCase);
    }

    public bool IsAllowed(string remediationId)
    {
        return definitions.ContainsKey(remediationId);
    }

    public RemediationDefinition GetRequired(string remediationId)
    {
        if (definitions.TryGetValue(remediationId, out RemediationDefinition? definition))
        {
            return definition;
        }

        throw new InvalidOperationException($"Remediation '{remediationId}' is not whitelisted.");
    }
}
