namespace WindowsDevInspector.Remediation.Tests;

public sealed class PackageInstallationVerifierTests
{
    [Fact]
    public async Task VerifyAsync_UsesOnlyCatalogOwnedExecutableAndArguments()
    {
        RecordingCommandRunner runner = new(new PackageInstallationCommandRunResult
        {
            ExitCode = 0,
            StandardOutput = "v1.2.3",
            StandardError = string.Empty
        });
        PackageInstallationVerifier verifier = new(runner, new FixedResolver(@"C:\Tools\kubectl.exe"));
        ApprovedInstallationPackage package = BuiltInInstallationCatalog.Create()
            .GetRequired("Kubernetes.kubectl", InstallationSource.Winget);

        PackageInstallationVerificationResult result = await verifier.VerifyAsync(
            package,
            TimeSpan.FromSeconds(30),
            CancellationToken.None);

        Assert.Equal(PackageInstallationVerificationOutcome.Succeeded, result.Outcome);
        Assert.Equal(@"C:\Tools\kubectl.exe", runner.FileName);
        Assert.Equal(["version", "--client"], runner.Arguments);
        Assert.Equal(TimeSpan.FromSeconds(30), runner.Timeout);
    }

    [Fact]
    public async Task VerifyAsync_MissingExecutable_ReturnsFailedWithoutRawOutput()
    {
        RecordingCommandRunner runner = new(new PackageInstallationCommandRunResult
        {
            ExitCode = null,
            StandardOutput = "sensitive output must not propagate",
            StandardError = "sensitive error must not propagate",
            ErrorMessage = "Executable was not found."
        });
        PackageInstallationVerifier verifier = new(runner, new FixedResolver(@"C:\Tools\pnpm.exe"));

        PackageInstallationVerificationResult result = await verifier.VerifyAsync(
            Package("pnpm", ["--version"]),
            TimeSpan.FromSeconds(30),
            CancellationToken.None);

        Assert.Equal(PackageInstallationVerificationOutcome.Failed, result.Outcome);
        Assert.Contains("could not run", result.Message);
        Assert.DoesNotContain("sensitive", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task VerifyAsync_Timeout_ReturnsTimedOut()
    {
        PackageInstallationVerifier verifier = new(new RecordingCommandRunner(
            new PackageInstallationCommandRunResult
            {
                ExitCode = null,
                TimedOut = true,
                StandardOutput = string.Empty,
                StandardError = string.Empty
            }), new FixedResolver(@"C:\Tools\terraform.exe"));

        PackageInstallationVerificationResult result = await verifier.VerifyAsync(
            Package("terraform", ["version"]),
            TimeSpan.FromMilliseconds(1),
            CancellationToken.None);

        Assert.Equal(PackageInstallationVerificationOutcome.TimedOut, result.Outcome);
    }

    [Fact]
    public async Task VerifyAsync_PropagatesCancellation()
    {
        PackageInstallationVerifier verifier = new(
            new CancellingCommandRunner(),
            new FixedResolver(@"C:\Tools\terraform.exe"));
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() => verifier.VerifyAsync(
            Package("terraform", ["version"]),
            TimeSpan.FromSeconds(30),
            cancellation.Token));
    }

    [Fact]
    public async Task VerifyAsync_BatchShim_UsesSystemCmdWithFixedSafeTokens()
    {
        RecordingCommandRunner runner = new(new PackageInstallationCommandRunResult
        {
            ExitCode = 0,
            StandardOutput = "9.0.0",
            StandardError = string.Empty
        });
        PackageInstallationVerifier verifier = new(
            runner,
            new FixedResolver(@"C:\Program Files\pnpm\pnpm.cmd"));

        PackageInstallationVerificationResult result = await verifier.VerifyAsync(
            Package("pnpm", ["--version"]),
            TimeSpan.FromSeconds(30),
            CancellationToken.None);

        Assert.Equal(PackageInstallationVerificationOutcome.Succeeded, result.Outcome);
        Assert.Equal(Path.Combine(Environment.SystemDirectory, "cmd.exe"), runner.FileName);
        Assert.Equal(
            ["/d", "/s", "/c", "\"\"C:\\Program Files\\pnpm\\pnpm.cmd\" \"--version\"\""],
            runner.Arguments);
    }

    [Fact]
    public async Task VerifyAsync_ExecutableMissingFromRefreshedPath_DoesNotStartProcess()
    {
        RecordingCommandRunner runner = new(new PackageInstallationCommandRunResult
        {
            ExitCode = 0,
            StandardOutput = string.Empty,
            StandardError = string.Empty
        });
        PackageInstallationVerifier verifier = new(runner, new FixedResolver(null));

        PackageInstallationVerificationResult result = await verifier.VerifyAsync(
            Package("az", ["version"]),
            TimeSpan.FromSeconds(30),
            CancellationToken.None);

        Assert.Equal(PackageInstallationVerificationOutcome.Failed, result.Outcome);
        Assert.Contains("not found", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Null(runner.FileName);
    }

    private static ApprovedInstallationPackage Package(
        string executable,
        IReadOnlyList<string> arguments) => new()
    {
        DiagnosticCheckId = "test.check",
        AvailabilityCheckId = "test.availability",
        PackageId = "Test.Package",
        DisplayName = "Test package",
        Source = InstallationSource.Winget,
        Risk = WindowsDevInspector.Core.RiskLevel.Medium,
        VerificationExecutable = executable,
        VerificationArguments = arguments
    };

    private sealed class RecordingCommandRunner(PackageInstallationCommandRunResult result)
        : IPackageInstallationCommandRunner
    {
        public string? FileName { get; private set; }

        public IReadOnlyList<string>? Arguments { get; private set; }

        public TimeSpan? Timeout { get; private set; }

        public Task<PackageInstallationCommandRunResult> RunAsync(
            string fileName,
            IReadOnlyList<string> arguments,
            TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            FileName = fileName;
            Arguments = arguments;
            Timeout = timeout;
            return Task.FromResult(result);
        }
    }

    private sealed class CancellingCommandRunner : IPackageInstallationCommandRunner
    {
        public Task<PackageInstallationCommandRunResult> RunAsync(
            string fileName,
            IReadOnlyList<string> arguments,
            TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            throw new InvalidOperationException("Expected cancellation before this point.");
        }
    }

    private sealed class FixedResolver(string? path) : IExecutablePathResolver
    {
        public string? Resolve(string executableName) => path;
    }
}
