# Tasks

## Current Objective

Make the current repository state recoverable across AI sessions by maintaining the standard project-memory documents and then continue small vertical slices for diagnostics and remediation.

## In Progress

No active implementation task.

## Ready

- [ ] Expand Phase 4 tool installation planning without executing installs.
  - Completion criteria: winget abstraction design is documented and first read-only package availability checks are implemented where safe.
  - Related files: `docs/CHECK_CATALOG.md`, `docs/DECISION.md`, `src/WindowsDevInspector.Windows`, `tests/WindowsDevInspector.Windows.Tests`.
  - Verification: `dotnet build`, `dotnet test`.

- [ ] Expand WSL and Docker diagnostics.
  - Completion criteria: WSL, Virtual Machine Platform, Hyper-V, Docker service, WinNAT, and HNS checks report clear current/expected/impact values.
  - Related files: `src/WindowsDevInspector.Windows`, `src/WindowsDevInspector.Core/BuiltInCheckCatalog.cs`, `tests/WindowsDevInspector.Windows.Tests`.
  - Verification: unit tests with fakes plus manual read-only scan on Windows.

- [ ] Add profile schema design.
  - Completion criteria: JSON profile schema, validation rules, and prohibition against arbitrary commands are documented before external profile loading is implemented.
  - Related files: `docs/DECISION.md`, `docs/ENVIRONMENT_PROFILES.md`, future profile model files.
  - Verification: documentation review; later schema validation tests.

## Blocked

- [ ] Installer, code signing, and auto update.
  - Blocker: distribution strategy and signing certificate are not decided.
  - Required decision: packaging and release strategy.

## Completed

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

## Remaining TODO

- [ ] Add UI smoke-test checklist or automation strategy for WPF behavior.
- [ ] Reconcile README scope wording with the now-completed remediation, backup, rollback, and scoring slices.
- [ ] Decide whether role selection is still a separate UI concept or represented only by technology groups.

## Known Issues

- [ ] `MainWindow.xaml.cs` has grown into a broad orchestration class.
- [ ] Some check IDs are documented but not executable yet; pending checks currently surface informational placeholders.
- [ ] README still describes parts of the implementation as early MVP even though remediation and rollback now exist.
- [ ] Automated validation does not currently include a WPF UI smoke test.

## Next Recommended Task

Add a UI smoke-test checklist or automation strategy for WPF behavior.
