namespace WindowsDevInspector.Remediation;

public sealed record EncryptedBackupEnvelope
{
    public required int Version { get; init; }

    public required string Algorithm { get; init; }

    public required string MachineFingerprint { get; init; }

    public required string CipherText { get; init; }
}
