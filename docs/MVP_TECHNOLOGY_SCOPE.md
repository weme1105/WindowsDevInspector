# MVP Technology Scope

This document records the first-version technology scope selected from the current catalog review.

The goal is to keep the MVP focused on technologies that either already have executable checks or are explicitly selected as priority backlog work. The WPF technology picker should not show every future technology idea by default.

## MVP Included Technologies

These technologies are currently considered in scope for the first MVP UI because their mapped checks are implemented.

| Technology ID | Display name |
|---|---|
| `aspnetcore` | ASP.NET Core |
| `azure-cli` | Azure CLI |
| `chocolatey` | Chocolatey |
| `cmake` | CMake |
| `csharp` | C# |
| `developer-mode` | Developer Mode |
| `docker` | Docker |
| `dotnet` | .NET |
| `git` | Git |
| `go` | Go |
| `java` | Java |
| `jenkins` | Jenkins |
| `kubectl` | kubectl |
| `kubernetes` | Kubernetes |
| `localdb` | SQL Server LocalDB |
| `long-paths` | Long Paths |
| `msbuild` | MSBuild |
| `mysql` | MySQL |
| `netcore` | .NET Core |
| `newman` | Newman |
| `ninja` | Ninja |
| `npm` | npm |
| `nvm` | nvm |
| `odbc-driver` | ODBC Driver |
| `oracle-db` | Oracle Database |
| `pnpm` | pnpm |
| `postgresql` | PostgreSQL |
| `postman` | Postman |
| `powershell7` | PowerShell 7 |
| `python` | Python |
| `redis` | Redis |
| `sqlcmd` | sqlcmd |
| `sqlite` | SQLite |
| `sqlserver` | SQL Server |
| `terraform` | Terraform |
| `vcpkg` | vcpkg |
| `visual-studio` | Visual Studio |
| `vite` | Vite |
| `vscode` | Visual Studio Code |
| `windows-sdk` | Windows SDK |
| `winget` | winget |
| `winui3` | WinUI 3 |
| `wpf` | WPF |
| `wsl` | WSL |
| `yarn` | Yarn |

## Priority Backlog Technologies

These technologies should not be treated as complete MVP UI items yet. They were selected as priority candidates for future checks or catalog definitions.

| Technology ID | Display name | Current gap |
|---|---|---|
| `android-sdk` | Android SDK | Core mapping and SDK path check exist; emulator/tool-specific diagnostics remain backlog. |
| `angular` | Angular | Core catalog exists; remaining work is to validate Angular CLI behavior beyond basic command execution. |
| `cpp` | C++ | Core mapping and Visual Studio Build Tools check exist; compiler/toolchain depth remains backlog. |
| `dotnet-desktop-runtime` | .NET Desktop Runtime | Core mapping exists and reuses the .NET Desktop Runtime check. |
| `electron` | Electron | UI item exists, but Core technology mapping is not defined. |
| `flask` | Flask | Core mapping exists for Python CLI; Flask package/project diagnostics remain backlog. |
| `flutter` | Flutter | Core mapping, Flutter CLI check, and Android SDK path check exist; project-specific Flutter diagnostics remain backlog. |
| `github-cli` | GitHub CLI | Core mapping and CLI check exist; workflow-specific GitHub diagnostics remain backlog. |
| `google-cloud-cli` | Google Cloud CLI | Core mapping and CLI check exist; cloud auth/project diagnostics remain backlog. |
| `nextjs` | Next.js | Core catalog mapping exists for Node.js and npm; framework-specific diagnostics remain backlog. |
| `nodejs` | Node.js | Core catalog exists; Node.js CLI check is implemented, but package manager/version policy still needs MVP review. |
| `php` | PHP | Core mapping and PHP CLI check exist; framework/package diagnostics remain backlog. |
| `powershell` | PowerShell | Core mapping and Windows PowerShell CLI check exist. |
| `pytest` | pytest | Core mapping exists for Python CLI; pytest package diagnostics remain backlog. |
| `rails` | Ruby on Rails | Core mapping and Ruby CLI check exist; Rails gem/project diagnostics remain backlog. |
| `react` | React | Core catalog mapping exists for Node.js and npm; framework-specific diagnostics remain backlog. |
| `react-native` | React Native | UI item exists, but Core technology mapping is not defined. |
| `rust` | Rust | Core mapping and Rust CLI check exist; Cargo/toolchain diagnostics remain backlog. |
| `ssms` | SQL Server Management Studio | Core mapping exists and reuses the SSMS file detection check. |
| `swift` | Swift | UI item exists, but Core technology mapping is not defined. |
| `tailwindcss` | Tailwind CSS | Core catalog mapping exists for Node.js and npm; CSS/tooling-specific diagnostics remain backlog. |
| `vue` | Vue | Core catalog exists; remaining work is to decide whether Vite/npm checks are enough for MVP Vue diagnostics. |
| `windows-terminal` | Windows Terminal | Core mapping and CLI check exist; profile/settings diagnostics remain backlog. |

## UI Scope Decision

For the first MVP, the WPF technology picker should show:

- MVP included technologies.
- Priority backlog technologies explicitly selected for follow-up implementation.

Priority backlog technologies should not be selected by default. They may appear in the picker so the user can keep them visible while deciding the next implementation slices, but they should be understood as incomplete until their catalog mappings and executable checks are finished.

Technologies outside these two lists should stay hidden from the first MVP picker. This avoids a broad UI where users can select technologies that were never selected for the MVP or priority backlog.

## Local Review Artifact

The local helper file `artifacts/mvp-technology-selection.html` was used to review and select this scope. It is an output artifact and is not required by the app at runtime.
