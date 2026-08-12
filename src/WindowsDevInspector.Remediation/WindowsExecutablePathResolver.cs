namespace WindowsDevInspector.Remediation;

public sealed class WindowsExecutablePathResolver : IExecutablePathResolver
{
    public string? Resolve(string executableName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(executableName);

        if (Path.GetFileName(executableName) != executableName)
        {
            return null;
        }

        string[] extensions = Path.HasExtension(executableName)
            ? [string.Empty]
            : GetExecutableExtensions();

        foreach (string directory in (Environment.GetEnvironmentVariable("Path") ?? string.Empty)
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            foreach (string extension in extensions)
            {
                string candidate;
                try
                {
                    candidate = Path.Combine(directory, executableName + extension);
                }
                catch (ArgumentException)
                {
                    continue;
                }

                if (File.Exists(candidate))
                {
                    return Path.GetFullPath(candidate);
                }
            }
        }

        return null;
    }

    private static string[] GetExecutableExtensions()
    {
        string[] configured = (Environment.GetEnvironmentVariable("PATHEXT") ?? string.Empty)
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return configured.Length == 0
            ? [".COM", ".EXE", ".BAT", ".CMD"]
            : configured;
    }
}
