namespace WindowsDevInspector.Windows;

public sealed class SystemFileSystem : IFileSystem
{
    public bool DirectoryExists(string path)
    {
        return Directory.Exists(path);
    }
}
