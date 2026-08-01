# Tasks

## Current Objective

Make the current repository state recoverable across AI sessions by maintaining the standard project-memory documents and then continue small vertical slices for diagnostics and remediation.

## In Progress

No active implementation task.

## Ready

- [ ] Expand WSL and Docker diagnostics.
  - Completion criteria: WSL, Virtual Machine Platform, Hyper-V, Docker service, WinNAT, and HNS checks report clear current/expected/impact values.
  - Related files: `src/WindowsDevInspector.Windows`, `src/WindowsDevInspector.Core/BuiltInCheckCatalog.cs`, `tests/WindowsDevInspector.Windows.Tests`.
  - Verification: unit tests with fakes plus manual read-only scan on Windows.

## Blocked

- [ ] Installer, code signing, and auto update.
  - Blocker: distribution strategy and signing certificate are not decided.
  - Required decision: packaging and release strategy.

## Completed

- [x] Add reversible backup behavior for every approved remediation.
  - Completed: local directory fixes now write encrypted backup records, directory rollback removes only tool-created directories that are still empty, registry rollback still goes through ElevatedWorker, elevated rollback process launching is testable through an injectable worker runner, and missing rollback result files are surfaced as worker failures.
  - Related files: `src/WindowsDevInspector.App/RemediationCoordinator.cs`, `src/WindowsDevInspector.App/IWorkerProcessRunner.cs`, `src/WindowsDevInspector.App/WorkerProcessRunner.cs`, `src/WindowsDevInspector.Remediation/DirectoryBackup.cs`, `src/WindowsDevInspector.Remediation/DirectoryRollbackExecutor.cs`, `src/WindowsDevInspector.Remediation/BackupFileService.cs`, `src/WindowsDevInspector.Remediation/DpapiBackupProtector.cs`, `src/WindowsDevInspector.Remediation/MachineFingerprint.cs`, `src/WindowsDevInspector.Remediation/BuiltInRemediationCatalog.cs`, `src/WindowsDevInspector.Windows/DirectoryExistsCheck.cs`, `tests/WindowsDevInspector.App.Tests/RemediationCoordinatorTests.cs`, `tests/WindowsDevInspector.Remediation.Tests/DirectoryRollbackExecutorTests.cs`, `tests/WindowsDevInspector.Windows.Tests/DirectoryExistsCheckTests.cs`, `docs/AI_CONTEXT.md`, `docs/UI_SMOKE_TESTS.md`.
  - Verification: `dotnet build WindowsDevInspector.sln --configuration Release --no-restore`; `dotnet test WindowsDevInspector.sln --configuration Release --no-build`.

- [x] Add Windows baseline checks for version and processor architecture.
  - Completed: Common baseline now includes executable read-only checks for Windows version/build and processor architecture.
  - Related files: `src/WindowsDevInspector.Core/BuiltInCheckCatalog.cs`, `src/WindowsDevInspector.Windows/CommonCheckFactory.cs`, `src/WindowsDevInspector.Windows/WindowsVersionCheck.cs`, `src/WindowsDevInspector.Windows/ProcessorArchitectureCheck.cs`, `tests/WindowsDevInspector.Windows.Tests/WindowsVersionCheckTests.cs`, `tests/WindowsDevInspector.Windows.Tests/ProcessorArchitectureCheckTests.cs`, `tests/WindowsDevInspector.Windows.Tests/BuiltInEnvironmentCheckFactoryTests.cs`.
  - Verification: `dotnet build WindowsDevInspector.sln --configuration Release --no-restore`; `dotnet test WindowsDevInspector.sln --configuration Release --no-build`.

- [x] Complete first-pass result remediation selection behavior.
  - Completed: PASS and unsupported results cannot be selected for remediation, result detail shows compact fixability/risk/elevation/restart/Rollback/remediation context, batch selection only checks low-risk supported non-pass fixes, "修正勾選項目" shows UAC/backup/restart/Rollback/whitelist confirmation before executing selected fixes, and backup restore is disabled when no backup exists and confirms before elevated rollback.
  - Related files: `src/WindowsDevInspector.App/CheckResultRow.cs`, `src/WindowsDevInspector.App/ResultFixSelection.cs`, `src/WindowsDevInspector.App/MainWindow.xaml`, `src/WindowsDevInspector.App/MainWindow.xaml.cs`, `tests/WindowsDevInspector.App.Tests/ResultFixSelectionTests.cs`, `docs/UI_SMOKE_TESTS.md`.
  - Verification: `dotnet build WindowsDevInspector.sln --no-restore`; `dotnet test WindowsDevInspector.sln --no-build`.

- [x] Add Chocolatey CLI availability check.
  - Completed: added `chocolatey` to the WPF technology picker and Core catalog, backed by a read-only `choco --version` check. Missing Chocolatey reports Info because it is an optional package manager, and detected versions are informational only.
  - Related files: `src/WindowsDevInspector.App/TechnologyCatalog.cs`, `src/WindowsDevInspector.Core/BuiltInCheckCatalog.cs`, `src/WindowsDevInspector.Windows/CommonCheckFactory.cs`, `src/WindowsDevInspector.Windows/CommandVersionCheck.cs`, `tests/WindowsDevInspector.App.Tests/TechnologyCatalogTests.cs`, `tests/WindowsDevInspector.Core.Tests/CheckCatalogTests.cs`, `tests/WindowsDevInspector.Windows.Tests/BuiltInEnvironmentCheckFactoryTests.cs`, `tests/WindowsDevInspector.Windows.Tests/CommandVersionCheckTests.cs`, `docs/CHECK_CATALOG.md`, `docs/MVP_TECHNOLOGY_SCOPE.md`.
  - Verification: `dotnet build WindowsDevInspector.sln --no-restore`; `dotnet test WindowsDevInspector.sln --no-build`.

- [x] Align PowerShell checks with no-latest-version policy.
  - Completed: PowerShell 7 now maps only to the `pwsh` CLI availability check, and no longer adds the PowerShell 7 winget package planning check. Windows PowerShell and PowerShell 7 may display current version output as context, but do not compare against latest versions.
  - Related files: `src/WindowsDevInspector.Core/BuiltInCheckCatalog.cs`, `src/WindowsDevInspector.Windows/BuiltInEnvironmentCheckFactory.cs`, `tests/WindowsDevInspector.Core.Tests/CheckCatalogTests.cs`, `tests/WindowsDevInspector.Windows.Tests/BuiltInEnvironmentCheckFactoryTests.cs`, `docs/CHECK_CATALOG.md`, `docs/ROADMAP.md`.
  - Verification: `dotnet build WindowsDevInspector.sln --no-restore`; `dotnet test WindowsDevInspector.sln --no-build`.

- [x] Speed up scan execution with bounded concurrency.
  - Completed: `EnvironmentScanService` now runs executable checks concurrently, and the WPF UI exposes a scan concurrency dropdown from 1 through the machine processor count. The default leaves one processor available.
  - Related files: `src/WindowsDevInspector.App/MainWindow.xaml`, `src/WindowsDevInspector.App/MainWindow.xaml.cs`, `src/WindowsDevInspector.App/EnvironmentScanService.cs`, `src/WindowsDevInspector.App/ScanConcurrencySettings.cs`, `tests/WindowsDevInspector.App.Tests/EnvironmentScanServiceTests.cs`, `tests/WindowsDevInspector.App.Tests/ScanConcurrencySettingsTests.cs`.
  - Verification: `dotnet build WindowsDevInspector.sln --no-restore`; `dotnet test WindowsDevInspector.sln --no-build`.

- [x] Add select-all and clear-all toggle for technology selection.
  - Completed: replaced the one-way clear-all action with a toggle button that selects all visible technologies when not all are selected and clears all visible technologies when all are selected.
  - Related files: `src/WindowsDevInspector.App/MainWindow.xaml`, `src/WindowsDevInspector.App/MainWindow.xaml.cs`, `src/WindowsDevInspector.App/TechnologySelectionToggle.cs`, `tests/WindowsDevInspector.App.Tests/TechnologySelectionToggleTests.cs`, `docs/UI_SMOKE_TESTS.md`.
  - Verification: `dotnet build WindowsDevInspector.sln --no-restore`; `dotnet test WindowsDevInspector.sln --no-build`.

- [x] Add Core mappings for selected language/runtime backlog technologies.
  - Completed: .NET Desktop Runtime, PowerShell, Flask, pytest, PHP, Rails, and Rust now resolve to concrete runtime or CLI checks instead of being ignored by the check catalog.
  - Related files: `src/WindowsDevInspector.Core/BuiltInCheckCatalog.cs`, `src/WindowsDevInspector.Windows/BuiltInEnvironmentCheckFactory.cs`, `tests/WindowsDevInspector.Core.Tests/CheckCatalogTests.cs`, `tests/WindowsDevInspector.Windows.Tests/BuiltInEnvironmentCheckFactoryTests.cs`, `docs/CHECK_CATALOG.md`, `docs/MVP_TECHNOLOGY_SCOPE.md`.
  - Verification: `dotnet build WindowsDevInspector.sln --no-restore`; `dotnet test WindowsDevInspector.sln --no-build`.

- [x] Add Core mappings for selected tool backlog technologies.
  - Completed: GitHub CLI, Google Cloud CLI, Windows Terminal, and SSMS now resolve to concrete checks instead of being ignored by the check catalog.
  - Related files: `src/WindowsDevInspector.Core/BuiltInCheckCatalog.cs`, `src/WindowsDevInspector.Windows/BuiltInEnvironmentCheckFactory.cs`, `tests/WindowsDevInspector.Core.Tests/CheckCatalogTests.cs`, `tests/WindowsDevInspector.Windows.Tests/BuiltInEnvironmentCheckFactoryTests.cs`, `docs/CHECK_CATALOG.md`, `docs/MVP_TECHNOLOGY_SCOPE.md`.
  - Verification: `dotnet build WindowsDevInspector.sln --no-restore`; `dotnet test WindowsDevInspector.sln --no-build`.

- [x] Add Core mappings for selected frontend backlog technologies.
  - Completed: React, Next.js, and Tailwind CSS now resolve to Node.js and npm checks instead of being ignored by the check catalog.
  - Related files: `src/WindowsDevInspector.Core/BuiltInCheckCatalog.cs`, `tests/WindowsDevInspector.Core.Tests/CheckCatalogTests.cs`, `docs/CHECK_CATALOG.md`, `docs/MVP_TECHNOLOGY_SCOPE.md`.
  - Verification: `dotnet build WindowsDevInspector.sln --no-restore`; `dotnet test WindowsDevInspector.sln --no-build`.

- [x] Add Node.js CLI executable check.
  - Completed: `frontend.node-cli` now runs `node --version`, improving Node.js, Angular, and Vue diagnostics.
  - Related files: `src/WindowsDevInspector.Windows/BuiltInEnvironmentCheckFactory.cs`, `tests/WindowsDevInspector.Windows.Tests/BuiltInEnvironmentCheckFactoryTests.cs`, `docs/CHECK_CATALOG.md`, `docs/MVP_TECHNOLOGY_SCOPE.md`.
  - Verification: `dotnet build WindowsDevInspector.sln --no-restore`; `dotnet test WindowsDevInspector.sln --no-build`.

- [x] Restrict the WPF technology picker to the selected MVP scope.
  - Completed: `TechnologyCatalog` now displays the 44 MVP included technologies plus the 23 selected priority backlog technologies; priority backlog items are visible but not selected by default.
  - Related files: `src/WindowsDevInspector.App/TechnologyCatalog.cs`, `tests/WindowsDevInspector.App.Tests/TechnologyCatalogTests.cs`, `docs/MVP_TECHNOLOGY_SCOPE.md`.
  - Verification: `dotnet build WindowsDevInspector.sln --no-restore`; `dotnet test WindowsDevInspector.sln --no-build`.

- [x] Select first-version MVP technology scope.
  - Completed: captured 44 implemented MVP technologies and 23 priority backlog technologies selected from the HTML review artifact.
  - Related files: `docs/MVP_TECHNOLOGY_SCOPE.md`, `artifacts/mvp-technology-selection.html`.
  - Verification: documentation review.

- [x] Add .NET runtime family checks for C# desktop and ASP.NET workflows.
  - Completed: added read-only `dotnet --list-runtimes` checks for ASP.NET Core runtime and .NET Desktop Runtime.
  - Related files: `src/WindowsDevInspector.Windows/DotNetRuntimeCheck.cs`, `src/WindowsDevInspector.Windows/BuiltInEnvironmentCheckFactory.cs`, `tests/WindowsDevInspector.Windows.Tests/DotNetRuntimeCheckTests.cs`, `docs/CHECK_CATALOG.md`.
  - Verification: `dotnet build WindowsDevInspector.sln --no-restore`; `dotnet test WindowsDevInspector.sln --no-build`.

- [x] Expand Phase 4 tool installation planning without executing installs.
  - Completed: added read-only winget package availability checks for pnpm, Azure CLI, kubectl, and Terraform; documented that package checks do not install or create remediation plans.
  - Related files: `docs/CHECK_CATALOG.md`, `docs/ROADMAP.md`, `docs/DECISION.md`, `src/WindowsDevInspector.Core/BuiltInCheckCatalog.cs`, `src/WindowsDevInspector.Windows/BuiltInEnvironmentCheckFactory.cs`, `src/WindowsDevInspector.Windows/WingetPackageAvailabilityCheck.cs`, `tests/WindowsDevInspector.Windows.Tests/WingetPackageAvailabilityCheckTests.cs`.
  - Verification: `dotnet build WindowsDevInspector.sln --no-restore`; `dotnet test WindowsDevInspector.sln --no-build`.

- [x] Align the WPF UI with technology-only selection wording.
  - Completed: clarified that category headings are browsing aids, kept existing default suggested technologies, and added a clear-all selection action for users who want to start from an empty technology set.
  - Related files: `src/WindowsDevInspector.App/MainWindow.xaml`, `src/WindowsDevInspector.App/MainWindow.xaml.cs`, `docs/UI_SMOKE_TESTS.md`.
  - Verification: `dotnet build WindowsDevInspector.sln --no-restore` and `dotnet test WindowsDevInspector.sln --no-build`.

- [x] Decide whether role selection is still a separate UI concept or represented only by technology groups.
  - Completed: accepted explicit technology selection as the source of truth and removed independent role selection from the product model.
  - Related files: `docs/DECISION.md`, `docs/PROJECT_SPEC.md`, `docs/ENVIRONMENT_PROFILES.md`, `README.md`, `AGENTS.md`, `docs/AI_CONTEXT.md`, `docs/PROJECT.md`.
  - Verification: documentation review; build/test before publish.

- [x] Establish branch workflow with `mvp` as the integration branch.
  - Completed: retargeted the current draft PR to `mvp` and documented the branch model in `docs/DECISION.md`.
  - Related files: `docs/DECISION.md`.
  - Verification: PR #1 base is `mvp`; branch refs checked with git/GitHub.

- [x] Standardize common notes directory on `D:\Note`.
  - Completed: updated check ID, remediation ID, approved directory target, check catalog, setup guidance, project spec, and decision record to use `D:\Note`.
  - Related files: `AGENTS.md`, `docs/PROJECT_SPEC.md`, `docs/CHECK_CATALOG.md`, `docs/ENVIRONMENT_PROFILES.md`, `docs/CODEX_DEVELOPMENT_SETUP.md`, `docs/DECISION.md`, `src/WindowsDevInspector.Core/BuiltInCheckCatalog.cs`, `src/WindowsDevInspector.Windows/CommonCheckFactory.cs`, `src/WindowsDevInspector.Remediation/BuiltInRemediationCatalog.cs`, `src/WindowsDevInspector.Remediation/DirectoryRemediationExecutor.cs`.
  - Verification: `dotnet build WindowsDevInspector.sln --no-restore` and `dotnet test WindowsDevInspector.sln --no-build`.

- [x] Reconcile README scope wording with the now-completed remediation, backup, rollback, and scoring slices.
  - Completed: updated README scope, architecture wording, test project list, and documentation links to reflect current implementation.
  - Related files: `README.md`, `docs/UI_SMOKE_TESTS.md`.
  - Verification: `dotnet build WindowsDevInspector.sln --no-restore` and `dotnet test WindowsDevInspector.sln --no-build`.

- [x] Add UI smoke-test checklist or automation strategy for WPF behavior.
  - Completed: added a manual WPF UI smoke-test checklist with safety checks, exit criteria, and a staged automation strategy.
  - Related files: `docs/UI_SMOKE_TESTS.md`.
  - Verification: documentation review.

- [x] Create .NET 10 WPF solution structure.
  - Completed: source and test project layout exists.
  - Verification: represented in solution and current project files.

- [x] Implement common read-only environment checks.
  - Completed: directory, registry DWORD, PATH, command version, service, optional feature, and file checks exist.
  - Verification: covered by Core and Windows tests.

- [x] Implement technology catalog resolution.
  - Completed: selected technology IDs resolve to deduplicated check IDs.
  - Verification: `CheckCatalogTests`.

- [x] Implement result sorting and detail display.
  - Completed: non-pass items sort before pass items and detail panel exposes full values.
  - Verification: `CheckResultSorterTests` and UI implementation.

- [x] Persist selected technologies.
  - Completed: selections save beside executable and reload at startup.
  - Verification: implemented in `TechnologySelectionConfig`.

- [x] Implement first safe remediation and rollback flow.
  - Completed: directory remediation, registry DWORD remediation, elevated worker validation, encrypted backup files, and rollback exist.
  - Verification: Remediation tests and ElevatedWorker implementation.

- [x] Implement backup browser and scan report export.
  - Completed: backup combo box and JSON report export exist in the app.
  - Verification: recent commit `89f9a0f feat: add backup browser and scan reports`.

- [x] Implement environment scoring.
  - Completed: severity-weighted score calculation and UI display exist.
  - Verification: `EnvironmentScoreCalculatorTests`.

- [x] Record UTF-8 file reading requirement.
  - Completed: `docs/AI_CONTEXT.md` documents that Traditional Chinese repository files are UTF-8 and must be read with explicit UTF-8 in Windows PowerShell.
  - Verification: AGENTS, Roadmap, Codex setup, XAML, and code-behind files were re-read with `Get-Content -Raw -Encoding UTF8`.

- [x] Refactor scan and remediation orchestration out of `MainWindow.xaml.cs`.
  - Completed: scan execution, report export, remediation execution, elevated worker launch, backup listing, and rollback orchestration were moved into App-layer services.
  - Related files: `src/WindowsDevInspector.App/MainWindow.xaml.cs`, `src/WindowsDevInspector.App/EnvironmentScanService.cs`, `src/WindowsDevInspector.App/EnvironmentScanResult.cs`, `src/WindowsDevInspector.App/RemediationCoordinator.cs`, `src/WindowsDevInspector.App/ScanReportExporter.cs`.
  - Verification: `dotnet build WindowsDevInspector.sln --no-restore` and `dotnet test WindowsDevInspector.sln --no-build`.

- [x] Add focused tests for App-layer scan service.
  - Completed: added `WindowsDevInspector.App.Tests` and covered executable check orchestration, pending check fallback, result sorting, scoring, and cancellation token forwarding for `EnvironmentScanService`.
  - Related files: `tests/WindowsDevInspector.App.Tests/WindowsDevInspector.App.Tests.csproj`, `tests/WindowsDevInspector.App.Tests/EnvironmentScanServiceTests.cs`, `WindowsDevInspector.sln`.
  - Verification: `dotnet build WindowsDevInspector.sln --no-restore` and `dotnet test WindowsDevInspector.sln --no-build`.

- [x] Make `ScanReportExporter` testable.
  - Completed: report output directory and timestamp provider are injectable while the default app behavior still writes to LocalAppData.
  - Related files: `src/WindowsDevInspector.App/ScanReportExporter.cs`, `tests/WindowsDevInspector.App.Tests/ScanReportExporterTests.cs`.
  - Verification: `dotnet build WindowsDevInspector.sln --no-restore` and `dotnet test WindowsDevInspector.sln --no-build`.

- [x] Add tests for technology selection configuration persistence.
  - Completed: config path is injectable through `TechnologySelectionConfigStore`, with tests for missing files, malformed JSON, unknown IDs, duplicate IDs, clearing stale selections, and save ordering.
  - Related files: `src/WindowsDevInspector.App/TechnologySelectionConfig.cs`, `tests/WindowsDevInspector.App.Tests/TechnologySelectionConfigStoreTests.cs`.
  - Verification: `dotnet build WindowsDevInspector.sln --no-restore` and `dotnet test WindowsDevInspector.sln --no-build`.

- [x] Expand local ignore rules.
  - Completed: `.gitignore` covers build outputs, coverage outputs, IDE state, NuGet/package folders, publish artifacts, logs, local secrets, Windows noise files, and local Codex state.
  - Related files: `.gitignore`.
  - Verification: `git status --ignored --short` showed build outputs and local tool folders ignored.

- [x] Polish result action layout and PASS filtering.
  - Completed: "修正勾選項目" no longer stretches into neighboring controls, report export moved to the result header, and the result list can hide PASS rows without deleting scan data.
  - Related files: `src/WindowsDevInspector.App/MainWindow.xaml`, `src/WindowsDevInspector.App/MainWindow.xaml.cs`.
  - Verification: manual WPF confirmation by user; `dotnet build WindowsDevInspector.sln --no-restore`; `dotnet test WindowsDevInspector.sln --no-build`.

## Remaining TODO

- [ ] Design approved installation plan schema before enabling any package installation.
- [ ] Add safe NuGet source diagnostics without logging credentials.

## Known Issues

- [ ] Some check IDs are documented but not executable yet; pending checks currently surface informational placeholders.
- [ ] Automated validation does not currently include a WPF UI smoke test.

## Next Recommended Task

Design approved installation plan schema before enabling any package installation.
