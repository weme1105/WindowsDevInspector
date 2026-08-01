using System.Security.Cryptography;
using System.Text;

namespace WindowsDevInspector.Remediation;

[System.Runtime.Versioning.SupportedOSPlatform("windows")]
public sealed class DpapiBackupProtector : IBackupProtector
{
    private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("WindowsDevInspector.Backup.v1");

    public string Algorithm => "DPAPI.CurrentUser.v1";

    public byte[] Protect(byte[] plaintext)
    {
        return ProtectedData.Protect(plaintext, Entropy, DataProtectionScope.CurrentUser);
    }

    public byte[] Unprotect(byte[] ciphertext)
    {
        return ProtectedData.Unprotect(ciphertext, Entropy, DataProtectionScope.CurrentUser);
    }
}
