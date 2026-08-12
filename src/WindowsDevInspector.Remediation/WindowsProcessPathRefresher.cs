namespace WindowsDevInspector.Remediation;

public sealed class WindowsProcessPathRefresher : IProcessPathRefresher
{
    public void Refresh()
    {
        string? machinePath = Environment.GetEnvironmentVariable(
            "Path",
            EnvironmentVariableTarget.Machine);
        string? userPath = Environment.GetEnvironmentVariable(
            "Path",
            EnvironmentVariableTarget.User);
        string refreshedPath = string.Join(
            Path.PathSeparator,
            new[] { machinePath, userPath }.Where(value => !string.IsNullOrWhiteSpace(value)));

        Environment.SetEnvironmentVariable("Path", refreshedPath, EnvironmentVariableTarget.Process);
    }
}
