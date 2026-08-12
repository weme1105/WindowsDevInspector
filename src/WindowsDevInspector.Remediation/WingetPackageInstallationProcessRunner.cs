namespace WindowsDevInspector.Remediation;

public sealed class WingetPackageInstallationProcessRunner(
    IPackageInstallationCommandRunner commandRunner) : IPackageInstallationProcessRunner
{
    public async Task<PackageInstallationProcessResult> RunAsync(
        PackageInstallationCommandPreview command,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        ValidateCommand(command);

        PackageInstallationCommandRunResult runResult = await commandRunner.RunAsync(
            command.FileName,
            command.Arguments,
            timeout,
            cancellationToken);

        if (runResult.TimedOut)
        {
            return Result(PackageInstallationOutcome.TimedOut, runResult, "winget installation timed out.");
        }

        if (!string.IsNullOrWhiteSpace(runResult.ErrorMessage))
        {
            return Result(PackageInstallationOutcome.Failed, runResult, runResult.ErrorMessage);
        }

        return runResult.ExitCode == 0
            ? Result(PackageInstallationOutcome.Succeeded, runResult, "winget reported successful installation.")
            : Result(
                PackageInstallationOutcome.Failed,
                runResult,
                $"winget exited with code {runResult.ExitCode?.ToString() ?? "unknown"}.");
    }

    private static void ValidateCommand(PackageInstallationCommandPreview command)
    {
        ArgumentNullException.ThrowIfNull(command);

        string[] expectedArguments =
        [
            "install",
            "--id",
            command.PackageId,
            "--exact",
            "--source",
            "winget",
            "--accept-package-agreements",
            "--accept-source-agreements"
        ];

        if (command.Source != InstallationSource.Winget
            || !command.FileName.Equals("winget", StringComparison.OrdinalIgnoreCase)
            || !command.Arguments.SequenceEqual(expectedArguments, StringComparer.Ordinal))
        {
            throw new InvalidOperationException("The winget installation command does not match the approved fixed-token contract.");
        }
    }

    private static PackageInstallationProcessResult Result(
        PackageInstallationOutcome outcome,
        PackageInstallationCommandRunResult runResult,
        string message)
    {
        return new PackageInstallationProcessResult
        {
            Outcome = outcome,
            ExitCode = runResult.ExitCode,
            Message = message
        };
    }
}
