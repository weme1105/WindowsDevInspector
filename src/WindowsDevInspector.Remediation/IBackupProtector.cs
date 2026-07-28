namespace WindowsDevInspector.Remediation;

public interface IBackupProtector
{
    string Algorithm { get; }

    byte[] Protect(byte[] plaintext);

    byte[] Unprotect(byte[] ciphertext);
}
