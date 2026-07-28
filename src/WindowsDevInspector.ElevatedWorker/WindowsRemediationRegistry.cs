using Microsoft.Win32;
using System.Security;
using WindowsDevInspector.Remediation;

namespace WindowsDevInspector.ElevatedWorker;

public sealed class WindowsRemediationRegistry : IRemediationRegistry
{
    public RegistryDwordValue ReadDword(string hive, string subKeyPath, string valueName)
    {
        try
        {
            using RegistryKey? baseKey = OpenBaseKey(hive);
            using RegistryKey? subKey = baseKey?.OpenSubKey(subKeyPath, writable: false);

            object? value = subKey?.GetValue(valueName);

            return value is int dword
                ? new RegistryDwordValue { Exists = true, Value = dword }
                : new RegistryDwordValue { Exists = false };
        }
        catch (Exception exception) when (exception is UnauthorizedAccessException or SecurityException or IOException)
        {
            return new RegistryDwordValue
            {
                Exists = false,
                ErrorMessage = exception.Message
            };
        }
    }

    public void WriteDword(string hive, string subKeyPath, string valueName, int value)
    {
        using RegistryKey baseKey = OpenBaseKey(hive)
            ?? throw new InvalidOperationException($"Unsupported registry hive '{hive}'.");
        using RegistryKey subKey = baseKey.CreateSubKey(subKeyPath, writable: true)
            ?? throw new InvalidOperationException($"Could not open or create registry key '{subKeyPath}'.");

        subKey.SetValue(valueName, value, RegistryValueKind.DWord);
    }

    public void DeleteValue(string hive, string subKeyPath, string valueName)
    {
        using RegistryKey baseKey = OpenBaseKey(hive)
            ?? throw new InvalidOperationException($"Unsupported registry hive '{hive}'.");
        using RegistryKey? subKey = baseKey.OpenSubKey(subKeyPath, writable: true);

        subKey?.DeleteValue(valueName, throwOnMissingValue: false);
    }

    private static RegistryKey? OpenBaseKey(string hive)
    {
        return hive.ToUpperInvariant() switch
        {
            "HKLM" or "HKEY_LOCAL_MACHINE" => Registry.LocalMachine,
            "HKCU" or "HKEY_CURRENT_USER" => Registry.CurrentUser,
            _ => null
        };
    }
}
