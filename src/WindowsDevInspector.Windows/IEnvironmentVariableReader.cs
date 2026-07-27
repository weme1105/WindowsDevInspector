namespace WindowsDevInspector.Windows;

public interface IEnvironmentVariableReader
{
    string? GetEnvironmentVariable(string variable, EnvironmentVariableTarget target);
}
