namespace WindowsDevInspector.App;

public static class TechnologyCatalog
{
    public static IReadOnlyList<TechnologyGroup> CreateDefaultGroups()
    {
        return [
            CreateGroup("frontend", "前端", true, [
                Item("angular", "Angular"), Item("nextjs", "Next.js"), Item("nodejs", "Node.js"), Item("npm", "npm", true), Item("nvm", "nvm", true),
                Item("pnpm", "pnpm"), Item("react", "React"), Item("tailwindcss", "Tailwind CSS"), Item("vite", "Vite"), Item("vue", "Vue"), Item("yarn", "Yarn")
            ]),
            CreateGroup("backend", "後端", true, [
                Item("csharp", "C#", true), Item("cpp", "C++"), Item("dotnet", ".NET", true), Item("netcore", ".NET Core", true), Item("aspnetcore", "ASP.NET Core"),
                Item("flask", "Flask"), Item("go", "Go"), Item("java", "Java"), Item("php", "PHP"), Item("python", "Python"), Item("rails", "Ruby on Rails"),
                Item("redis", "Redis"), Item("rust", "Rust")
            ]),
            CreateGroup("database", "資料庫", true, [
                Item("oracle-db", "Oracle Database", true), Item("sqlserver", "SQL Server", true), Item("localdb", "SQL Server LocalDB"), Item("ssms", "SQL Server Management Studio"),
                Item("sqlcmd", "sqlcmd"), Item("postgresql", "PostgreSQL"), Item("mysql", "MySQL"), Item("sqlite", "SQLite"), Item("odbc-driver", "ODBC Driver")
            ]),
            CreateGroup("devops", "雲端 / DevOps", true, [
                Item("docker", "Docker", true), Item("wsl", "WSL"), Item("kubernetes", "Kubernetes"), Item("kubectl", "kubectl"),
                Item("terraform", "Terraform"), Item("powershell7", "PowerShell 7"), Item("git", "Git", true),
                Item("github-cli", "GitHub CLI"), Item("google-cloud-cli", "Google Cloud CLI"), Item("azure-cli", "Azure CLI"), Item("jenkins", "Jenkins", true),
                Item("winget", "winget"), Item("chocolatey", "Chocolatey")
            ]),
            CreateGroup("qa", "QA / 自動化", true, [
                Item("postman", "Postman", true), Item("newman", "Newman"), Item("pytest", "pytest")
            ]),
            CreateGroup("mobile", "Mobile", false, [
                Item("android-sdk", "Android SDK"), Item("flutter", "Flutter"), Item("react-native", "React Native"), Item("swift", "Swift")
            ]),
            CreateGroup("desktop", "Desktop", true, [
                Item("wpf", "WPF"), Item("winui3", "WinUI 3"), Item("dotnet-desktop-runtime", ".NET Desktop Runtime"), Item("dotnet", ".NET SDK"),
                Item("visual-studio", "Visual Studio", true), Item("vscode", "Visual Studio Code", true), Item("msbuild", "MSBuild"),
                Item("windows-sdk", "Windows SDK"), Item("cmake", "CMake"), Item("ninja", "Ninja"), Item("vcpkg", "vcpkg"),
                Item("developer-mode", "Developer Mode"), Item("electron", "Electron"), Item("long-paths", "Long Paths", true),
                Item("powershell", "PowerShell"), Item("windows-terminal", "Windows Terminal")
            ])
        ];
    }

    private static TechnologyGroup CreateGroup(string id, string name, bool isExpanded, IReadOnlyList<TechnologyItem> technologies)
    {
        TechnologyItem[] indexedTechnologies = technologies
            .OrderBy(technology => technology.Name, StringComparer.OrdinalIgnoreCase)
            .Select((technology, index) => new TechnologyItem(technology.Id, technology.Name, technology.IsSelected)
            {
                OriginalIndex = index
            })
            .ToArray();

        return new TechnologyGroup(id, name, indexedTechnologies)
        {
            IsExpanded = isExpanded
        };
    }

    private static TechnologyItem Item(string id, string name, bool isSelected = false)
    {
        return new TechnologyItem(id, name, isSelected);
    }
}
