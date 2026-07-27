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

            Task<byte[]> outputTask = ReadAllBytesAsync(process.StandardOutput.BaseStream, timeoutSource.Token);
            Task<byte[]> errorTask = ReadAllBytesAsync(process.StandardError.BaseStream, timeoutSource.Token);

            await process.WaitForExitAsync(timeoutSource.Token);
            await Task.WhenAll(outputTask, errorTask);

            return new CommandRunResult
            {
                FileName = fileName,
                Arguments = arguments,
                ExitCode = process.ExitCode,
                StandardOutput = DecodeOutput(await outputTask),
                StandardError = DecodeOutput(await errorTask)
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

    private static async Task<byte[]> ReadAllBytesAsync(Stream stream, CancellationToken cancellationToken)
    {
        using MemoryStream memoryStream = new();

        await stream.CopyToAsync(memoryStream, cancellationToken);

        return memoryStream.ToArray();
    }

    private static string DecodeOutput(byte[] bytes)
    {
        if (bytes.Length == 0)
        {
            return string.Empty;
        }

        if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
        {
            return Encoding.Unicode.GetString(bytes);
        }

        if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
        {
            return Encoding.BigEndianUnicode.GetString(bytes);
        }

        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        if (LooksLikeUtf16LittleEndian(bytes))
        {
            return Encoding.Unicode.GetString(bytes);
        }

        Encoding ansiEncoding = GetEncodingOrDefault(CultureInfo.CurrentCulture.TextInfo.ANSICodePage);
        string[] candidates = [
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: false).GetString(bytes),
            ConsoleEncoding.GetString(bytes),
            ansiEncoding.GetString(bytes)
        ];

        return candidates
            .OrderBy(DecodePenalty)
            .First();
    }

    private static Encoding GetEncodingOrDefault(int codePage)
    {
        try
        {
            return Encoding.GetEncoding(codePage);
        }
        catch (ArgumentException)
        {
            return ConsoleEncoding;
        }
    }

    private static bool LooksLikeUtf16LittleEndian(byte[] bytes)
    {
        if (bytes.Length < 8)
        {
            return false;
        }

        int oddNullCount = 0;
        int evenNullCount = 0;
        int pairsToInspect = Math.Min(bytes.Length / 2, 128);

        for (int pairIndex = 0; pairIndex < pairsToInspect; pairIndex++)
        {
            if (bytes[pairIndex * 2] == 0)
            {
                evenNullCount++;
            }

            if (bytes[(pairIndex * 2) + 1] == 0)
            {
                oddNullCount++;
            }
        }

        return oddNullCount > pairsToInspect / 4 && oddNullCount > evenNullCount * 2;
    }

    private static int DecodePenalty(string value)
    {
        int penalty = value.Count(character => character == '\uFFFD') * 10;

        penalty += value.Count(character =>
            char.IsControl(character)
            && character is not '\r'
            && character is not '\n'
            && character is not '\t');

        return penalty;
    }
}
