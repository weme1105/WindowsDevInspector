namespace WindowsDevInspector.Remediation;

public sealed class DirectoryRemediationExecutor(
    RemediationWhitelist whitelist,
    IRemediationFileSystem fileSystem)
{
    private static readonly IReadOnlyDictionary<string, string> DirectoryTargets =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["create-source-directory"] = "D:\\Source",
            ["create-projects-directory"] = "D:\\Projects",
            ["create-gonote-directory"] = "D:\\GoNote"
        };

    public Task<RemediationExecutionResult> ExecuteAsync(
        string remediationId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        RemediationDefinition definition = whitelist.GetRequired(remediationId);

        if (definition.RequiresElevation)
        {
            return Task.FromResult(RemediationExecutionResult.Skip(
                remediationId,
                "This remediation requires the elevated worker, which is not implemented yet."));
        }

        if (!DirectoryTargets.TryGetValue(remediationId, out string? path))
        {
            return Task.FromResult(RemediationExecutionResult.Skip(
                remediationId,
                "This remediation is whitelisted but is not supported by the directory executor."));
        }

        if (fileSystem.DirectoryExists(path))
        {
            return Task.FromResult(RemediationExecutionResult.Success(
                remediationId,
                $"Directory already exists: {path}"));
        }

        fileSystem.CreateDirectory(path);

        return Task.FromResult(RemediationExecutionResult.Success(
            remediationId,
            $"Directory created: {path}"));
    }
}
