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
- Read-only WSL and Docker diagnostics with explicit status/version/distribution/engine/service summaries.
- Read-only advanced Windows diagnostics for firewall profile state, localhost bind health, recent Code Integrity events, and Smart App Control policy state.

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
- Controlled package installation accepts exactly one typed package/source/action item from `BuiltInInstallationCatalog`; execution metadata is catalog-owned and unknown JSON fields are rejected.
- App-layer selection is preview-only. A separate red execution button and second confirmation launch ElevatedWorker `--install`; App never invokes winget directly.
- The WPF result list uses one `操作` column: remediation rows show `修正`, while package candidates show red `安裝`. Confirming one installation candidate clears and disables remediation selection plus other package candidates; clearing installation mode re-enables them. Cancelling the warning preserves existing remediation selections.
- Debug builds inject one simulated remediation row and one simulated installation row for UI smoke testing. Both are excluded from real execution; Release builds do not insert the rows.
- ElevatedWorker reconstructs the exact approved winget tokens, accepts package/source agreements, enforces a five-minute timeout, and kills the process tree on timeout/cancellation. Silent, override, forced scope/version, and automatic rollback are excluded.
- After winget succeeds, ElevatedWorker refreshes only its own process PATH from current machine/user values and runs the catalog-owned verification executable/arguments with a 30-second timeout. A failed or timed-out verification makes the structured overall result unsuccessful without exposing command stdout/stderr.
- App shows a real installation action only when the package's current-scan exact winget availability check is PASS; missing winget, unresolved IDs, failures, and timeouts hide it.
- The unsigned WiX v5 installer packages only WindowsDevInspector output. It does not bundle third-party tools or runtime installers, and generated MSI/CAB files remain ignored build artifacts.
- Deployment is framework-dependent. WiX requires .NET 10 Desktop Runtime x64 before first installation, while every App scan runs the same Common prerequisite and disables all modifying actions when the result is missing or non-PASS.
- Installer authoring includes only root framework-dependent output; MSI validation caps payload count and rejects runtime host files to prevent stale self-contained publish output from entering the MSI.
- CI restores/builds the WiX project on `windows-latest`, validates MSI contents without uploading the unsigned MSI, and runs only for pushes/PRs on `main` and `mvp`.
- The installer project and solution configurations are pinned to x64; MSI database validation rejects non-x64 Template Summary values.
- Installer version is build-parameter driven (`WdiProductVersion`, default 0.1.0). Real lifecycle validation passed for install, 0.1.0→0.1.1 Major Upgrade, downgrade rejection, uninstall, failure rollback, and LocalAppData preservation.
- MSI repair requires the exact original package source. Versioned release MSI artifacts must be immutable and retained outside Git; rebuilding an MSI at the same path can change PackageCode and break repair source resolution.
- GitHub Releases is the canonical MSI artifact store. A `vMAJOR.MINOR.PATCH` tag reachable from `main` or `mvp` builds a version-matched unsigned prerelease plus SHA-256 file; the workflow refuses existing releases and never overwrites assets. A failed pre-publication run may be retried with the explicit manual tag input without moving the existing tag.
- Repository-level GitHub Immutable releases is enabled for `weme1105/WindowsDevInspector`; published release tags and assets cannot be modified or deleted.
- `MainWindowActionState` centrally derives modifying-control availability from Runtime readiness, busy state, installation mode, executable selection, and backup availability; workflow `finally` blocks must never directly re-enable controls.
- ElevatedWorker CLI parsing/routing is typed through `WorkerCommandParser` and `WorkerCommandRouter`; remediation, `--rollback`, and `--install` contracts and exit-code behavior remain compatible.

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

## CI/CD

GitHub Actions uses `.github/workflows/build.yml`.

CI should run on:

- Pushes to `main`.
- Pushes to `mvp`.
- Pull requests targeting `main`.
- Pull requests targeting `mvp`.

Feature branch pushes should not run CI by default. A feature branch is validated when it opens or updates a pull request into `mvp` or `main`.

## Branching Workflow

- `main` or `master` is the stable baseline.
- `mvp` is an integration branch created from the stable baseline for MVP development.
- Future production release branches should use `prd/<version>` from the stable baseline.
- New feature work must branch from the current integration branch, for example `mvp` or `prd/<version>`, and open PRs back to that same integration branch.
- After a feature PR is merged, do not continue new work on that merged feature branch. Start a new feature branch from the current integration branch instead.
- When MVP is complete, open a PR from `mvp` back to `main` or `master`.
- When a PRD release branch is ready to update the product, open a PR from `prd/<version>` back to `main` or `master`.
- If commits are accidentally made on an already merged feature branch, move them to a new feature branch from the current integration branch, normally with `git cherry-pick`, rather than reopening or continuing the merged branch.

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
- `src/WindowsDevInspector.App/PackageInstallationCandidateSelector.cs`
- `src/WindowsDevInspector.App/PackageInstallationConfirmationBuilder.cs`
- `src/WindowsDevInspector.App/PackageInstallationCoordinator.cs`
- `src/WindowsDevInspector.App/PackageInstallationSelection.cs`
- `src/WindowsDevInspector.Core/BuiltInCheckCatalog.cs`
- `src/WindowsDevInspector.Core/CheckCatalog.cs`
- `src/WindowsDevInspector.Core/CheckResultSorter.cs`
- `src/WindowsDevInspector.Core/EnvironmentScoreCalculator.cs`
- `src/WindowsDevInspector.Windows/BuiltInEnvironmentCheckFactory.cs`
- `src/WindowsDevInspector.Windows/ProcessCommandRunner.cs`
- `src/WindowsDevInspector.Remediation/BuiltInRemediationCatalog.cs`
- `src/WindowsDevInspector.Remediation/ChangePlanValidator.cs`
- `src/WindowsDevInspector.Remediation/InstallationPlanValidator.cs`
- `src/WindowsDevInspector.Remediation/BuiltInInstallationCatalog.cs`
- `src/WindowsDevInspector.Remediation/PackageInstallationCommandPreviewBuilder.cs`
- `src/WindowsDevInspector.Remediation/PackageInstallationExecutor.cs`
- `src/WindowsDevInspector.Remediation/DisabledPackageInstallationProcessRunner.cs`
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
- `docs/PRIVACY.md`: MVP privacy and local-data handling statement.
- `docs/RELEASE_NOTES.md`: MVP release notes and known limitations.
- `docs/UI_RULES.md`: UI behavior, smoke-test checklist, screenshot guidance, and UI automation direction.
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
- WSL diagnostics use `wsl --status`, `wsl --version`, and `wsl --list --verbose` through `ICommandRunner`; these commands must remain read-only and should not install distributions or modify WSL configuration.
- Docker diagnostics use `docker --version`, `docker info`, and read-only Windows service status checks; they must not start Docker Desktop, change services, or modify networking.
- Firewall diagnostics use read-only `netsh advfirewall show allprofiles state`; they must not enable, disable, or rewrite firewall policy.
- localhost bind diagnostics may open a short-lived ephemeral listener on `127.0.0.1` through `ILocalhostBindProbe`, then close it immediately; they must not reserve fixed ports or modify firewall/network configuration.
- Code Integrity diagnostics use read-only `wevtutil` queries against `Microsoft-Windows-CodeIntegrity/Operational`; they must not change event log channels or Windows security policy.
- Smart App Control diagnostics inspect `HKLM\SYSTEM\CurrentControlSet\Control\CI\Policy\VerifiedAndReputablePolicyState` read-only when present; they must never disable or toggle Smart App Control.
- Current known validation count is 236 passing Release tests after the fail-closed executor and shared command-preview tests.
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
- Privacy statement: `docs/PRIVACY.md`.
- Release notes: `docs/RELEASE_NOTES.md`.
- UI behavior, smoke tests, and screenshot guidance: `docs/UI_RULES.md`.
- Roadmap: `docs/ROADMAP.md`.
