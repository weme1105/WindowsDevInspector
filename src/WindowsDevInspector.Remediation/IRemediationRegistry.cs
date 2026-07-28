namespace WindowsDevInspector.Remediation;

public interface IRemediationRegistry
{
    RegistryDwordValue ReadDword(string hive, string subKeyPath, string valueName);

    void WriteDword(string hive, string subKeyPath, string valueName, int value);

    void DeleteValue(string hive, string subKeyPath, string valueName);
}
