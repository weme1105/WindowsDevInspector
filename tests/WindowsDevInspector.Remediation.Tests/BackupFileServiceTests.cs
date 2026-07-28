using System.Text;

namespace WindowsDevInspector.Remediation.Tests;

public sealed class BackupFileServiceTests
{
    [Fact]
    public void WriteEncryptedBackup_DoesNotStorePlaintextBackupJson()
    {
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.backup.json");
        BackupFileService service = new(new ReversingBackupProtector(), "machine-a");
        string backupJson = """{"hive":"HKLM","subKeyPath":"SYSTEM\\CurrentControlSet\\Control\\FileSystem","valueName":"LongPathsEnabled","existed":true,"previousValue":0}""";

        try
        {
            service.WriteEncryptedBackup(path, backupJson);

            string fileContent = File.ReadAllText(path);
            Assert.DoesNotContain("LongPathsEnabled", fileContent);

            RegistryDwordBackup backup = service.ReadEncryptedRegistryBackup(path);
            Assert.Equal("HKLM", backup.Hive);
            Assert.Equal("LongPathsEnabled", backup.ValueName);
            Assert.Equal(0, backup.PreviousValue);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void ReadEncryptedRegistryBackup_RejectsWrongProtector()
    {
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.backup.json");
        BackupFileService writer = new(new ReversingBackupProtector(), "machine-a");
        BackupFileService reader = new(new FailingBackupProtector(), "machine-a");

        try
        {
            writer.WriteEncryptedBackup(path, """{"hive":"HKLM","subKeyPath":"SYSTEM\\CurrentControlSet\\Control\\FileSystem","valueName":"LongPathsEnabled","existed":false}""");

            Assert.Throws<InvalidOperationException>(() => reader.ReadEncryptedRegistryBackup(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void ReadEncryptedRegistryBackup_RejectsDifferentMachineFingerprint()
    {
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.backup.json");
        BackupFileService writer = new(new ReversingBackupProtector(), "machine-a");
        BackupFileService reader = new(new ReversingBackupProtector(), "machine-b");

        try
        {
            writer.WriteEncryptedBackup(path, """{"hive":"HKLM","subKeyPath":"SYSTEM\\CurrentControlSet\\Control\\FileSystem","valueName":"LongPathsEnabled","existed":false}""");

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => reader.ReadEncryptedRegistryBackup(path));

            Assert.Contains("different machine", exception.Message);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private sealed class ReversingBackupProtector : IBackupProtector
    {
        public string Algorithm => "test.reverse";

        public byte[] Protect(byte[] plaintext)
        {
            return plaintext.Reverse().ToArray();
        }

        public byte[] Unprotect(byte[] ciphertext)
        {
            return ciphertext.Reverse().ToArray();
        }
    }

    private sealed class FailingBackupProtector : IBackupProtector
    {
        public string Algorithm => "test.fail";

        public byte[] Protect(byte[] plaintext)
        {
            return Encoding.UTF8.GetBytes("fail");
        }

        public byte[] Unprotect(byte[] ciphertext)
        {
            throw new InvalidOperationException("Cannot decrypt.");
        }
    }
}
