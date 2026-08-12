using System.Diagnostics;

namespace WindowsDevInspector.Remediation;

public sealed class PackageInstallationCommandRunner : IPackageInstallationCommandRunner
{
    public async Task<PackageInstallationCommandRunResult> RunAsync(
        string fileName,
        IReadOnlyList<string> arguments,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(arguments);

        if (timeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be greater than zero.");
        }

        ProcessStartInfo startInfo = new()
        {
            FileName = fileName,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        foreach (string argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using Process process = new() { StartInfo = startInfo };

        try
        {
            if (!process.Start())
            {
                return Failure("The package installation process did not start.");
            }
        }
        catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            return Failure(ex.Message);
        }

        Task<string> standardOutput = process.StandardOutput.ReadToEndAsync(cancellationToken);
        Task<string> standardError = process.StandardError.ReadToEndAsync(cancellationToken);
        using CancellationTokenSource timeoutCancellation = new(timeout);
        using CancellationTokenSource combinedCancellation = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            timeoutCancellation.Token);

        try
        {
            await process.WaitForExitAsync(combinedCancellation.Token);
        }
        catch (OperationCanceledException) when (timeoutCancellation.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            KillProcessTree(process);
            await DrainOutputAsync(standardOutput, standardError);
            return new PackageInstallationCommandRunResult
            {
                ExitCode = null,
                TimedOut = true,
                StandardOutput = standardOutput.IsCompletedSuccessfully ? standardOutput.Result : string.Empty,
                StandardError = standardError.IsCompletedSuccessfully ? standardError.Result : string.Empty,
                ErrorMessage = $"Package installation timed out after {timeout}."
            };
        }
        catch (OperationCanceledException)
        {
            KillProcessTree(process);
            await DrainOutputAsync(standardOutput, standardError);
            throw;
        }

        return new PackageInstallationCommandRunResult
        {
            ExitCode = process.ExitCode,
            TimedOut = false,
            StandardOutput = await standardOutput,
            StandardError = await standardError,
            ErrorMessage = null
        };
    }

    private static PackageInstallationCommandRunResult Failure(string message)
    {
        return new PackageInstallationCommandRunResult
        {
            ExitCode = null,
            TimedOut = false,
            StandardOutput = string.Empty,
            StandardError = string.Empty,
            ErrorMessage = message
        };
    }

    private static void KillProcessTree(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch (InvalidOperationException)
        {
        }
    }

    private static async Task DrainOutputAsync(Task<string> standardOutput, Task<string> standardError)
    {
        try
        {
            await Task.WhenAll(standardOutput, standardError);
        }
        catch (OperationCanceledException)
        {
        }
    }
}
