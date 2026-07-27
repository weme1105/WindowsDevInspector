namespace WindowsDevInspector.Windows;

public interface IRegistryReader
{
    RegistryDwordReadResult ReadDword(string hive, string subKeyPath, string valueName);
}
