# WPF UI Smoke Test Strategy

## Purpose

This checklist verifies the main WPF user flow after changes that affect scanning, result presentation, report export, remediation orchestration, backup browsing, or rollback entry points.

The smoke test is intentionally shallow. It should confirm that the app can start, primary controls remain usable, and dangerous operations still require explicit user confirmation plus the elevated worker boundary.

## When To Run

Run this checklist before merging changes that touch:

- `src/WindowsDevInspector.App/MainWindow.xaml`
- `src/WindowsDevInspector.App/MainWindow.xaml.cs`
- `src/WindowsDevInspector.App/EnvironmentScanService.cs`
- `src/WindowsDevInspector.App/RemediationCoordinator.cs`
- `src/WindowsDevInspector.App/ScanReportExporter.cs`
- Result sorting, scoring, remediation metadata, backup listing, or rollback display logic.

Run automated verification first:

```powershell
dotnet build WindowsDevInspector.sln --no-restore
dotnet test WindowsDevInspector.sln --no-build
```

## Manual Smoke Checklist

Start the app:

```powershell
dotnet run --project src\WindowsDevInspector.App\WindowsDevInspector.App.csproj
```

Then verify:

- The main window opens without an unhandled exception.
- Technology grouping and selection controls are visible and responsive.
- Default suggested technologies are visible and can be manually changed.
- Expanding or viewing a group does not select every technology in that group.
- The selection toggle button shows Select All when at least one technology is unchecked, selects every visible technology when clicked, then changes to Clear All.
- The selection toggle button shows Clear All when every visible technology is checked, clears every visible technology when clicked, then changes to Select All.
- Using the selection toggle does not delete the saved configuration until Save Options is clicked.
- Technology search filters visible items without clearing unrelated saved selections.
- Selecting multiple explicit technologies keeps shared checks deduplicated after scanning.
- Clicking Start Check runs a read-only scan and eventually re-enables the UI.
- Non-pass results appear before pass results.
- Pass results do not show a remediation checkbox.
- Selecting a result updates the detail area with current value, expected value, impact, fixability, risk, restart, elevation, Rollback, and remediation information where available.
- The environment score is visible after a scan and reflects the current result set.
- Batch selection checks only supported low-risk non-pass remediation items and does not execute them immediately.
- Starting remediation processes only currently checked remediation items.
- Starting remediation shows UAC, backup, restart, Rollback, and whitelist context before any local or elevated action is launched.
- Cancelling remediation confirmation leaves the system unchanged and keeps the app usable.
- Scan report export creates a JSON report and shows a clear success or failure message.
- Backup browser loads without crashing when no backup files exist.
- Rollback controls remain unavailable or explain why rollback cannot run when no backup is selected.
- Starting rollback from a selected backup shows UAC, backup validation, approved-target, and rescan context before ElevatedWorker is launched.

## Safety Checks

During smoke testing, do not approve UAC unless the task explicitly requires validating elevated execution on a disposable machine.

The normal app process must not directly:

- Write registry values.
- Modify PATH.
- Install software.
- Enable or disable Windows features.
- Execute arbitrary administrator commands.

If UAC is approved during a dedicated remediation test, verify that the worker accepts only approved remediation IDs and writes a JSON execution result.

## Automation Strategy

Short term:

- Keep UI smoke testing as a documented manual checklist.
- Cover orchestration logic with App-layer unit tests using fakes.
- Keep Core, Windows, and Remediation behavior covered by unit tests.

Medium term:

- Introduce a testable ViewModel layer for selection state, command enablement, result detail projection, scoring display, and remediation selection rules.
- Move more UI decisions out of `MainWindow.xaml.cs` so they can be verified without launching WPF.
- Add focused tests for ViewModel command state and result detail updates.

Long term:

- Evaluate a Windows-only UI automation harness for pre-release builds.
- Prefer smoke tests that run against a test mode with fake scan and remediation services.
- Avoid automation that approves UAC, changes real registry values, installs packages, or mutates PATH on developer machines.

## Exit Criteria

A UI smoke pass means:

- Build and unit tests pass.
- The manual checklist passes for the changed area.
- No privileged operation occurs without explicit confirmation.
- Any skipped checklist item is documented in the task or PR notes with the reason.
