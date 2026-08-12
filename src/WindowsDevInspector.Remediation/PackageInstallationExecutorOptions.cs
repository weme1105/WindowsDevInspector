namespace WindowsDevInspector.Remediation;

public sealed record PackageInstallationExecutorOptions
{
    public bool ExecutionEnabled { get; init; }

    public TimeSpan Timeout { get; init; } = TimeSpan.FromMinutes(5);

    public TimeSpan VerificationTimeout { get; init; } = TimeSpan.FromSeconds(30);
}
