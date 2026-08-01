using System.Text;
using System.Text.Json;

namespace WindowsDevInspector.Remediation;

public sealed class BackupFileService(IBackupProtector protector, string machineFingerprint)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public void WriteEncryptedBackup(string path, string backupJson)
    {
        byte[] plaintext = Encoding.UTF8.GetBytes(backupJson);
        byte[] ciphertext = protector.Protect(plaintext);

        EncryptedBackupEnvelope envelope = new()
        {
            Version = 1,
            Algorithm = protector.Algorithm,
            MachineFingerprint = machineFingerprint,
            CipherText = Convert.ToBase64String(ciphertext)
        };

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(envelope, JsonOptions), Encoding.UTF8);
    }

    public RegistryDwordBackup ReadEncryptedRegistryBackup(string path)
    {
        string backupJson = ReadEncryptedBackupJson(path);

        return JsonSerializer.Deserialize<RegistryDwordBackup>(backupJson, JsonOptions)
            ?? throw new InvalidOperationException("Decrypted backup content is not a valid registry backup.");
    }

    public DirectoryBackup ReadEncryptedDirectoryBackup(string path)
    {
        string backupJson = ReadEncryptedBackupJson(path);

        return JsonSerializer.Deserialize<DirectoryBackup>(backupJson, JsonOptions)
            ?? throw new InvalidOperationException("Decrypted backup content is not a valid directory backup.");
    }

    private string ReadEncryptedBackupJson(string path)
    {
        string envelopeJson = File.ReadAllText(path, Encoding.UTF8);
        EncryptedBackupEnvelope envelope = JsonSerializer.Deserialize<EncryptedBackupEnvelope>(envelopeJson, JsonOptions)
            ?? throw new InvalidOperationException("Backup file is not a valid encrypted backup envelope.");

        if (envelope.Version != 1)
        {
            throw new InvalidOperationException($"Unsupported backup version '{envelope.Version}'.");
        }

        if (!string.Equals(envelope.Algorithm, protector.Algorithm, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Backup encryption algorithm does not match this worker.");
        }

        if (!string.Equals(envelope.MachineFingerprint, machineFingerprint, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Backup was created on a different machine.");
        }

        byte[] ciphertext = Convert.FromBase64String(envelope.CipherText);
        byte[] plaintext = protector.Unprotect(ciphertext);
        return Encoding.UTF8.GetString(plaintext);
    }
}
