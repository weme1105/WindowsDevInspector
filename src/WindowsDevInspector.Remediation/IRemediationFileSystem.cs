namespace WindowsDevInspector.Remediation;

public interface IRemediationFileSystem
{
    bool DirectoryExists(string path);

    void CreateDirectory(string path);
}
