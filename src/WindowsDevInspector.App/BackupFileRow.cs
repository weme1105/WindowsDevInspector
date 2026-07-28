using System.IO;

namespace WindowsDevInspector.App;

public sealed class BackupFileRow
{
    public BackupFileRow(FileInfo file)
    {
        FullPath = file.FullName;
        DisplayName = $"{file.LastWriteTime:yyyy-MM-dd HH:mm:ss} - {file.Name}";
    }

    public string DisplayName { get; }

    public string FullPath { get; }
}
