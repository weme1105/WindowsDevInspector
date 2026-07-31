using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows;

public static class CommonCheckFactory
{
    public static IReadOnlyList<IEnvironmentCheck> Create(
        IFileSystem fileSystem,
        IRegistryReader registryReader,
        ICommandRunner commandRunner,
        IEnvironmentVariableReader environmentReader)
    {
        return [
            new PathInvalidEntriesCheck(environmentReader),
            new PathDuplicateEntriesCheck(environmentReader),
            new DirectoryExistsCheck("common.directory-source", "Common", "D:\\Source exists", "D:\\Source", fileSystem, "create-source-directory"),
            new DirectoryExistsCheck("common.directory-projects", "Common", "D:\\Projects exists", "D:\\Projects", fileSystem, "create-projects-directory"),
            new DirectoryExistsCheck("common.directory-note", "Common", "D:\\Note exists", "D:\\Note", fileSystem, "create-note-directory"),
            new RegistryDwordCheck(
                "common.long-paths",
                "Common",
                "Long Paths enabled",
                "HKLM",
                "SYSTEM\\CurrentControlSet\\Control\\FileSystem",
                "LongPathsEnabled",
                1,
                registryReader,
                "Long path support is disabled. Some development tools can fail with deeply nested dependency paths.",
                "enable-long-paths"),
            new RegistryDwordCheck(
                "common.developer-mode",
                "Common",
                "Developer Mode enabled",
                "HKLM",
                "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\AppModelUnlock",
                "AllowDevelopmentWithoutDevLicense",
                1,
                registryReader,
                "Developer Mode is disabled. Some local development workflows and app deployment operations may be blocked.",
                "enable-developer-mode"),
            new CommandVersionCheck("common.git", "Common", "Git CLI", "git", "--version", commandRunner),
            new CommandVersionCheck("common.winget", "Common", "winget CLI", "winget", "--version", commandRunner),
            new CommandVersionCheck("common.powershell7", "Common", "PowerShell 7 CLI", "pwsh", "--version", commandRunner),
            new CommandVersionCheck("common.chocolatey", "Common", "Chocolatey CLI", "choco", "--version", commandRunner, failureSeverity: CheckSeverity.Info)
        ];
    }
}
