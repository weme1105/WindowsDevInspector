using WindowsDevInspector.Core;

namespace WindowsDevInspector.Windows;

public static class BuiltInEnvironmentCheckFactory
{
    public static IReadOnlyDictionary<string, IEnvironmentCheck> CreateAll()
    {
        IFileSystem fileSystem = new SystemFileSystem();
        IRegistryReader registryReader = new WindowsRegistryReader();
        ICommandRunner commandRunner = new ProcessCommandRunner();
        IServiceReader serviceReader = new WindowsServiceReader();
        IEnvironmentVariableReader environmentReader = new SystemEnvironmentVariableReader();
        ILocalhostBindProbe localhostBindProbe = new SystemLocalhostBindProbe();
        return CommonCheckFactory
            .Create(fileSystem, registryReader, commandRunner, environmentReader)
            .Concat(CreateCliChecks(commandRunner, fileSystem))
            .Concat(CreatePackageAvailabilityChecks(commandRunner))
            .Concat(CreateWindowsIntegrationChecks(commandRunner, registryReader, serviceReader, fileSystem, environmentReader, localhostBindProbe))
            .ToDictionary(check => check.Id, StringComparer.OrdinalIgnoreCase);
    }

    private static IReadOnlyList<IEnvironmentCheck> CreateCliChecks(ICommandRunner commandRunner, IFileSystem fileSystem)
    {
        return [
            new CommandVersionCheck("backend.dotnet-cli", "Backend", "dotnet CLI", "dotnet", "--info", commandRunner, TimeSpan.FromSeconds(8)),
            new CommandVersionCheck("backend.dotnet-sdk", "Backend", ".NET SDK version", "dotnet", "--list-sdks", commandRunner, TimeSpan.FromSeconds(8)),
            new CommandVersionCheck("backend.dotnet-runtime", "Backend", ".NET runtime version", "dotnet", "--list-runtimes", commandRunner, TimeSpan.FromSeconds(8)),
            new DotNetRuntimeCheck("backend.aspnet-runtime", "Backend", "ASP.NET Core runtime", "Microsoft.AspNetCore.App", commandRunner),
            new NuGetSourcesCheck(commandRunner),
            new CommandVersionCheck("frontend.node-cli", "Frontend", "Node.js CLI", "node", "--version", commandRunner),
            new CommandVersionCheck("frontend.npm-cli", "Frontend", "npm CLI", "npm", "--version", commandRunner),
            new CommandVersionCheck("frontend.npm-global-prefix", "Frontend", "npm global prefix", "npm", "prefix -g", commandRunner),
            new CommandVersionCheck("frontend.nvm", "Frontend", "nvm", "nvm", "version", commandRunner),
            new CommandVersionCheck("frontend.pnpm-cli", "Frontend", "pnpm CLI", "pnpm", "--version", commandRunner),
            new CommandVersionCheck("frontend.yarn-cli", "Frontend", "Yarn CLI", "yarn", "--version", commandRunner),
            new CommandVersionCheck("frontend.angular-cli", "Frontend", "Angular CLI", "ng", "version", commandRunner, TimeSpan.FromSeconds(8)),
            new CommandVersionCheck("frontend.vite-cli", "Frontend", "Vite CLI", "vite", "--version", commandRunner),
            new CommandVersionCheck("frontend.playwright-cli", "Frontend", "Playwright CLI", "npx", "playwright --version", commandRunner, TimeSpan.FromSeconds(8)),
            new CommandVersionCheck("frontend.electron-package", "Frontend", "Electron package", "npm", "list -g electron --depth=0", commandRunner, TimeSpan.FromSeconds(8), CheckSeverity.Info),
            new CommandVersionCheck("backend.docker-cli", "Backend", "Docker CLI", "docker", "--version", commandRunner),
            new CommandVersionCheck("backend.go-cli", "Backend", "Go CLI", "go", "version", commandRunner),
            new CommandVersionCheck("backend.go-env", "Backend", "Go environment", "go", "env GOVERSION GOPATH GOROOT", commandRunner),
            new CommandVersionCheck("backend.python-cli", "Backend", "Python CLI", "python", "--version", commandRunner),
            new CommandVersionCheck("backend.flask-package", "Backend", "Flask package", "python", "-m flask --version", commandRunner, TimeSpan.FromSeconds(8), CheckSeverity.Info),
            new CommandVersionCheck("backend.php-cli", "Backend", "PHP CLI", "php", "--version", commandRunner),
            new CommandVersionCheck("backend.ruby-cli", "Backend", "Ruby CLI", "ruby", "--version", commandRunner),
            new CommandVersionCheck("backend.rails-gem", "Backend", "Rails gem", "rails", "--version", commandRunner, failureSeverity: CheckSeverity.Info),
            new CommandVersionCheck("backend.rust-cli", "Backend", "Rust CLI", "rustc", "--version", commandRunner),
            new CommandVersionCheck("backend.cargo-cli", "Backend", "Cargo CLI", "cargo", "--version", commandRunner, failureSeverity: CheckSeverity.Info),
            new CommandVersionCheck("backend.java-cli", "Backend", "Java CLI", "java", "-version", commandRunner),
            new CommandVersionCheck("backend.redis-cli", "Backend", "Redis CLI", "redis-cli", "--version", commandRunner),
            new CommandVersionCheck("devops.docker-cli", "DevOps", "Docker CLI", "docker", "--version", commandRunner),
            new DockerDesktopCheck(commandRunner),
            new CommandVersionCheck("devops.kubectl", "DevOps", "kubectl CLI", "kubectl", "version --client", commandRunner),
            new CommandVersionCheck("devops.github-cli", "DevOps", "GitHub CLI", "gh", "--version", commandRunner),
            new CommandVersionCheck("devops.azure-cli", "DevOps", "Azure CLI", "az", "version", commandRunner, TimeSpan.FromSeconds(8)),
            new CommandVersionCheck("devops.google-cloud-cli", "DevOps", "Google Cloud CLI", "gcloud", "--version", commandRunner, TimeSpan.FromSeconds(8)),
            new CommandVersionCheck("devops.terraform", "DevOps", "Terraform CLI", "terraform", "version", commandRunner),
            new CommandVersionCheck("devops.jenkins", "DevOps", "Jenkins CLI", "jenkins", "--version", commandRunner),
            new CommandVersionCheck("database.sqlcmd-cli", "Database", "sqlcmd CLI", "sqlcmd", "-?", commandRunner),
            new CommandVersionCheck("database.postgresql-cli", "Database", "PostgreSQL CLI", "psql", "--version", commandRunner),
            new CommandVersionCheck("database.mysql-cli", "Database", "MySQL CLI", "mysql", "--version", commandRunner),
            new CommandVersionCheck("database.sqlite-cli", "Database", "SQLite CLI", "sqlite3", "--version", commandRunner),
            new CommandVersionCheck("database.oracle-client", "Database", "Oracle Client", "sqlplus", "-V", commandRunner),
            new CommandVersionCheck("qa.playwright", "QA", "Playwright availability", "npx", "playwright --version", commandRunner, TimeSpan.FromSeconds(8)),
            new CommandVersionCheck("qa.pytest-package", "QA", "pytest package", "python", "-m pytest --version", commandRunner, TimeSpan.FromSeconds(8), CheckSeverity.Info),
            new CommandVersionCheck("qa.selenium", "QA", "Selenium tooling", "selenium", "--version", commandRunner),
            new CommandVersionCheck("qa.postman", "QA", "Postman", "postman", "--version", commandRunner),
            new CommandVersionCheck("qa.newman", "QA", "Newman CLI", "newman", "--version", commandRunner),
            new CommandVersionCheck("qa.k6", "QA", "k6 CLI", "k6", "version", commandRunner),
            new CommandVersionCheck("qa.jmeter", "QA", "JMeter", "jmeter", "--version", commandRunner),
            new CommandVersionCheck("mobile.adb", "Mobile", "ADB CLI", "adb", "version", commandRunner),
            new CommandVersionCheck("mobile.flutter", "Mobile", "Flutter CLI", "flutter", "--version", commandRunner, TimeSpan.FromSeconds(8)),
            new CommandVersionCheck("mobile.swift-cli", "Mobile", "Swift CLI", "swift", "--version", commandRunner, failureSeverity: CheckSeverity.Info),
            new DotNetMauiWorkloadCheck(commandRunner),
            new DotNetRuntimeCheck("desktop.dotnet-desktop-runtime", "Desktop", ".NET Desktop Runtime", "Microsoft.WindowsDesktop.App", commandRunner),
            new CommandVersionCheck("desktop.vscode", "Desktop", "Visual Studio Code", "code", "--version", commandRunner),
            new CommandVersionCheck("desktop.windows-terminal", "Desktop", "Windows Terminal", "wt", "--version", commandRunner),
            new CommandVersionCheck("desktop.powershell", "Desktop", "Windows PowerShell", "powershell", "-NoProfile -Command $PSVersionTable.PSVersion.ToString()", commandRunner),
            new CommandVersionCheck("desktop.msbuild", "Desktop", "MSBuild", "msbuild", "-version", commandRunner),
            new CommandVersionCheck("desktop.cmake", "Desktop", "CMake CLI", "cmake", "--version", commandRunner),
            new CommandVersionCheck("desktop.ninja", "Desktop", "Ninja CLI", "ninja", "--version", commandRunner),
            new CommandVersionCheck("desktop.vcpkg", "Desktop", "vcpkg", "vcpkg", "version", commandRunner),
            new FileExistsCheck("database.sqlserver-tools", "Database", "SQL Server tooling", [
                "C:\\Program Files\\Microsoft SQL Server\\Client SDK\\ODBC\\170\\Tools\\Binn\\SQLCMD.EXE",
                "C:\\Program Files\\Microsoft SQL Server\\Client SDK\\ODBC\\180\\Tools\\Binn\\SQLCMD.EXE"
            ], "SQL Server command line tooling"),
            new FileExistsCheck("database.odbc-driver", "Database", "ODBC Driver", [
                "C:\\Windows\\System32\\msodbcsql17.dll",
                "C:\\Windows\\System32\\msodbcsql18.dll"
            ], "Microsoft ODBC Driver for SQL Server"),
            new FileExistsCheck("database.localdb", "Database", "SQL Server LocalDB", [
                "C:\\Program Files\\Microsoft SQL Server\\160\\Tools\\Binn\\SqlLocalDB.exe",
                "C:\\Program Files\\Microsoft SQL Server\\150\\Tools\\Binn\\SqlLocalDB.exe"
            ], "SqlLocalDB.exe"),
            new VisualStudioCheck(commandRunner),
            new VisualStudioBuildToolsCheck(commandRunner, fileSystem)
        ];
    }

    private static IReadOnlyList<IEnvironmentCheck> CreatePackageAvailabilityChecks(ICommandRunner commandRunner)
    {
        return [
            new WingetPackageAvailabilityCheck("install.pnpm-winget", "Install Planning", "pnpm winget package", "pnpm.pnpm", commandRunner),
            new WingetPackageAvailabilityCheck("install.azure-cli-winget", "Install Planning", "Azure CLI winget package", "Microsoft.AzureCLI", commandRunner),
            new WingetPackageAvailabilityCheck("install.kubectl-winget", "Install Planning", "kubectl winget package", "Kubernetes.kubectl", commandRunner),
            new WingetPackageAvailabilityCheck("install.terraform-winget", "Install Planning", "Terraform winget package", "Hashicorp.Terraform", commandRunner)
        ];
    }

    private static IReadOnlyList<IEnvironmentCheck> CreateWindowsIntegrationChecks(
        ICommandRunner commandRunner,
        IRegistryReader registryReader,
        IServiceReader serviceReader,
        IFileSystem fileSystem,
        IEnvironmentVariableReader environmentReader,
        ILocalhostBindProbe localhostBindProbe)
    {
        return [
            new FirewallProfilesCheck(commandRunner),
            new CodeIntegrityEventsCheck(commandRunner),
            new SmartAppControlCheck(registryReader),
            new WslStatusCheck(commandRunner),
            new WslVersionCheck(commandRunner),
            new WslDistributionsCheck(commandRunner),
            new OptionalFeatureCheck("devops.virtual-machine-platform", "DevOps", "Virtual Machine Platform", "VirtualMachinePlatform", commandRunner),
            new OptionalFeatureCheck("devops.hyper-v", "DevOps", "Hyper-V", "Microsoft-Hyper-V-All", commandRunner),
            new ServiceStatusCheck(
                "devops.winnat",
                "DevOps",
                "WinNAT service",
                "WinNat",
                serviceReader,
                expectedValue: "WinNat service exists and can be inspected",
                missingImpact: "WinNAT is used by local container and WSL networking. If it is missing, Docker or WSL networking may be unavailable.",
                availableImpact: "WinNAT service is present for local virtualization networking."),
            new ServiceStatusCheck(
                "devops.hns",
                "DevOps",
                "Host Network Service",
                "hns",
                serviceReader,
                expectedStatus: "Running",
                severityWhenUnexpected: CheckSeverity.Warning,
                missingImpact: "Host Network Service should be running for Docker, WSL, and Kubernetes networking.",
                availableImpact: "Host Network Service is running for local container networking."),
            new ServiceStatusCheck(
                "devops.docker-service",
                "DevOps",
                "Docker Desktop service",
                "com.docker.service",
                serviceReader,
                expectedStatus: "Running",
                severityWhenUnexpected: CheckSeverity.Warning,
                missingImpact: "Docker Desktop service should be running for Docker Desktop workflows.",
                availableImpact: "Docker Desktop service is running."),
            new LocalhostBindHealthCheck(localhostBindProbe),
            new BrowserAvailabilityCheck(fileSystem),
            new AndroidSdkCheck(environmentReader, fileSystem),
            new FileExistsCheck("desktop.windows-sdk", "Desktop", "Windows SDK", [
                "C:\\Program Files (x86)\\Windows Kits\\10\\bin",
                "C:\\Program Files (x86)\\Windows Kits\\11\\bin"
            ], "Windows SDK bin directory"),
            new FileExistsCheck("database.ssms", "Database", "SQL Server Management Studio", [
                "C:\\Program Files (x86)\\Microsoft SQL Server Management Studio 20\\Common7\\IDE\\Ssms.exe",
                "C:\\Program Files (x86)\\Microsoft SQL Server Management Studio 19\\Common7\\IDE\\Ssms.exe",
                "C:\\Program Files (x86)\\Microsoft SQL Server Management Studio 18\\Common7\\IDE\\Ssms.exe"
            ], "SSMS executable")
        ];
    }
}
