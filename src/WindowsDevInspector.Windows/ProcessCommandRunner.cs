using System.Diagnostics;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace WindowsDevInspector.Windows;

public sealed class ProcessCommandRunner : ICommandRunner
{
    private static readonly Encoding ConsoleEncoding = GetConsoleEncoding();

    public async Task<CommandRunResult> RunAsync(string fileName, string arguments, TimeSpan timeout, CancellationToken cancellationToken)
    {
        using CancellationTokenSource timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(timeout);

        ProcessStartInfo startInfo = new()
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = ConsoleEncoding,
            StandardErrorEncoding = ConsoleEncoding,
            CreateNoWindow = true
        };

        using Process process = new()
        {
            StartInfo = startInfo
        };

        try
        {
            if (!process.Start())
            {
                return Failed(fileName, arguments, "Process could not be started.");
            }

            Task<string> outputTask = process.StandardOutput.ReadToEndAsync(timeoutSource.Token);
            Task<string> errorTask = process.StandardError.ReadToEndAsync(timeoutSource.Token);

            await process.WaitForExitAsync(timeoutSource.Token);

            return new CommandRunResult
            {
                FileName = fileName,
                Arguments = arguments,
                ExitCode = process.ExitCode,
                StandardOutput = await outputTask,
                StandardError = await errorTask
            };
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            TryKill(process);

            return new CommandRunResult
            {
                FileName = fileName,
                Arguments = arguments,
                TimedOut = true,
                ErrorMessage = $"Command timed out after {timeout.TotalSeconds:N0} seconds."
            };
        }
        catch (Exception exception) when (exception is Win32Exception or InvalidOperationException or IOException)
        {
            return Failed(fileName, arguments, exception.Message);
        }
    }

    private static CommandRunResult Failed(string fileName, string arguments, string message)
    {
        return new CommandRunResult
        {
            FileName = fileName,
            Arguments = arguments,
            ErrorMessage = message
        };
    }

    private static void TryKill(Process process)
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

    private static Encoding GetConsoleEncoding()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        int codePage = CultureInfo.CurrentCulture.TextInfo.OEMCodePage;

        try
        {
            return Encoding.GetEncoding(codePage);
        }
        catch (ArgumentException)
        {
            return Encoding.UTF8;
        }
    }
}
