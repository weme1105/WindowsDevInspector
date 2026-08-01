namespace WindowsDevInspector.Remediation;

public interface IRemediationFileSystem
{
    bool DirectoryExists(string path);

    void CreateDirectory(string path);

    bool IsDirectoryEmpty(string path);

    void DeleteDirectory(string path);
}
