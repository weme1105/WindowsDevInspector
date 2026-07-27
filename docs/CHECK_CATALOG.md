# Check Catalog

This document is the working inventory for WindowsDevInspector checks. It separates user-facing technologies from executable checks so one technology can appear in multiple groups and shared checks only run once.

## Data Flow

```text
TechnologyDefinition
  -> belongs to one or more display groups
  -> maps to one or more CheckDefinition IDs

SelectedTechnologyIds
  -> distinct TechnologyDefinition IDs
  -> distinct CheckDefinition IDs
  -> IEnvironmentCheck implementations
  -> CheckResult rows
```

Rules:

- Technology IDs are unique across the catalog.
- Display groups are views only; they do not own technology state.
- A technology may appear in multiple groups.
- Check IDs are unique and are deduplicated before execution.
- Common checks always run, even when no technology is selected.
- Unsupported selected technologies produce an `Info` result that says diagnostics are not implemented yet.

## Check Definition Shape

```text
Id
Name
Category
ImplementationProject
TriggerTechnologyIds
SeverityWhenMissing
CanFix
Risk
RequiresElevation
RequiresRestart
SupportsRollback
RemediationId
```

## Common Baseline Checks

These always run.

| Check ID | Name | Category | Implementation | Missing severity | Can fix | Risk | Elevation | Restart | Rollback | Remediation ID |
|---|---|---|---|---|---:|---|---:|---:|---:|---|
| common.windows-version | Windows version and build | Common | Windows | Info | No | None | No | No | No | |
| common.processor-architecture | Processor architecture | Common | Windows | Info | No | None | No | No | No | |
| common.path-invalid-entries | PATH invalid entries | Common | Windows | Warning | No | None | No | No | No | |
| common.path-duplicate-entries | PATH duplicate entries | Common | Windows | Info | No | None | No | No | No | |
| common.long-paths | Long Paths enabled | Common | Windows | Warning | Yes | Low | Yes | No | Yes | enable-long-paths |
| common.developer-mode | Developer Mode enabled | Common | Windows | Warning | Yes | Low | Yes | No | Yes | enable-developer-mode |
| common.directory-source | `D:\Source` exists | Common | Windows | Warning | Yes | Low | No | No | Yes | create-source-directory |
| common.directory-projects | `D:\Projects` exists | Common | Windows | Warning | Yes | Low | No | No | Yes | create-projects-directory |
| common.directory-gonote | `D:\GoNote` exists | Common | Windows | Info | Yes | Low | No | No | Yes | create-gonote-directory |
| common.powershell7 | PowerShell 7 CLI | Common | Windows | Info | No | None | No | No | No | |
| common.git | Git CLI | Common | Windows | Warning | No | None | No | No | No | |
| common.winget | winget CLI | Common | Windows | Warning | No | None | No | No | No | |

## Frontend Checks

| Check ID | Name | Trigger technologies | Category | Missing severity | Can fix | Notes |
|---|---|---|---|---|---:|---|
| frontend.node-cli | Node.js CLI | nodejs, javascript, typescript, react, vue, angular, svelte, vite, nextjs, nuxt | Frontend | Warning | No | Shared with backend, QA, Mobile. |
| frontend.npm-cli | npm CLI | nodejs, npm, javascript, typescript, react, vue, angular | Frontend | Warning | No | Usually bundled with Node.js. |
| frontend.pnpm-cli | pnpm CLI | pnpm, vue, angular, react, vite | Frontend | Info | No | Optional package manager. |
| frontend.yarn-cli | Yarn CLI | yarn, react, angular, vue | Frontend | Info | No | Optional package manager. |
| frontend.angular-cli | Angular CLI | angular | Frontend | Warning | No | Check `ng version`. |
| frontend.vite-cli | Vite CLI | vite, vue, react, svelte | Frontend | Info | No | Check local/global availability later. |
| frontend.playwright-cli | Playwright CLI | playwright | Frontend | Info | No | Shared with QA. |
| frontend.npm-global-prefix | npm global prefix | npm, nodejs | Frontend | Info | No | Detect path problems. |

## Backend Checks

| Check ID | Name | Trigger technologies | Category | Missing severity | Can fix | Notes |
|---|---|---|---|---|---:|---|
| backend.dotnet-cli | dotnet CLI | dotnet, csharp, aspnetcore | Backend | Warning | No | Shared with Desktop. |
| backend.dotnet-sdk | .NET SDK version | dotnet, csharp, aspnetcore | Backend | Warning | No | Use `dotnet --list-sdks`. |
| backend.dotnet-runtime | .NET runtime version | dotnet, csharp, aspnetcore | Backend | Info | No | Use `dotnet --list-runtimes`. |
| backend.aspnet-runtime | ASP.NET Core runtime | aspnetcore | Backend | Info | No | Use runtime list. |
| backend.nuget-sources | NuGet sources | dotnet, csharp, aspnetcore | Backend | Info | No | Must avoid logging credentials. |
| backend.visualstudio-buildtools | Visual Studio / Build Tools | csharp, cpp, dotnet | Backend | Info | No | Shared with Desktop. |
| backend.go-cli | Go CLI | go | Backend | Warning | No | Use `go version`. |
| backend.go-env | Go environment | go | Backend | Info | No | Use `go env`; sanitize output if needed. |
| backend.python-cli | Python CLI | python, django, fastapi, flask | Backend | Warning | No | Detect `py` and `python`. |
| backend.java-cli | Java CLI | java, springboot, kotlin | Backend | Warning | No | Detect `java -version`. |
| backend.docker-cli | Docker CLI | docker | Backend | Info | No | Shared with DevOps. |
| backend.redis-cli | Redis CLI | redis | Backend | Info | No | Optional client check. |

## Database Checks

| Check ID | Name | Trigger technologies | Category | Missing severity | Can fix | Notes |
|---|---|---|---|---|---:|---|
| database.sqlserver-tools | SQL Server tooling | sqlserver, ssms, sqlcmd, localdb | Database | Info | No | Detect installed tools. |
| database.sqlcmd-cli | sqlcmd CLI | sqlcmd, sqlserver | Database | Warning | No | Useful for automation. |
| database.localdb | SQL Server LocalDB | localdb, sqlserver | Database | Info | No | Detect `sqllocaldb`. |
| database.ssms | SQL Server Management Studio | ssms, sqlserver | Database | Info | No | Registry/app detection. |
| database.odbc-driver | ODBC Driver | odbc-driver, sqlserver | Database | Info | No | Registry detection. |
| database.postgresql-cli | PostgreSQL CLI | postgresql, pgadmin | Database | Info | No | Detect `psql`. |
| database.mysql-cli | MySQL CLI | mysql, mariadb, mysql-workbench | Database | Info | No | Detect `mysql`. |
| database.sqlite-cli | SQLite CLI | sqlite | Database | Info | No | Detect `sqlite3`. |
| database.oracle-client | Oracle Client | oracle-db, oracle-client | Database | Info | No | Detect client install and PATH. |

## DevOps And Cloud Checks

| Check ID | Name | Trigger technologies | Category | Missing severity | Can fix | Notes |
|---|---|---|---|---|---:|---|
| devops.docker-cli | Docker CLI | docker, docker-desktop | DevOps | Warning | No | Shared with backend. |
| devops.docker-desktop | Docker Desktop | docker, docker-desktop | DevOps | Warning | No | Detect app/service. |
| devops.wsl | WSL installed | wsl, docker, kubernetes | DevOps | Warning | No | Use `wsl --status`. |
| devops.wsl-version | WSL version | wsl | DevOps | Info | No | Detect WSL 1 vs 2. |
| devops.wsl-distros | WSL distributions | wsl | DevOps | Info | No | Use `wsl --list --verbose`. |
| devops.virtual-machine-platform | Virtual Machine Platform | wsl, docker | DevOps | Warning | No | Optional feature read-only. |
| devops.hyper-v | Hyper-V | docker, kubernetes | DevOps | Info | No | Optional feature read-only. |
| devops.kubectl | kubectl CLI | kubernetes, kubectl | DevOps | Info | No | Detect `kubectl version --client`. |
| devops.azure-cli | Azure CLI | azure-cli, azure-developer-cli | DevOps | Info | No | Detect `az version`. |
| devops.terraform | Terraform CLI | terraform | DevOps | Info | No | Detect `terraform version`. |
| devops.localhost-bind | localhost bind health | docker, kubernetes, nodejs | DevOps | Info | No | Later network diagnostic. |
| devops.winnat | WinNAT service/state | docker, wsl | DevOps | Info | No | Read-only service/network check. |
| devops.hns | Host Network Service | docker, wsl | DevOps | Info | No | Read-only service check. |

## QA And Automation Checks

| Check ID | Name | Trigger technologies | Category | Missing severity | Can fix | Notes |
|---|---|---|---|---|---:|---|
| qa.playwright | Playwright availability | playwright | QA | Info | No | Shared with frontend. |
| qa.browser-availability | Browser availability | playwright, selenium, cypress, webdriverio | QA | Info | No | Detect common browsers. |
| qa.selenium | Selenium tooling | selenium, webdriverio | QA | Info | No | First pass can be catalog Info only. |
| qa.postman | Postman | postman, newman | QA | Info | No | Detect app or CLI. |
| qa.newman | Newman CLI | newman, postman | QA | Info | No | Detect CLI. |
| qa.k6 | k6 CLI | k6 | QA | Info | No | Detect CLI. |
| qa.jmeter | JMeter | jmeter | QA | Info | No | Detect install or command. |

## Mobile Checks

| Check ID | Name | Trigger technologies | Category | Missing severity | Can fix | Notes |
|---|---|---|---|---|---:|---|
| mobile.android-sdk | Android SDK | android-sdk, android-studio, flutter, react-native | Mobile | Info | No | Detect SDK path. |
| mobile.adb | ADB CLI | adb, android-sdk, android-studio | Mobile | Info | No | Detect `adb version`. |
| mobile.android-emulator | Android Emulator | android-emulator, android-studio | Mobile | Info | No | Detect emulator tool. |
| mobile.gradle | Gradle | gradle, android-studio, kotlin, java | Mobile | Info | No | Detect `gradle` or wrapper later. |
| mobile.flutter | Flutter CLI | flutter, dart | Mobile | Info | No | Detect `flutter --version`. |
| mobile.react-native | React Native tooling | react-native, expo | Mobile | Info | No | Depends on Node.js checks. |
| mobile.dotnet-maui | .NET MAUI workload | dotnet-maui, maui-check | Mobile | Info | No | Use `dotnet workload list`. |

## Desktop Checks

| Check ID | Name | Trigger technologies | Category | Missing severity | Can fix | Notes |
|---|---|---|---|---|---:|---|
| desktop.dotnet-desktop-runtime | .NET Desktop Runtime | dotnet-desktop-runtime, wpf, winui3 | Desktop | Info | No | Use runtime list. |
| desktop.windows-sdk | Windows SDK | windows-sdk, winui3, windows-app-sdk, cpp | Desktop | Info | No | Detect installed kits. |
| desktop.visualstudio | Visual Studio | visual-studio, wpf, winui3, cpp | Desktop | Info | No | Detect install instances later. |
| desktop.build-tools | Visual Studio Build Tools | visual-studio-build-tools, msvc, cpp | Desktop | Info | No | Shared with backend. |
| desktop.msbuild | MSBuild | msbuild, visual-studio, visual-studio-build-tools | Desktop | Info | No | Detect command/path. |
| desktop.cmake | CMake CLI | cmake, cpp, qt | Desktop | Info | No | Detect `cmake --version`. |
| desktop.ninja | Ninja CLI | ninja, cpp, cmake | Desktop | Info | No | Detect `ninja --version`. |
| desktop.vcpkg | vcpkg | vcpkg, cpp | Desktop | Info | No | Detect command/path. |
| desktop.signtool | signtool | signtool, msix-packaging-tool | Desktop | Info | No | Usually from Windows SDK. |

## First Implementation Slice

Recommended order:

1. Implement shared command runner with timeout in `WindowsDevInspector.Windows`.
2. Implement Common checks first: directories, Long Paths, Developer Mode, Git, winget.
3. Add check mapping in Core: technology ID -> check IDs.
4. Replace App placeholder results with deduplicated check IDs.
5. Add fake process/registry abstractions before wider CLI and Registry coverage.

