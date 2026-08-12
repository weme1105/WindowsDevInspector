namespace WindowsDevInspector.Remediation;

public interface IExecutablePathResolver
{
    string? Resolve(string executableName);
}
