using WindowsDevInspector.App;

namespace WindowsDevInspector.App.Tests;

public sealed class TechnologyCatalogTests
{
    [Fact]
    public void CreateDefaultGroups_IncludesSelectedMvpAndPriorityBacklogScope()
    {
        IReadOnlyList<TechnologyGroup> groups = TechnologyCatalog.CreateDefaultGroups();

        string[] technologyIds = groups
            .SelectMany(group => group.Technologies)
            .Select(technology => technology.Id)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        Assert.Equal(VisibleTechnologyIds, technologyIds);
    }

    [Fact]
    public void CreateDefaultGroups_ShowsPriorityBacklogTechnologiesWithoutDefaultSelection()
    {
        IReadOnlyList<TechnologyGroup> groups = TechnologyCatalog.CreateDefaultGroups();
        Dictionary<string, TechnologyItem> technologies = groups
            .SelectMany(group => group.Technologies)
            .GroupBy(technology => technology.Id, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        foreach (string backlogTechnologyId in PriorityBacklogTechnologyIds)
        {
            Assert.True(technologies.TryGetValue(backlogTechnologyId, out TechnologyItem? technology));
            Assert.False(technology.IsSelected);
        }
    }

    [Fact]
    public void CreateDefaultGroups_PreservesExistingSuggestedDefaults()
    {
        IReadOnlyList<TechnologyGroup> groups = TechnologyCatalog.CreateDefaultGroups();

        string[] selectedTechnologyIds = groups
            .SelectMany(group => group.Technologies)
            .Where(technology => technology.IsSelected)
            .Select(technology => technology.Id)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        Assert.Equal([
            "csharp",
            "docker",
            "dotnet",
            "git",
            "jenkins",
            "long-paths",
            "netcore",
            "npm",
            "nvm",
            "oracle-db",
            "postman",
            "sqlserver",
            "visual-studio",
            "vscode"
        ], selectedTechnologyIds);
    }

    [Fact]
    public void CreateDefaultGroups_KeepsMobileGroupForSelectedBacklogScope()
    {
        IReadOnlyList<TechnologyGroup> groups = TechnologyCatalog.CreateDefaultGroups();

        TechnologyGroup mobileGroup = Assert.Single(groups, group => group.Id == "mobile");
        string[] mobileTechnologyIds = mobileGroup.Technologies
            .Select(technology => technology.Id)
            .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        Assert.Equal([
            "android-sdk",
            "flutter",
            "react-native",
            "swift"
        ], mobileTechnologyIds);
    }

    private static readonly string[] VisibleTechnologyIds =
    [
        "android-sdk",
        "angular",
        "aspnetcore",
        "azure-cli",
        "chocolatey",
        "cmake",
        "cpp",
        "csharp",
        "developer-mode",
        "docker",
        "dotnet",
        "dotnet-desktop-runtime",
        "electron",
        "flask",
        "flutter",
        "git",
        "github-cli",
        "go",
        "google-cloud-cli",
        "java",
        "jenkins",
        "kubectl",
        "kubernetes",
        "localdb",
        "long-paths",
        "msbuild",
        "mysql",
        "netcore",
        "newman",
        "nextjs",
        "ninja",
        "nodejs",
        "npm",
        "nvm",
        "odbc-driver",
        "oracle-db",
        "php",
        "pnpm",
        "postgresql",
        "postman",
        "powershell",
        "powershell7",
        "pytest",
        "python",
        "rails",
        "react",
        "react-native",
        "redis",
        "rust",
        "sqlcmd",
        "sqlite",
        "sqlserver",
        "ssms",
        "swift",
        "tailwindcss",
        "terraform",
        "vcpkg",
        "visual-studio",
        "vite",
        "vscode",
        "vue",
        "windows-sdk",
        "windows-terminal",
        "winget",
        "winui3",
        "wpf",
        "wsl",
        "yarn"
    ];

    private static readonly string[] PriorityBacklogTechnologyIds =
    [
        "android-sdk",
        "angular",
        "cpp",
        "dotnet-desktop-runtime",
        "electron",
        "flask",
        "flutter",
        "github-cli",
        "google-cloud-cli",
        "nextjs",
        "nodejs",
        "php",
        "powershell",
        "pytest",
        "rails",
        "react",
        "react-native",
        "rust",
        "ssms",
        "swift",
        "tailwindcss",
        "vue",
        "windows-terminal"
    ];
}
