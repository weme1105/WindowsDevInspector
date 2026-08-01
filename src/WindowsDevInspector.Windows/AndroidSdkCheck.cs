using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows;

public sealed class AndroidSdkCheck(IEnvironmentVariableReader environmentReader, IFileSystem fileSystem) : IEnvironmentCheck
{
    public string Id => "mobile.android-sdk";

    public Task<CheckResult> RunAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string[] sdkRoots = CandidateRoots()
            .OfType<string>()
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        string? existingRoot = sdkRoots.FirstOrDefault(fileSystem.DirectoryExists);
        bool found = existingRoot is not null;
        bool hasAdb = found && fileSystem.FileExists(Path.Combine(existingRoot!, "platform-tools", "adb.exe"));

        return Task.FromResult(new CheckResult
        {
            Id = Id,
            Category = "Mobile",
            Name = "Android SDK",
            Severity = found ? CheckSeverity.Pass : CheckSeverity.Info,
            CurrentValue = found
                ? $"{existingRoot}; platform-tools/adb: {(hasAdb ? "found" : "not found")}"
                : "ANDROID_HOME/ANDROID_SDK_ROOT and common SDK paths were not found",
            ExpectedValue = "Android SDK root with platform-tools available",
            Impact = found
                ? "Android SDK path was detected without modifying environment variables."
                : "Android, Flutter, or React Native builds may fail until an Android SDK is configured.",
            CanFix = false,
            Risk = RiskLevel.None
        });
    }

    private IEnumerable<string?> CandidateRoots()
    {
        yield return environmentReader.GetEnvironmentVariable("ANDROID_HOME", EnvironmentVariableTarget.Process);
        yield return environmentReader.GetEnvironmentVariable("ANDROID_SDK_ROOT", EnvironmentVariableTarget.Process);
        yield return environmentReader.GetEnvironmentVariable("ANDROID_HOME", EnvironmentVariableTarget.User);
        yield return environmentReader.GetEnvironmentVariable("ANDROID_SDK_ROOT", EnvironmentVariableTarget.User);
        yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Android", "Sdk");
        yield return "C:\\Android\\Sdk";
    }
}
