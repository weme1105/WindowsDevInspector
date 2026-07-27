using Microsoft.Win32;
using System.Security;

namespace WindowsDevInspector.Windows;

public sealed class WindowsRegistryReader : IRegistryReader
{
    public RegistryDwordReadResult ReadDword(string hive, string subKeyPath, string valueName)
    {
        try
        {
            using RegistryKey? baseKey = OpenBaseKey(hive);
            using RegistryKey? subKey = baseKey?.OpenSubKey(subKeyPath, writable: false);

            object? value = subKey?.GetValue(valueName);

            return value is int dword
                ? new RegistryDwordReadResult { Exists = true, Value = dword }
                : new RegistryDwordReadResult { Exists = false };
        }
        catch (Exception exception) when (exception is UnauthorizedAccessException or SecurityException or IOException)
        {
            return new RegistryDwordReadResult
            {
                Exists = false,
                ErrorMessage = exception.Message
            };
        }
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
