# AI Context

## Technology Stack

### Application

- Language: C#.
- Runtime: .NET 10.
- UI: WPF.
- Architecture: MVVM-oriented WPF with clear project boundaries. `MainWindow.xaml.cs` still handles WPF events and UI state, while scan, report, remediation, rollback, and backup orchestration are delegated to App-layer services.
- JSON: `System.Text.Json`.
- Dependency management: central package management through `Directory.Packages.props`.

### Windows Integration

- Registry read checks.
- Registry DWORD remediation through elevated worker.
- Services read checks.
- Optional feature checks.
- Process execution with timeout and output decoding.
- File system checks and approved directory creation.
- Environment variable inspection for PATH diagnostics.
- Read-only developer tooling checks for NuGet sources, Visual Studio Build Tools, common browsers, Android SDK paths, and .NET MAUI workloads.

### Testing

- xUnit.
- Test projects:
  - `tests/WindowsDevInspector.Core.Tests`
  - `tests/WindowsDevInspector.App.Tests`
  - `tests/WindowsDevInspector.Windows.Tests`
  - `tests/WindowsDevInspector.Remediation.Tests`

## Repository Structure

```text
WindowsDevInspector.sln
├─ src
│  ├─ WindowsDevInspector.App
│  ├─ WindowsDevInspector.Core
│  ├─ WindowsDevInspector.Windows
│  ├─ WindowsDevInspector.Remediation
│  └─ WindowsDevInspector.ElevatedWorker
├─ tests
│  ├─ WindowsDevInspector.Core.Tests
│  ├─ WindowsDevInspector.App.Tests
│  ├─ WindowsDevInspector.Windows.Tests
│  └─ WindowsDevInspector.Remediation.Tests
├─ docs
└─ scripts
```

## Architecture Pattern

The intended direction is MVVM-oriented WPF with strict project boundaries.

Current implementation note: `WindowsDevInspector.App/MainWindow.xaml.cs` handles WPF events and UI state, while scan execution, report export, remediation execution, elevated worker launch, rollback, and backup listing are delegated to App-layer services. A fuller ViewModel split remains a future task.

## Dependency Direction

- `Core` must not depend on WPF or Windows-specific APIs.
- `Windows` depends on `Core` and implements read-only environment checks.
- `Remediation` depends on `Core` for risk and remediation concepts.
- `ElevatedWorker` depends on `Remediation` and performs privileged approved changes.
- `App` references the other projects for UI orchestration.

## Coding Conventions

- File-scoped namespaces.
- Nullable reference types enabled.
- Implicit usings enabled.
- Prefer immutable records or init-only properties where practical.
- Async methods use the `Async` suffix.
- External process calls must set a timeout.
- Do not use broad catch blocks that silently ignore failures.
- Keep changes small and reviewable.

## Security Requirements

- Do not disable Defender, Smart App Control, or UAC.
- Do not accept arbitrary PowerShell strings from UI.
- Do not use `cmd /c` with UI-composed commands.
- Do not execute remote downloaded scripts.
- Do not modify arbitrary ACLs.
- Do not log tokens, passwords, cookies, private keys, or connection strings.
- Do not automatically delete unknown PATH entries.
- Do not modify registry values without backup.
- All remediation must use approved remediation IDs.
- Automatic encrypted backup and rollback apply to all currently approved remediation IDs. Registry DWORD rollback goes through `ElevatedWorker`; directory rollback is handled in App-layer remediation code and deletes only tool-created directories that are still empty.

## Build Commands

Preferred restore command for this repository:

```powershell
dotnet restore WindowsDevInspector.sln --configfile NuGet.Config
```

## Test Commands

After restore and build:

```powershell
dotnet test WindowsDevInspector.sln --no-build
```

## Run Commands

```powershell
dotnet run --project src\WindowsDevInspector.App\WindowsDevInspector.App.csproj
```

Running the WPF app may lock build outputs. Close the app before build/test if copy lock errors such as `MSB3027` or `MSB3021` occur.

## Lint and Format Commands

No separate lint or format command is currently documented. Build enforces code style through `EnforceCodeStyleInBuild`.

## Important Entry Points

- `src/WindowsDevInspector.App/MainWindow.xaml`
- `src/WindowsDevInspector.App/MainWindow.xaml.cs`
- `src/WindowsDevInspector.App/EnvironmentScanService.cs`
- `src/WindowsDevInspector.App/RemediationCoordinator.cs`
- `src/WindowsDevInspector.App/ScanReportExporter.cs`
- `src/WindowsDevInspector.App/TechnologySelectionConfig.cs`
- `src/WindowsDevInspector.App/TechnologySelectionToggle.cs`
- `src/WindowsDevInspector.Core/BuiltInCheckCatalog.cs`
- `src/WindowsDevInspector.Core/CheckCatalog.cs`
- `src/WindowsDevInspector.Core/CheckResultSorter.cs`
- `src/WindowsDevInspector.Core/EnvironmentScoreCalculator.cs`
- `src/WindowsDevInspector.Windows/BuiltInEnvironmentCheckFactory.cs`
- `src/WindowsDevInspector.Windows/ProcessCommandRunner.cs`
- `src/WindowsDevInspector.Remediation/BuiltInRemediationCatalog.cs`
- `src/WindowsDevInspector.Remediation/ChangePlanValidator.cs`
- `src/WindowsDevInspector.ElevatedWorker/Program.cs`
- `tests/WindowsDevInspector.App.Tests/EnvironmentScanServiceTests.cs`
- `tests/WindowsDevInspector.App.Tests/ScanReportExporterTests.cs`
- `tests/WindowsDevInspector.App.Tests/TechnologySelectionConfigStoreTests.cs`
- `tests/WindowsDevInspector.App.Tests/TechnologySelectionToggleTests.cs`

## Important Files

- `AGENTS.md`: repository rules.
- `README.md`: user-facing project overview.
- `docs/PROJECT_SPEC.md`: product and architecture specification.
- `docs/CHECK_CATALOG.md`: working check inventory.
- `docs/ENVIRONMENT_PROFILES.md`: technology catalog and grouping source.
- `docs/MVP_TECHNOLOGY_SCOPE.md`: selected first-version MVP technology list and priority backlog.
- `docs/ROADMAP.md`: phase progress.
- `docs/CODEX_DEVELOPMENT_SETUP.md`: local Codex and Windows setup guidance.

## Known Technical Constraints

- Repository documentation and UI text contain Traditional Chinese and are stored as UTF-8.
- When inspecting text files in Windows PowerShell, always use explicit UTF-8 reading, for example `Get-Content -Raw -Encoding UTF8 <path>`.
- Do not treat mojibake from a tool or terminal as file corruption until the same file has been re-read with explicit UTF-8.
- `scripts/Use-Utf8.ps1` configures the current PowerShell process for UTF-8 output and should be used before longer local verification sessions.
- `wsl --status` output may be UTF-16LE without BOM; command output decoding must handle raw bytes.
- `WindowsDevInspector.App` must not directly mutate registry, PATH, Windows features, or install software.
- Elevated changes must stay inside `WindowsDevInspector.ElevatedWorker`.
- `EnvironmentScanService` runs executable checks with bounded concurrency. The WPF UI exposes a scan concurrency dropdown from 1 through `Environment.ProcessorCount`; the default leaves one processor available.
- PowerShell checks should not compare against the latest version. `desktop.powershell` checks Windows PowerShell availability, and `common.powershell7` checks `pwsh` availability; version output is informational current value only.
- Chocolatey is represented by `chocolatey` -> `common.chocolatey`, implemented with `choco --version`. Missing Chocolatey should be Info, not Warning.
- NuGet source diagnostics must redact credentials, tokens, passwords, API keys, and credential-bearing URLs before putting command output into `CheckResult`.
- Visual Studio Build Tools diagnostics use `vswhere.exe` read-only detection and must not invoke VS Installer repair or installation behavior.
- Browser availability diagnostics must not launch browsers or inspect user profiles, cookies, or browser data.
- Android SDK diagnostics may inspect `ANDROID_HOME`, `ANDROID_SDK_ROOT`, common SDK paths, and SDK tool file presence; they must not modify environment variables.
- .NET MAUI diagnostics use `dotnet workload list` read-only output and must not install workloads or modify dotnet configuration.
- Current known validation count is 164 passing tests after all current Core catalog check IDs were backed by executable checks.
- A running `WindowsDevInspector.App` can produce MSB3026/MSB3027/MSB3021 copy-lock warnings during build. If this happens, close the app and rebuild before claiming a clean 0-warning build.

## Prohibited Changes

- Do not rewrite the solution from scratch.
- Do not introduce commercial dependencies without approval.
- Do not broaden remediation to arbitrary commands, registry paths, executables, scripts, or ACL changes.
- Do not weaken tests or analyzers merely to pass validation.

## Documentation Index

- Project overview: `docs/PROJECT.md`.
- AI technical context: `docs/AI_CONTEXT.md`.
- Current work and backlog: `docs/TASK.md`.
- Important decisions: `docs/DECISION.md`.
- Detailed product spec: `docs/PROJECT_SPEC.md`.
- Check inventory: `docs/CHECK_CATALOG.md`.
- Technology catalog and grouping source: `docs/ENVIRONMENT_PROFILES.md`.
- MVP technology scope: `docs/MVP_TECHNOLOGY_SCOPE.md`.
- Roadmap: `docs/ROADMAP.md`.
