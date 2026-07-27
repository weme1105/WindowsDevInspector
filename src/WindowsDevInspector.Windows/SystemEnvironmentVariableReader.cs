namespace WindowsDevInspector.Windows;

public sealed class SystemEnvironmentVariableReader : IEnvironmentVariableReader
{
    public string? GetEnvironmentVariable(string variable, EnvironmentVariableTarget target)
    {
        return Environment.GetEnvironmentVariable(variable, target);
    }
}
