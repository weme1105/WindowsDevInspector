namespace WindowsDevInspector.Remediation;

public sealed class SystemRemediationFileSystem : IRemediationFileSystem
{
    public bool DirectoryExists(string path)
    {
        return Directory.Exists(path);
    }

    public void CreateDirectory(string path)
    {
        Directory.CreateDirectory(path);
    }

    public bool IsDirectoryEmpty(string path)
    {
        return !Directory.EnumerateFileSystemEntries(path).Any();
    }

    public void DeleteDirectory(string path)
    {
        Directory.Delete(path);
    }
}
