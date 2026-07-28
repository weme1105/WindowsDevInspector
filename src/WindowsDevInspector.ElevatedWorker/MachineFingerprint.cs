using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;

namespace WindowsDevInspector.ElevatedWorker;

public static class MachineFingerprint
{
    public static string CreateFromMacAddresses()
    {
        string[] macAddresses = NetworkInterface
            .GetAllNetworkInterfaces()
            .Where(adapter => adapter.OperationalStatus == OperationalStatus.Up)
            .Select(adapter => adapter.GetPhysicalAddress().ToString())
            .Where(address => !string.IsNullOrWhiteSpace(address))
            .Select(NormalizeMacAddress)
            .Where(address => address.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        string fingerprintSource = macAddresses.Length == 0
            ? "NO_ACTIVE_MAC"
            : string.Join('|', macAddresses);

        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(fingerprintSource));
        return Convert.ToHexString(hash);
    }

    private static string NormalizeMacAddress(string value)
    {
        return new string(value
            .Where(char.IsAsciiHexDigit)
            .Select(char.ToUpperInvariant)
            .ToArray());
    }
}
