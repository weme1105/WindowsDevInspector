namespace WindowsDevInspector.Remediation;

public sealed class PackageInstallationVerifier(
    IPackageInstallationCommandRunner commandRunner,
    IExecutablePathResolver executablePathResolver) : IPackageInstallationVerifier
{
    public async Task<PackageInstallationVerificationResult> VerifyAsync(
        ApprovedInstallationPackage package,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(package);

        string? executablePath = executablePathResolver.Resolve(package.VerificationExecutable);
        if (executablePath is null)
        {
            return new PackageInstallationVerificationResult
            {
                Outcome = PackageInstallationVerificationOutcome.Failed,
                Message = $"Verification executable '{package.VerificationExecutable}' was not found on refreshed PATH."
            };
        }

        (string fileName, IReadOnlyList<string> arguments) = BuildCommand(executablePath, package.VerificationArguments);
        PackageInstallationCommandRunResult result = await commandRunner.RunAsync(
            fileName,
            arguments,
            timeout,
            cancellationToken);

        if (result.TimedOut)
        {
            return new PackageInstallationVerificationResult
            {
                Outcome = PackageInstallationVerificationOutcome.TimedOut,
                ExitCode = null,
                Message = $"Verification command '{package.VerificationExecutable}' timed out."
            };
        }

        if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
        {
            return new PackageInstallationVerificationResult
            {
                Outcome = PackageInstallationVerificationOutcome.Failed,
                ExitCode = result.ExitCode,
                Message = $"Verification command '{package.VerificationExecutable}' could not run: {result.ErrorMessage}"
            };
        }

        return result.ExitCode == 0
            ? new PackageInstallationVerificationResult
            {
                Outcome = PackageInstallationVerificationOutcome.Succeeded,
                ExitCode = 0,
                Message = $"Verification command '{package.VerificationExecutable}' succeeded."
            }
            : new PackageInstallationVerificationResult
            {
                Outcome = PackageInstallationVerificationOutcome.Failed,
                ExitCode = result.ExitCode,
                Message = $"Verification command '{package.VerificationExecutable}' exited with code {result.ExitCode?.ToString() ?? "unknown"}."
            };
    }

    private static (string FileName, IReadOnlyList<string> Arguments) BuildCommand(
        string executablePath,
        IReadOnlyList<string> verificationArguments)
    {
        string extension = Path.GetExtension(executablePath);
        if (!extension.Equals(".cmd", StringComparison.OrdinalIgnoreCase)
            && !extension.Equals(".bat", StringComparison.OrdinalIgnoreCase))
        {
            return (executablePath, verificationArguments);
        }

        if (verificationArguments.Any(argument => !IsSafeBatchArgument(argument)))
        {
            throw new InvalidOperationException("Catalog-owned batch verification arguments contain unsupported shell metacharacters.");
        }

        string commandBody = string.Join(
            " ",
            new[] { QuoteForBatch(executablePath) }
                .Concat(verificationArguments.Select(QuoteForBatch)));
        string command = $"\"{commandBody}\"";
        return (
            Path.Combine(Environment.SystemDirectory, "cmd.exe"),
            ["/d", "/s", "/c", command]);
    }

    private static bool IsSafeBatchArgument(string argument)
    {
        return argument.Length > 0
            && argument.All(character => char.IsLetterOrDigit(character)
                || character is '-' or '_' or '.' or ':' or '/');
    }

    private static string QuoteForBatch(string value)
    {
        if (value.Contains('"', StringComparison.Ordinal)
            || value.Contains('%', StringComparison.Ordinal)
            || value.Contains('!', StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Verification command contains unsupported batch metacharacters.");
        }

        return $"\"{value}\"";
    }
}
